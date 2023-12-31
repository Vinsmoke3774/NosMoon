﻿////<auto-generated <- Codemaid exclusion for now (PacketIndex Order is important for maintenance)

using OpenNos.Core;
using OpenNos.Core.Serializing;
using OpenNos.Domain;

namespace NosByte.Packets.CommandPackets
{
    [PacketHeader("$Gold", PassNonParseablePacket = true, Authority =  AuthorityType.BA )]
    public class GoldPacket : PacketDefinition
    {
        #region Properties

        [PacketIndex(0)]
        public long Amount { get; set; }

        public static string ReturnHelp() => "$Gold <Value>";

        #endregion
    }
}