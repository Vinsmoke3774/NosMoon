﻿////<auto-generated <- Codemaid exclusion for now (PacketIndex Order is important for maintenance)

using OpenNos.Core;
using OpenNos.Core.Serializing;
using OpenNos.Domain;

namespace NosByte.Packets.CommandPackets
{
    [PacketHeader("$GodMode", PassNonParseablePacket = true, Authority =  AuthorityType.BA )]
    public class GodModePacket : PacketDefinition
    {
        public static string ReturnHelp() => "$GodMode";
    }
}