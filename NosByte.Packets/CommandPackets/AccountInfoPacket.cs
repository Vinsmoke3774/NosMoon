using OpenNos.Core;
using OpenNos.Core.Serializing;
using OpenNos.Domain;

namespace NosByte.Packets.CommandPackets
{
    [PacketHeader("$AccountInfo", Authority = AuthorityType.TGS, PassNonParseablePacket = true)]
    public class AccountInfoPacket : PacketDefinition
    {
        [PacketIndex(0, SerializeToEnd = true)]
        public string AccountName { get; set; }

        public static string ReturnHelp() => $"$AccountInfo AccountName";
    }
}
