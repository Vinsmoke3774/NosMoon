﻿////<auto-generated <- Codemaid exclusion for now (PacketIndex Order is important for maintenance)

using OpenNos.Core;
using OpenNos.Core.Serializing;
using OpenNos.Domain;

namespace NosByte.Packets.CommandPackets
{
    [PacketHeader("$XpRate", PassNonParseablePacket = true, Authority =  AuthorityType.GD )]
    public class XpRatePacket : PacketDefinition
    {
        #region Properties

        [PacketIndex(0)]
        public int Value { get; set; }

        public static string ReturnHelp() => "$XpRate <Value>";

        #endregion
    }
}