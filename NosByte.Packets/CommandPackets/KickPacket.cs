﻿////<auto-generated <- Codemaid exclusion for now (PacketIndex Order is important for maintenance)

using OpenNos.Core;
using OpenNos.Core.Serializing;
using OpenNos.Domain;

namespace NosByte.Packets.CommandPackets
{
    [PacketHeader("$Kick", PassNonParseablePacket = true, Authority =  AuthorityType.GS )]
    public class KickPacket : PacketDefinition
    {
        #region Properties

        [PacketIndex(0)]
        public string CharacterName { get; set; }

        public static string ReturnHelp() => "$Kick <Nickname>";

        #endregion
    }
}