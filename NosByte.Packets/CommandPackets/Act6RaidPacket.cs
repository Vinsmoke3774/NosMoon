using OpenNos.Core;
using OpenNos.Core.Serializing;
using OpenNos.Domain;

namespace NosByte.Packets.CommandPackets
{
    [PacketHeader("$Act6Percent", PassNonParseablePacket = true, Authority = AuthorityType.Administrator)]
    public class Act6RaidPacket : PacketDefinition
    {
        #region Properties

        [PacketIndex(0)]
        public string Name { get; set; }

        [PacketIndex(1)]
        public byte? Percent { get; set; }

        public override string ToString() => "$Act6Percent Name [Percent]";

        #endregion
    }
}