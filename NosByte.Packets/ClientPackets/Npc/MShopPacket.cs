using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNos.Core;
using OpenNos.Core.Serializing;
using OpenNos.Domain;

namespace NosByte.Packets.ClientPackets.Npc
{
    [PacketHeader("m_shop", PassNonParseablePacket = true, Authority = AuthorityType.User)]
    public class MShopPacket : PacketDefinition
    {
        [PacketIndex(0)]
        public MShopType Type { get; set; }

        [PacketIndex(1, SerializeToEnd = true)]
        public string Data { get; set; }
    }
}
