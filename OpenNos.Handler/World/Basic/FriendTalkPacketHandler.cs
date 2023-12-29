using NosByte.Packets.ClientPackets;
using OpenNos.Core;
using OpenNos.DAL;
using OpenNos.Data;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Extension.CharacterExtensions;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;
using OpenNos.Master.Library.Client;
using OpenNos.Master.Library.Data;

namespace OpenNos.Handler.World.Basic
{
    public class FriendTalkPacketHandler : IPacketHandler
    {
        private ClientSession Session { get; }

        public FriendTalkPacketHandler(ClientSession session) => Session = session;

        /// <summary>
        /// btk packet
        /// </summary>
        /// <param name="btkPacket"></param>
        public void FriendTalk(BtkPacket btkPacket)
        {
            if (string.IsNullOrEmpty(btkPacket.Message))
            {
                return;
            }

            if (Session.CurrentMapInstance?.MapInstanceType == MapInstanceType.RainbowBattleInstance ||
                Session.CurrentMapInstance?.MapInstanceType == MapInstanceType.CaligorInstance)
            {
                Session.SendPacket(Session.Character.GenerateSay("You cannot speak to your friends on this map.", 12));
                return;
            }

            string message = btkPacket.Message;
            if (message.Length > 60)
            {
                message = message.Substring(0, 60);
            }

            message = message.Trim();

            CharacterDTO character = DAOFactory.CharacterDAO.LoadById(btkPacket.CharacterId);
            if (character != null)
            {
                //ServerManager.Instance.ChatLogs.Add(new ChatLogDTO
                //{
                //    CharacterId = Session.Character.CharacterId,
                //    CharacterName = Session.Character.Name,
                //    DateTime = DateTime.Now,
                //    DestinationCharacterId = character.CharacterId,
                //    DestinationCharacterName = character.Name,
                //    MessageType = "Friend",
                //    Message = message
                //});

                int? sentChannelId = CommunicationServiceClient.Instance.SendMessageToCharacter(new SCSCharacterMessage
                {
                    DestinationCharacterId = character.CharacterId,
                    SourceCharacterId = Session.Character.CharacterId,
                    SourceWorldId = ServerManager.Instance.WorldId,
                    Message = PacketFactory.Serialize(Session.Character.GenerateTalk(message)),
                    Type = MessageType.PrivateChat
                });

                if (!sentChannelId.HasValue) //character is even offline on different world
                {
                    Session.SendPacket(
                        UserInterfaceHelper.GenerateInfo(Language.Instance.GetMessageFromKey("FRIEND_OFFLINE")));
                }
            }
        }
    }
}
