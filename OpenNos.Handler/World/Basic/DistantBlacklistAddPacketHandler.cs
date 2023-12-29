using NosByte.Packets.ClientPackets;
using OpenNos.Core;
using OpenNos.GameObject;
using OpenNos.GameObject.Networking;

namespace OpenNos.Handler.World.Basic
{
    public class DistantBlacklistAddPacketHandler : IPacketHandler
    {
        private ClientSession Session { get; set; }

        public DistantBlacklistAddPacketHandler(ClientSession session) => Session = session;

        /// <summary>
        /// $bl packet
        /// </summary>
        /// <param name="blPacket"></param>
        public void BlBlacklistAdd(BlPacket blPacket)
        {
            var receiverSession = ServerManager.Instance.GetSessionByCharacterName(blPacket.CharacterName);
            if (blPacket.CharacterName != null && receiverSession != null)
            {
                new BlackListAddPacketHandler(Session).BlacklistAdd(new BlInsPacket { CharacterId = receiverSession.Character.CharacterId });
            }
        }
    }
}
