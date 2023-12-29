﻿////<auto-generated <- Codemaid exclusion for now (PacketIndex Order is important for maintenance)

using OpenNos.Core;
using OpenNos.Core.Serializing;
using OpenNos.Domain;

namespace NosByte.Packets.CommandPackets
{
    [PacketHeader("$Help", PassNonParseablePacket = true, Authority = AuthorityType.User )]
    public class HelpPacket : PacketDefinition
    {
        [PacketIndex(0, SerializeToEnd = true)]
        public string Contents { get; set; }
    }
}