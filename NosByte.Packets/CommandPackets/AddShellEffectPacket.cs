﻿////<auto-generated <- Codemaid exclusion for now (PacketIndex Order is important for maintenance)

using OpenNos.Core;
using OpenNos.Core.Serializing;
using OpenNos.Domain;

namespace NosByte.Packets.CommandPackets
{
    [PacketHeader("$AddShell" , PassNonParseablePacket = true, Authority =  AuthorityType.GD )]
    public class AddShellEffectPacket : PacketDefinition
    {
        #region Properties

        [PacketIndex(0)]
        public byte Slot { get; set; }

        [PacketIndex(1)]
        public byte EffectLevel { get; set; }

        [PacketIndex(2)]
        public byte Effect { get; set; }

        [PacketIndex(3)]
        public short Value { get; set; }

        #endregion

        #region Methods

        public static string ReturnHelp() => "$AddShell <Slot> <EffectLevel> <Effect> <Value>";

        #endregion
    }
}