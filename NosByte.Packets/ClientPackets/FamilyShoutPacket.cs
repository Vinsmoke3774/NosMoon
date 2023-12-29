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
    [PacketHeader("%FamilyShout", Authority = AuthorityType.User, PassNonParseablePacket = true)]
    public class FamilyShoutPacket : PacketDefinition
    {
        [PacketIndex(0, SerializeToEnd = true)]
        public string Message { get; set; }
    }
}
