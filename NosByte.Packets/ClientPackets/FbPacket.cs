using OpenNos.Core;
using OpenNos.Core.Serializing;

namespace NosByte.Packets.ClientPackets
{
    [PacketHeader("fb")]
    public class FbPacket : PacketDefinition
    {
        #region Properties

        [PacketIndex(0, serializeToEnd: true)]
        public string Type { get; set; }

        #endregion
    }
}
