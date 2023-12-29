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
    [PacketHeader("%Title", Authority = AuthorityType.User, PassNonParseablePacket = true)]
    public class ChangeRankPacket : PacketDefinition
    {
        [PacketIndex(0)]
        public string Name { get; set; }

        [PacketIndex(1)]
        public FamilyMemberRank Rank { get; set; }
    }
}
