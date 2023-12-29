using NosByte.Packets.ClientPackets.Family;
using OpenNos.Core;
using OpenNos.Core.Logger;
using OpenNos.DAL;
using OpenNos.Data;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;
using OpenNos.Master.Library.Client;
using OpenNos.Master.Library.Data;

namespace OpenNos.Handler.World.Family
{
    public class CmdFamilyNoticePacketHandler : IPacketHandler
    {
        private ClientSession Session { get; set; }

        public CmdFamilyNoticePacketHandler(ClientSession session) => Session = session;

        public void FamilyMessage(FamilyNoticePacket packet)
        {
            if (Session.Character.Family != null && Session.Character.FamilyCharacter != null)
            {
                if (Session.Character.FamilyCharacter.Authority == FamilyAuthority.Familydeputy
                    || (Session.Character.FamilyCharacter.Authority == FamilyAuthority.Familykeeper
                     && Session.Character.Family.ManagerCanShout)
                    || Session.Character.FamilyCharacter.Authority == FamilyAuthority.Head)
                {
                    if (string.IsNullOrEmpty(packet.Data))
                    {
                        Session?.SendPacket(UserInterfaceHelper.GenerateMsg("The Familyhint can't be empty!", 0));
                        return;
                    }

                    Logger.Log.LogUserEvent("GUILDCOMMAND", Session.GenerateIdentity(),
                        $"[FamilyMessage][{Session.Character.Family.FamilyId}]Message: {packet.Data}");

                    
                    Session.Character.Family.FamilyMessage = packet.Data;
                    FamilyDTO fam = Session.Character.Family;
                    DAOFactory.FamilyDAO.InsertOrUpdate(ref fam);
                    ServerManager.Instance.FamilyRefresh(Session.Character.Family.FamilyId);
                    CommunicationServiceClient.Instance.SendMessageToCharacter(new SCSCharacterMessage
                    {
                        DestinationCharacterId = Session.Character.Family.FamilyId,
                        SourceCharacterId = Session.Character.CharacterId,
                        SourceWorldId = ServerManager.Instance.WorldId,
                        Message = "fhis_stc",
                        Type = MessageType.Family
                    });
                    if (!string.IsNullOrWhiteSpace(packet.Data))
                    {
                        CommunicationServiceClient.Instance.SendMessageToCharacter(new SCSCharacterMessage
                        {
                            DestinationCharacterId = Session.Character.Family.FamilyId,
                            SourceCharacterId = Session.Character.CharacterId,
                            SourceWorldId = ServerManager.Instance.WorldId,
                            Message = UserInterfaceHelper.GenerateInfo(
                                "--- Family Message ---\n" + Session.Character.Family.FamilyMessage),
                            Type = MessageType.Family
                        });
                    }
                }
            }
        }
    }
}
