﻿////<auto-generated <- Codemaid exclusion for now (PacketIndex Order is important for maintenance)

using OpenNos.Core;
using OpenNos.Core.Serializing;
using OpenNos.Domain;

namespace NosByte.Packets.CommandPackets
{
    [PacketHeader("$Packet", PassNonParseablePacket = true, Authority =  AuthorityType.DEV )]
    public class PacketCallbackPacket : PacketDefinition
    {
        #region Properties

        [PacketIndex(0, SerializeToEnd = true)]
        public string Packet { get; set; }

        public static string ReturnHelp() => "$Packet <Value>";

        #endregion
    }
}