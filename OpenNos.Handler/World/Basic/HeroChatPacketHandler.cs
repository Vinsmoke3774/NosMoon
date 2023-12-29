using NosByte.Packets.ClientPackets;
using OpenNos.Core;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Extension.CharacterExtensions;
using OpenNos.GameObject.Networking;

namespace OpenNos.Handler.World.Basic
{
    public class HeroChatPacketHandler : IPacketHandler
    {
        private ClientSession Session { get; }

        public HeroChatPacketHandler(ClientSession session) => Session = session;

        /// <summary>
        /// hero packet
        /// </summary>
        /// <param name="heroPacket"></param>
        public void Hero(HeroPacket heroPacket)
        {
            if (!string.IsNullOrEmpty(heroPacket.Message))
            {
                if (Session.Character.IsReputationHero() >= 3 && Session.Character.Reputation > 5000000 || Session.Account.Authority > AuthorityType.GS)
                {
                    //ServerManager.Instance.ChatLogs.Add(new ChatLogDTO
                    //{
                    //    CharacterId = Session.Character.CharacterId,
                    //    CharacterName = Session.Character.Name,
                    //    DateTime = DateTime.Now,
                    //    MessageType = "Hero",
                    //    Message = heroPacket.Message
                    //});
                    heroPacket.Message = heroPacket.Message.Trim();
                    ServerManager.Instance.Broadcast(Session, $"msg 5 [{Session.Character.Name}]:{heroPacket.Message}",
                        ReceiverType.AllNoHeroBlocked);
                }
                else
                {
                    Session.SendPacket(
                        Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("USER_NOT_HERO"), 11));
                }
            }
        }
    }
}
