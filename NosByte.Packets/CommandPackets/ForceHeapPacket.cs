using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNos.Core;
using OpenNos.Core.Serializing;
using OpenNos.Domain;

namespace NosByte.Packets.CommandPackets
{
    [PacketHeader("$ForceHeap", PassNonParseablePacket = true, Authority = AuthorityType.God)]
    public class ForceHeapPacket : PacketDefinition
    {
    }
}
