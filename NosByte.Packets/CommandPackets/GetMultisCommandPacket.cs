using OpenNos.Core;
using OpenNos.Core.Serializing;
using OpenNos.Domain;

namespace NosByte.Packets.CommandPackets
{
    [PacketHeader("$GetMultis", Authority = AuthorityType.GM, PassNonParseablePacket = true)]
    public class GetMultisCommandPacket : PacketDefinition
    {
        [PacketIndex(0)]
        public string CharacterName { get; set; }

        public static string ReturnHelp() => "$GetMultis [CharacterName]";
    }
}
