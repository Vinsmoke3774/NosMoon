﻿////<auto-generated <- Codemaid exclusion for now (PacketIndex Order is important for maintenance)

using OpenNos.Core;
using OpenNos.Core.Serializing;
using OpenNos.Domain;

namespace NosByte.Packets.CommandPackets
{
    [PacketHeader("$Monster", PassNonParseablePacket = true, Authority =  AuthorityType.GD )]
    public class MobPacket : PacketDefinition
    {
        #region Properties

        [PacketIndex(0)]
        public short NpcMonsterVNum { get; set; }

        [PacketIndex(1)]
        public short Amount { get; set; }

        [PacketIndex(2)]
        public bool IsMoving { get; set; }

        public static string ReturnHelp() => "$Monster <VNum> <Amount> <IsMoving>";

        #endregion
    }
}