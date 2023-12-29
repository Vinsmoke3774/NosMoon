using OpenNos.Core;
using OpenNos.Core.Serializing;
using OpenNos.Domain;

namespace NosByte.Packets.CommandPackets
{
    [PacketHeader("$ClearShell", Authority = AuthorityType.Administrator, PassNonParseablePacket = true)]
    public class ClearShellPacket : PacketDefinition
    {
        [PacketIndex(0)]
        public bool DeleteLastOption { get; set; }

        public static string ReturnHelp()
        {
            return "$ClearShell [DeleteLastOption]";
        }
    }
}
