﻿////<auto-generated <- Codemaid exclusion for now (PacketIndex Order is important for maintenance)

using OpenNos.Core;
using OpenNos.Core.Serializing;
using OpenNos.Domain;

namespace NosByte.Packets.CommandPackets
{
    [PacketHeader("$Undercover", PassNonParseablePacket = true, Authority =  AuthorityType.TM )]
    public class UndercoverPacket : PacketDefinition
    {
        public static string ReturnHelp() => "$Undercover";
    }
}