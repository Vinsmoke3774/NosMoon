﻿////<auto-generated <- Codemaid exclusion for now (PacketIndex Order is important for maintenance)

using OpenNos.Core;
using OpenNos.Core.Serializing;
using OpenNos.Domain;

namespace NosByte.Packets.CommandPackets
{
    [PacketHeader("$Faction", PassNonParseablePacket = true, Authority =  AuthorityType.TMOD )]
    public class FactionPacket : PacketDefinition
    {
        public static string ReturnHelp() => "$Faction";
    }
}