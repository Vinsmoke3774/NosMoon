using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNos.Core;
using OpenNos.Core.Serializing;
using OpenNos.Domain;

namespace NosByte.Packets.ClientPackets.Inventory
{
    [PacketHeader("exc_list", Authority = AuthorityType.User, PassNonParseablePacket = true)]
    public class ExchangeListPacket : PacketDefinition
    {
        [PacketIndex(0)]
        public int Gold { get; set; }

        [PacketIndex(1)]
        public long BankGold { get; set; }

        [PacketIndex(2, SerializeToEnd = true)]
        public string Data { get; set; }

        public string PacketData { get; set; }
       

        public override string ToString() => $"Gold: {Gold} BankGold: {BankGold} Data: {Data}";

    }
}
