using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using OpenNos.Core;
using OpenNos.Core.Serializing;
using OpenNos.Domain;

namespace NosByte.Packets.ClientPackets.Family
{
    [PacketHeader("%Familybankdonation", Authority = AuthorityType.User, PassNonParseablePacket = true)]
    public class FamilyBankGoldDonatePacket : PacketDefinition
    {
        [PacketIndex(0)]
        public long Amount { get; set; }
    }
}
