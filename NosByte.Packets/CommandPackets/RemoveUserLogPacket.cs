﻿using OpenNos.Core;
using OpenNos.Core.Serializing;
using OpenNos.Domain;

namespace NosByte.Packets.CommandPackets
{
    [PacketHeader("$RemoveUserLog", PassNonParseablePacket = true, Authority =  AuthorityType.Administrator)]
    public class RemoveUserLogPacket : PacketDefinition
    {
        #region Properties

        [PacketIndex(0)]
        public string Username { get; set; }

        #endregion

        #region Methods

        public static string ReturnHelp() => "$RemoveUserLog <Username>";

        #endregion
    }
}