using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNos.Core;
using OpenNos.Core.Serializing;
using OpenNos.Domain;

namespace NosByte.Packets.ClientPackets.Family
{
    [PacketHeader("%FamilyLeave", PassNonParseablePacket = true, Authority = AuthorityType.User)]
    public class FamilyLeavePacket : PacketDefinition
    {
    }
}
