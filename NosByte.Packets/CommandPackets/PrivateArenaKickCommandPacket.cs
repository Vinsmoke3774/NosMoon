using OpenNos.Core;
using OpenNos.Core.Serializing;
using OpenNos.Domain;

namespace NosByte.Packets.CommandPackets
{
    [PacketHeader("%Kick", Authority = AuthorityType.User, PassNonParseablePacket = true)]
    public class PrivateArenaKickCommandPacket
    {
        [PacketIndex(0, SerializeToEnd = true)]
        public string Name { get; set; }

        public string ReturnHelp() => "%Kick Name";
    }
}
