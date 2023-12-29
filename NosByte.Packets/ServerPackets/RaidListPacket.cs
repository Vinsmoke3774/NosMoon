using OpenNos.Core;
using OpenNos.Core.Serializing;

namespace NosByte.Packets.ServerPackets
{
    [PacketHeader("rl")]
    public class RaidListPacket : PacketDefinition
    {
        #region Properties

        [PacketIndex(0)]
        public short MonsterVNum { get; set; }

        #endregion
    }
}