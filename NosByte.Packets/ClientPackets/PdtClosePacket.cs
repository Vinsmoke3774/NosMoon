using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNos.Core;
using OpenNos.Core.Serializing;

namespace NosByte.Packets.ClientPackets
{
    [PacketHeader("pdtclose")]
    public class PdtClosePacket : PacketDefinition
    {
        [PacketIndex(0, SerializeToEnd = true)]
        public string Data { get; set; }
    }
}
