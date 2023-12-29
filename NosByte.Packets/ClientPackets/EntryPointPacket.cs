using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNos.Core;
using OpenNos.Core.Serializing;
using OpenNos.Domain;

namespace NosByte.Packets.ClientPackets
{
    [PacketHeader("OpenNos.EntryPoint", Amount = 3, AnonymousAccess = true, PassNonParseablePacket = true, Authority = AuthorityType.User)]
    public class EntryPointPacket : PacketDefinition
    {
        [PacketIndex(0, SerializeToEnd = true)]
        public string PacketData { get; set; }

        public bool IgnoreSecurity { get; set; }
    }
}
