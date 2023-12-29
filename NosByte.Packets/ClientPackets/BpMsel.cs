using OpenNos.Core;
using OpenNos.Core.Serializing;

namespace NosByte.Packets.ClientPackets
{
    [PacketHeader("bp_msel")]
    public class BpMsel : PacketDefinition
    {
        [PacketIndex(0)]
        public long Slot { get; set; }
    }
}
