using NosByte.Packets.ClientPackets;
using OpenNos.Core;
using OpenNos.Core.Logger;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;
using OpenNos.Master.Library.Client;
using OpenNos.Master.Library.Data;

namespace OpenNos.Handler.World.Family
{
    public class FamilyCallPacketHandler : IPacketHandler
    {
        private ClientSession Session { get; set; }

        public FamilyCallPacketHandler(ClientSession session) => Session = session;

        public void FamilyCall(FamilyShoutPacket packet)
        {
            if (Session.Character.Family != null && Session.Character.FamilyCharacter != null)
            {
                if (Session.Character.FamilyCharacter.Authority == FamilyAuthority.Familydeputy
                    || (Session.Character.FamilyCharacter.Authority == FamilyAuthority.Familykeeper
                        && Session.Character.Family.ManagerCanShout)
                    || Session.Character.FamilyCharacter.Authority == FamilyAuthority.Head)
                {

                    Logger.Log.LogUserEvent("GUILDCOMMAND", Session.GenerateIdentity(),
                        $"[FamilyShout][{Session.Character.Family.FamilyId}]Message: {packet.Message}");
                    CommunicationServiceClient.Instance.SendMessageToCharacter(new SCSCharacterMessage
                    {
                        DestinationCharacterId = Session.Character.Family.FamilyId,
                        SourceCharacterId = Session.Character.CharacterId,
                        SourceWorldId = ServerManager.Instance.WorldId,
                        Message = UserInterfaceHelper.GenerateMsg(
                            $"<{Language.Instance.GetMessageFromKey("FAMILYCALL")}{Session.Character.Name}:> {packet.Message}", 0),
                        Type = MessageType.Family
                    });
                }
            }
        }
    }
}
