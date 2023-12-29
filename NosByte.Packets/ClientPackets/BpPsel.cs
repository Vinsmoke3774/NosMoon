using OpenNos.Core;
using OpenNos.Core.Serializing;
using OpenNos.Domain;

namespace NosByte.Packets.ClientPackets
{
    [PacketHeader("bp_psel")]
    public class BpPsel : PacketDefinition
    {
        [PacketIndex(0)]
        public GetBattlePassItemType Type { get; set; }

        [PacketIndex(1)]
        public long Palier { get; set; }
    }
}
