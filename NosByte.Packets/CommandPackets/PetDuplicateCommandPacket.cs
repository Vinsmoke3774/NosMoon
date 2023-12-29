using OpenNos.Core;
using OpenNos.Core.Serializing;
using OpenNos.Domain;

namespace NosByte.Packets.CommandPackets
{
    [PacketHeader("$DupedPets", Authority =  AuthorityType.Administrator, PassNonParseablePacket = true)]
    public class PetDuplicateCommandPacket : PacketDefinition
    {
    }
}
