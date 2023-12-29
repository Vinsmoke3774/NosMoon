using NosByte.Packets.ClientPackets;
using OpenNos.Core;
using OpenNos.GameObject;
using OpenNos.GameObject.Extension.CharacterExtensions;

namespace OpenNos.Handler.World.Basic
{
    public class DirectionPacketHandler : IPacketHandler
    {
        private ClientSession Session { get; set; }

        public DirectionPacketHandler(ClientSession session) => Session = session;

        /// <summary>
        /// dir packet
        /// </summary>
        /// <param name="directionPacket"></param>
        public void Dir(DirectionPacket directionPacket)
        {
            if (directionPacket.CharacterId == Session.Character.CharacterId)
            {
                Session.Character.Direction = directionPacket.Direction;
                Session.CurrentMapInstance?.Broadcast(Session.Character.GenerateDir());
            }
        }
    }
}
