using NosByte.Packets.ClientPackets;
using OpenNos.Core;
using OpenNos.Core.Logger;
using OpenNos.DAL;
using OpenNos.Data;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Extension.CharacterExtensions;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;
using System.Linq;

namespace OpenNos.Handler.World.Family
{
    public class FamilyManagementPacketHandler : IPacketHandler
    {
        private ClientSession Session { get; set; }

        public FamilyManagementPacketHandler(ClientSession session) => Session = session;

        /// <summary>
        /// fmg packet
        /// </summary>
        /// <param name="familyManagementPacket"></param>
        public void FamilyManagement(FamilyManagementPacket familyManagementPacket)
        {
            if (Session.Character.Family == null)
            {
                return;
            }

            Logger.Log.LogUserEvent("GUILDMGMT", Session.GenerateIdentity(),
                $"[FamilyManagement][{Session.Character.Family.FamilyId}]TargetId: {familyManagementPacket.TargetId} AuthorityType: {familyManagementPacket.FamilyAuthorityType.ToString()}");

            if (Session.Character.FamilyCharacter.Authority == FamilyAuthority.Member
                || Session.Character.FamilyCharacter.Authority == FamilyAuthority.Familykeeper)
            {
                return;
            }

            long targetId = familyManagementPacket.TargetId;
            if (DAOFactory.FamilyCharacterDAO.LoadByCharacterId(targetId)?.FamilyId
                != Session.Character.FamilyCharacter.FamilyId)
            {
                return;
            }

            FamilyCharacterDTO famChar = DAOFactory.FamilyCharacterDAO.LoadByCharacterId(targetId);
            if (famChar.Authority == familyManagementPacket.FamilyAuthorityType)
            {
                return;
            }
            switch (familyManagementPacket.FamilyAuthorityType)
            {
                case FamilyAuthority.Head:
                    if (Session.Character.FamilyCharacter.Authority != FamilyAuthority.Head)
                    {
                        Session.SendPacket(
                            UserInterfaceHelper.GenerateInfo(Language.Instance.GetMessageFromKey("NOT_FAMILY_HEAD")));
                        return;
                    }

                    if (famChar.Authority != FamilyAuthority.Familydeputy)
                    {
                        Session.SendPacket(
                            UserInterfaceHelper.GenerateInfo(
                                Language.Instance.GetMessageFromKey("ONLY_PROMOTE_ASSISTANT")));
                        return;
                    }

                    famChar.Authority = FamilyAuthority.Head;
                    DAOFactory.FamilyCharacterDAO.InsertOrUpdate(ref famChar);

                    Session.Character.Family.Warehouse.Values.ToList().ForEach(s =>
                    {
                        s.CharacterId = famChar.CharacterId;
                        DAOFactory.ItemInstanceDAO.InsertOrUpdate(s);
                    });
                    Session.Character.FamilyCharacter.Authority = FamilyAuthority.Familydeputy;
                    FamilyCharacterDTO chara2 = Session.Character.FamilyCharacter;
                    DAOFactory.FamilyCharacterDAO.InsertOrUpdate(ref chara2);
                    Session.SendPacket(UserInterfaceHelper.GenerateInfo(Language.Instance.GetMessageFromKey("DONE")));
                    break;

                case FamilyAuthority.Familydeputy:
                    if (Session.Character.FamilyCharacter.Authority != FamilyAuthority.Head)
                    {
                        Session.SendPacket(
                            UserInterfaceHelper.GenerateInfo(Language.Instance.GetMessageFromKey("NOT_FAMILY_HEAD")));
                        return;
                    }

                    if (famChar.Authority == FamilyAuthority.Head)
                    {
                        Session.SendPacket(
                            UserInterfaceHelper.GenerateInfo(Language.Instance.GetMessageFromKey("HEAD_UNDEMOTABLE")));
                        return;
                    }

                    if (Session.Character.Family.DeputyExtension == 2 && DAOFactory.FamilyCharacterDAO.LoadByFamilyId(Session.Character.Family.FamilyId)
                            .Count(s => s.Authority == FamilyAuthority.Familydeputy) == 4)
                    {
                        Session.SendPacket(
                        UserInterfaceHelper.GenerateInfo(
                            Language.Instance.GetMessageFromKey("ALREADY_FOUR_ASSISTANT")));
                        return;
                    }

                    if (Session.Character.Family.DeputyExtension == 1 && DAOFactory.FamilyCharacterDAO.LoadByFamilyId(Session.Character.Family.FamilyId)
                            .Count(s => s.Authority == FamilyAuthority.Familydeputy) == 3)
                    {
                        Session.SendPacket(
                        UserInterfaceHelper.GenerateInfo(
                            Language.Instance.GetMessageFromKey("ALREADY_THREE_ASSISTANT")));
                        return;
                    }

                    if (Session.Character.Family.DeputyExtension == 0 && DAOFactory.FamilyCharacterDAO.LoadByFamilyId(Session.Character.Family.FamilyId)
                            .Count(s => s.Authority == FamilyAuthority.Familydeputy) == 2)
                    {
                        
                        Session.SendPacket(
                            UserInterfaceHelper.GenerateInfo(
                                Language.Instance.GetMessageFromKey("ALREADY_TWO_ASSISTANT")));
                        return;
                    }

                    famChar.Authority = FamilyAuthority.Familydeputy;
                    Session.SendPacket(UserInterfaceHelper.GenerateInfo(Language.Instance.GetMessageFromKey("DONE")));

                    DAOFactory.FamilyCharacterDAO.InsertOrUpdate(ref famChar);
                    break;

                case FamilyAuthority.Familykeeper:
                    if (famChar.Authority == FamilyAuthority.Head)
                    {
                        Session.SendPacket(
                            UserInterfaceHelper.GenerateInfo(Language.Instance.GetMessageFromKey("HEAD_UNDEMOTABLE")));
                        return;
                    }

                    if (famChar.Authority == FamilyAuthority.Familydeputy
                        && Session.Character.FamilyCharacter.Authority != FamilyAuthority.Head)
                    {
                        Session.SendPacket(
                            UserInterfaceHelper.GenerateInfo(
                                Language.Instance.GetMessageFromKey("ASSISTANT_UNDEMOTABLE")));
                        return;
                    }

                    famChar.Authority = FamilyAuthority.Familykeeper;
                    Session.SendPacket(UserInterfaceHelper.GenerateInfo(Language.Instance.GetMessageFromKey("DONE")));
                    DAOFactory.FamilyCharacterDAO.InsertOrUpdate(ref famChar);
                    break;

                case FamilyAuthority.Member:
                    if (famChar.Authority == FamilyAuthority.Head)
                    {
                        Session.SendPacket(
                            UserInterfaceHelper.GenerateInfo(Language.Instance.GetMessageFromKey("HEAD_UNDEMOTABLE")));
                        return;
                    }

                    if (famChar.Authority == FamilyAuthority.Familydeputy
                        && Session.Character.FamilyCharacter.Authority != FamilyAuthority.Head)
                    {
                        Session.SendPacket(
                            UserInterfaceHelper.GenerateInfo(
                                Language.Instance.GetMessageFromKey("ASSISTANT_UNDEMOTABLE")));
                        return;
                    }

                    famChar.Authority = FamilyAuthority.Member;
                    Session.SendPacket(UserInterfaceHelper.GenerateInfo(Language.Instance.GetMessageFromKey("DONE")));

                    DAOFactory.FamilyCharacterDAO.InsertOrUpdate(ref famChar);
                    break;
            }
            CharacterDTO character = DAOFactory.CharacterDAO.LoadById(targetId);
            ClientSession targetSession = ServerManager.Instance.GetSessionByCharacterId(targetId);

            Session.Character.Family.InsertFamilyLog(FamilyLogType.AuthorityChanged, Session.Character.Name,
                character.Name, authority: familyManagementPacket.FamilyAuthorityType);
            targetSession?.CurrentMapInstance?.Broadcast(targetSession?.Character.GenerateGidx());
            if (familyManagementPacket.FamilyAuthorityType == FamilyAuthority.Head)
            {
                Session.CurrentMapInstance?.Broadcast(Session.Character.GenerateGidx());
            }
        }
    }
}
