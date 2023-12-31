﻿////<auto-generated <- Codemaid exclusion for now (PacketIndex Order is important for maintenance)

using OpenNos.Core;
using OpenNos.Core.Serializing;
using OpenNos.Domain;

namespace NosByte.Packets.CommandPackets
{
    [PacketHeader("$ChangeShopName" , PassNonParseablePacket = true, Authority =  AuthorityType.GD )]
    public class ChangeShopNamePacket : PacketDefinition
    {
        #region Properties

        [PacketIndex(0)]
        public string Name { get; set; }

        public static string ReturnHelp() => "$ChangeShopName <Value>";

        #endregion
    }
}