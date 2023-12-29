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
    [PacketHeader("sc_p_cts")]
    public class ScpCtsPacket : PacketDefinition
    {
        [PacketIndex(0, SerializeToEnd = true)]
        public string Data { get; set; }
    }
}
