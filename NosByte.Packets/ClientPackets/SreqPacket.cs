using OpenNos.Core;
using OpenNos.Core.Serializing;

namespace NosByte.Packets.ServerPackets
{
    [PacketHeader("sreq")]
    public class SreqPacket : PacketDefinition
    {
        #region Properties

        public short Argument { get; set; }

        #endregion
    }
}