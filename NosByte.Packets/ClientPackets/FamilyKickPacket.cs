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
    [PacketHeader("%FamilyKick", "%FamilyDismiss", Authority = AuthorityType.User, PassNonParseablePacket = true)]
    public class FamilyKickPacket : PacketDefinition
    {
        [PacketIndex(0)]
        public string Name { get; set; }
    }
}
