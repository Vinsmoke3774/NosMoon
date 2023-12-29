using NosByte.Packets.ClientPackets.Family;
using OpenNos.Core;
using OpenNos.Core.Logger;
using OpenNos.DAL;
using OpenNos.Data;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Extension.CharacterExtensions;
using OpenNos.GameObject.Networking;
using OpenNos.Master.Library.Client;
using OpenNos.Master.Library.Data;

namespace OpenNos.Handler.World.Family
{
    public class CmdChangeRankPacketHandler : IPacketHandler
    {
        private ClientSession Session { get; set; }

        public CmdChangeRankPacketHandler(ClientSession session) => Session = session;

        public void TitleChange(ChangeRankPacket packet)
        {
            if (Session.Character.Family == null || Session.Character.FamilyCharacter == null ||
                Session.Character.FamilyCharacter.Authority != FamilyAuthority.Head)
            {
                return;
            }

            FamilyCharacterDTO fchar = Session.Character.Family.FamilyCharacters.Find(s => s.Character.Name == packet.Name);
            if (fchar == null)
            {
                return;
            }

            fchar.Rank = packet.Rank;

            Logger.Log.LogUserEvent("GUILDCOMMAND", Session.GenerateIdentity(),
                $"[Title][{Session.Character.Family.FamilyId}]CharacterName: {packet.Name} Title: {fchar.Rank.ToString()}");

            DAOFactory.FamilyCharacterDAO.InsertOrUpdate(ref fchar);
            ServerManager.Instance.FamilyRefresh(Session.Character.Family.FamilyId);
            CommunicationServiceClient.Instance.SendMessageToCharacter(new SCSCharacterMessage
            {
                DestinationCharacterId = Session.Character.Family.FamilyId,
                SourceCharacterId = Session.Character.CharacterId,
                SourceWorldId = ServerManager.Instance.WorldId,
                Message = "fhis_stc",
                Type = MessageType.Family
            });
            Session.SendPacket(Session.Character.GenerateFamilyMember());
            Session.SendPacket(Session.Character.GenerateFamilyMemberMessage());
        }
    }
}
