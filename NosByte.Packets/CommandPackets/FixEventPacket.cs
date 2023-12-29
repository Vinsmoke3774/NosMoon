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
    [PacketHeader("$FixEvent", PassNonParseablePacket = true, Authority = AuthorityType.GM)]
    public class FixEventPacket : PacketDefinition
    {
        [PacketIndex(0)]
        public EventType EventType { get; set; }

        public static string ReturnHelp()
        {
            return "$FixEvent <EventType>";
        }
    }
}
