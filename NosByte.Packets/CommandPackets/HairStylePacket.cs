﻿////<auto-generated <- Codemaid exclusion for now (PacketIndex Order is important for maintenance)

using OpenNos.Core;
using OpenNos.Core.Serializing;
using OpenNos.Domain;

namespace NosByte.Packets.CommandPackets
{
    [PacketHeader("$HairStyle", PassNonParseablePacket = true, Authority =  AuthorityType.TMOD )]
    public class HairStylePacket : PacketDefinition
    {
        #region Properties

        [PacketIndex(0)]
        public HairStyleType HairStyle { get; set; }

        public static string ReturnHelp() => "$HairStyle <Value>";

        #endregion
    }
}