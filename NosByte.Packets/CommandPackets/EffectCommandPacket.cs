﻿////<auto-generated <- Codemaid exclusion for now (PacketIndex Order is important for maintenance)

using OpenNos.Core;
using OpenNos.Core.Serializing;
using OpenNos.Domain;

namespace NosByte.Packets.CommandPackets
{
    [PacketHeader("$Effect", PassNonParseablePacket = true, Authority =  AuthorityType.SMOD )]
    public class EffectCommandPacket : PacketDefinition
    {
        #region Properties

        [PacketIndex(0)]
        public int EffectId { get; set; }

        public static string ReturnHelp() => "$Effect <Value>";

        #endregion
    }
}