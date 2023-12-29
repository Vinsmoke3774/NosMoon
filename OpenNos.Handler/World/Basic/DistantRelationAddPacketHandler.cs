using NosByte.Packets.ClientPackets;
using OpenNos.Core;
using OpenNos.GameObject;
using OpenNos.GameObject.Networking;

namespace OpenNos.Handler.World.Basic
{
    public class DistantRelationAddPacketHandler : IPacketHandler
    {
        private ClientSession Session { get; set; }

        public DistantRelationAddPacketHandler(ClientSession session) => Session = session;

        /// <summary>
        /// $fl packet
        /// </summary>
        /// <param name="flPacket"></param>
        public void FlRelationAdd(FlPacket flPacket)
        {
            if (flPacket.CharacterName != null && ServerManager.Instance.GetSessionByCharacterName(flPacket.CharacterName) is ClientSession receiverSession)
            {
                new AddRelationPacketHandler(Session).RelationAdd(new FInsPacket { Type = 1, CharacterId = receiverSession.Character.CharacterId });
            }
        }
    }
}
