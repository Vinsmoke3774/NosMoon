﻿////<auto-generated <- Codemaid exclusion for now (PacketIndex Order is important for maintenance)

using OpenNos.Core;
using OpenNos.Core.Serializing;
using OpenNos.Domain;

namespace NosByte.Packets.CommandPackets
{
    [PacketHeader("$Time", "$ServerTime", PassNonParseablePacket = true, Authority =  AuthorityType.User)]
    public class Time : PacketDefinition
    {
        #region Properties

        public static string ReturnHelp() => "$Time";

        #endregion
    }
}