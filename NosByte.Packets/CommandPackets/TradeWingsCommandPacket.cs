using OpenNos.Core;
using OpenNos.Core.Serializing;
using OpenNos.Domain;

namespace NosByte.Packets.CommandPackets
{
    [PacketHeader("$TradeWings", Authority = AuthorityType.User, PassNonParseablePacket = true)]
    public class TradeWingsCommandPacket : PacketDefinition
    {

    }
}
