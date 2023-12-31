﻿////<auto-generated <- Codemaid exclusion for now (PacketIndex Order is important for maintenance)

using OpenNos.Core;
using OpenNos.Core.Serializing;
using OpenNos.Domain;

namespace NosByte.Packets.CommandPackets
{
    [PacketHeader("$Guri", PassNonParseablePacket = true, Authority =  AuthorityType.DEV )]
    public class GuriCommandPacket : PacketDefinition
    {
        #region Properties

        [PacketIndex(0)]
        public byte Type { get; set; }

        [PacketIndex(1)]
        public byte Argument { get; set; }

        [PacketIndex(2)]
        public int Value { get; set; }

        public static string ReturnHelp() => "$Guri <Type> <Argument> <Value>";

        #endregion
    }
}