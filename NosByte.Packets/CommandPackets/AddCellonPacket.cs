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
    [PacketHeader("$AddCellon", Authority = AuthorityType.GM, PassNonParseablePacket = true)]
    public class AddCellonPacket : PacketDefinition
    {
        [PacketIndex(0)]
        public byte Slot { get; set; }

        [PacketIndex(1)]
        public byte CellonLevel { get; set; }

        [PacketIndex(2)]
        public byte EffectType { get; set; }

        [PacketIndex(3)]
        public short Value { get; set; }
    }
}
