﻿////<auto-generated <- Codemaid exclusion for now (PacketIndex Order is important for maintenance)

using OpenNos.Core;
using OpenNos.Core.Serializing;
using OpenNos.Domain;

namespace NosByte.Packets.CommandPackets
{
    [PacketHeader("$GoldDropRate", PassNonParseablePacket = true, Authority =  AuthorityType.GD )]
    public class GoldDropRatePacket : PacketDefinition
    {
        #region Properties

        [PacketIndex(0)]
        public int Value { get; set; }

        public static string ReturnHelp() => "$GoldDropRate <Value>";

        #endregion
    }
}