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
    [PacketHeader("$RestoreAccount", PassNonParseablePacket = true, Authority = AuthorityType.Administrator)]
    public class RestoreAccountPacket : PacketDefinition
    {
        [PacketIndex(0)]
        public string AccountName { get; set; }
    }
}
