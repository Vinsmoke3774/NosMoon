﻿////<auto-generated <- Codemaid exclusion for now (PacketIndex Order is important for maintenance)

using OpenNos.Core;
using OpenNos.Core.Serializing;
using OpenNos.Domain;

namespace NosByte.Packets.CommandPackets
{
    [PacketHeader("$Warn", PassNonParseablePacket = true, Authority = AuthorityType.TGS )]
    public class WarningPacket : PacketDefinition
    {
        #region Properties

        [PacketIndex(0)]
        public string CharacterName { get; set; }

        [PacketIndex(1, serializeToEnd: true)]
        public string Reason { get; set; }

        public static string ReturnHelp() => "$Warn <Nickname> <Reason>";

        #endregion
    }
}