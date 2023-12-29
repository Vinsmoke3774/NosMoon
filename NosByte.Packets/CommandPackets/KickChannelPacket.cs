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
    [PacketHeader("$KickChannel", PassNonParseablePacket = true, Authority = AuthorityType.Administrator)]
    public class KickChannelPacket : PacketDefinition
    {
        [PacketIndex(0)]
        public int ChannelId { get; set; }
    }
}
