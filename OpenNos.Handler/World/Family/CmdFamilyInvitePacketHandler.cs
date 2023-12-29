using NosByte.Packets.ClientPackets.Family;
using OpenNos.Core;
using OpenNos.Core.Logger;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;
using System;

namespace OpenNos.Handler.World.Family
{
    public class CmdFamilyInvitePacketHandler : IPacketHandler
    {
        private ClientSession Session { get; set; }

        public CmdFamilyInvitePacketHandler(ClientSession session) => Session = session;

        public void InviteFamily(FamilyInviteCommandPacket packet)
        {
            if (string.IsNullOrEmpty(packet.CharacterName))
            {
                return;
            }

            if (Session.Character.Family == null || Session.Character.FamilyCharacter == null)
            {
                return;
            }

            if (Session.Character.FamilyCharacter.Authority == FamilyAuthority.Member
                || (Session.Character.FamilyCharacter.Authority == FamilyAuthority.Familykeeper
                 && !Session.Character.Family.ManagerCanInvite))
            {
                Session.SendPacket(UserInterfaceHelper.GenerateInfo(
                    string.Format(Language.Instance.GetMessageFromKey("FAMILY_INVITATION_NOT_ALLOWED"))));
                return;
            }

            Logger.Log.LogUserEvent("GUILDCOMMAND", Session.GenerateIdentity(),
                $"[FamilyInvite][{Session.Character.Family.FamilyId}]Message: {packet.CharacterName}");
            ClientSession otherSession = ServerManager.Instance.GetSessionByCharacterName(packet.CharacterName);
            if (otherSession == null)
            {
                Session.SendPacket(
                    UserInterfaceHelper.GenerateInfo(
                        string.Format(Language.Instance.GetMessageFromKey("USER_NOT_FOUND"))));
                return;
            }

            if (otherSession.CurrentMapInstance?.MapInstanceType == MapInstanceType.TalentArenaMapInstance)
            {
                return;
            }

            if (otherSession.Character.FamilyRequestBlocked)
            {
                Session.SendPacket(
                    UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("FAMILY_BLOCKED"),
                        0));
                return;
            }

            if (Session.Character.IsBlockedByCharacter(otherSession.Character.CharacterId))
            {
                Session.SendPacket(
                    UserInterfaceHelper.GenerateInfo(Language.Instance.GetMessageFromKey("BLACKLIST_BLOCKED")));
                return;
            }

            if (Session.Character.Family.FamilyCharacters.Count + 1 > Session.Character.Family.MaxSize)
            {
                Session.SendPacket(
                    UserInterfaceHelper.GenerateInfo(Language.Instance.GetMessageFromKey("FAMILY_FULL")));
                return;
            }

            if (otherSession.Character.Family != null || otherSession.Character.FamilyCharacter != null)
            {
                Session.SendPacket(
                    UserInterfaceHelper.GenerateInfo(Language.Instance.GetMessageFromKey("ALREADY_IN_FAMILY")));
                return;
            }

            if (otherSession.Character.LastFamilyLeave > DateTime.Now.AddDays(-1).Ticks)
            {
                Session.SendPacket(
                    UserInterfaceHelper.GenerateInfo(Language.Instance.GetMessageFromKey("CANT_ENTER_FAMILY")));
                return;
            }
            if (ServerManager.Instance.ChannelId == 51 && otherSession.Character.Faction != Session.Character.Faction)
            {
                return;
            }

            Session.SendPacket(UserInterfaceHelper.GenerateInfo(
                string.Format(Language.Instance.GetMessageFromKey("FAMILY_INVITED"), otherSession.Character.Name)));
            otherSession.SendPacket(UserInterfaceHelper.GenerateDialog(
                $"#gjoin^1^{Session.Character.CharacterId} #gjoin^2^{Session.Character.CharacterId} {string.Format(Language.Instance.GetMessageFromKey("ASK_FAMILY_INVITED"), Session.Character.Family.Name)}"));
            Session.Character.FamilyInviteCharacters.Add(otherSession.Character.CharacterId);
        }
    }
}
