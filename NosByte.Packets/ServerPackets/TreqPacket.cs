using OpenNos.Core;
using OpenNos.Core.Serializing;

namespace NosByte.Packets.ServerPackets
{
    [PacketHeader("treq")]
    public class TreqPacket : PacketDefinition
    {
        #region Properties


        [PacketIndex(0)]
        public int X { get; set; }

        [PacketIndex(1)]
        public int Y { get; set; }

        [PacketIndex(2)]
        public byte? StartPress { get; set; }

        [PacketIndex(3)]
        public byte? RecordPress { get; set; }

        #endregion
    }
}