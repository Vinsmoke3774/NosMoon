using OpenNos.Core;
using OpenNos.Core.Serializing;
using OpenNos.Domain;

namespace NosByte.Packets.CommandPackets
{
    [PacketHeader("$RestoreChar", "$RestoreCharacter", Authority = AuthorityType.Administrator, PassNonParseablePacket = true)]
    public class RestoreCharacterPacket : PacketDefinition
    {
        [PacketIndex(0)]
        public string Name { get; set; }

        public static string ReturnHelp() => "$RestoreChar [Name] OR $RestoreCharacter [Name]";
    }
}
