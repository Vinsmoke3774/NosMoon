﻿////<auto-generated <- Codemaid exclusion for now (PacketIndex Order is important for maintenance)

using OpenNos.Core;
using OpenNos.Core.Serializing;
using OpenNos.Domain;

namespace NosByte.Packets.CommandPackets
{
    [PacketHeader("$AddNpc" , PassNonParseablePacket = true, Authority = AuthorityType.TM )]
    public class AddNpcPacket : PacketDefinition
    {
        #region Properties

        [PacketIndex(0)]
        public short NpcVNum { get; set; }

        [PacketIndex(1)]
        public bool IsMoving { get; set; }

        public static string ReturnHelp() => "$AddNpc <VNum> <IsMoving>";

        #endregion
    }
}