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
    [PacketHeader("$Act4Stats", PassNonParseablePacket = true, Authority = AuthorityType.User)]
    public class Act4StatsPacket : PacketDefinition
    {
        public static string ReturnHelp() => "$Act4Stats";
    }

}
