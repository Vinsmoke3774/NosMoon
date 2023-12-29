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
    [PacketHeader("%Familygold", Authority = AuthorityType.User, PassNonParseablePacket = true)]
    public class FamilyGoldPacket : PacketDefinition
    {
        
    }
}
