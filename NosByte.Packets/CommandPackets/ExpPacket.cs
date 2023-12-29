using OpenNos.Core;
using OpenNos.Core.Serializing;
using OpenNos.Domain;

namespace NosByte.Packets.CommandPackets
{
    [PacketHeader("$Exp", Authority = AuthorityType.GM, PassNonParseablePacket = true)]
    public class ExpPacket : PacketDefinition
    {
        [PacketIndex(0)]
        public ExpType Type { get; set; }

        [PacketIndex(1)]
        public byte Level { get; set; }

        [PacketIndex(2)]
        public long Experience { get; set; }

        [PacketIndex(3)]
        public bool IsAdventurer { get; set; }

        public static string ReturnHelp() => "$Exp <Type:0=XP:1=JXP:2=HXP> Level Experience";
    }
}
