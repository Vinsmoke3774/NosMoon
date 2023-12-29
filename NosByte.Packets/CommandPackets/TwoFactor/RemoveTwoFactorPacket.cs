using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNos.Core;
using OpenNos.Core.Serializing;
using OpenNos.Domain;

namespace NosByte.Packets.CommandPackets.TwoFactor
{
    [PacketHeader("$Remove2FA", Authority = AuthorityType.User, PassNonParseablePacket = true)]
    public class RemoveTwoFactorPacket : PacketDefinition
    {
        [PacketIndex(0)]
        public string Code { get; set; }
    }
}
