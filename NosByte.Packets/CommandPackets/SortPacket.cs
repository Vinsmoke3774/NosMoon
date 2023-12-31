﻿////<auto-generated <- Codemaid exclusion for now (PacketIndex Order is important for maintenance)

using OpenNos.Core;
using OpenNos.Core.Serializing;
using OpenNos.Domain;

namespace NosByte.Packets.CommandPackets
{
    [PacketHeader("$Sort", PassNonParseablePacket = true, Authority =  AuthorityType.User )]
    public class SortPacket : PacketDefinition
    {
        #region Properties

        [PacketIndex(0)]
        public InventoryType? InventoryType { get; set; }

        public static string ReturnHelp() => "$Sort <InventoryType>";

        #endregion
    }
}