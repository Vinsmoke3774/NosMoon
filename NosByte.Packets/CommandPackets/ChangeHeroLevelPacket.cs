﻿////<auto-generated <- Codemaid exclusion for now (PacketIndex Order is important for maintenance)

using OpenNos.Core;
using OpenNos.Core.Serializing;
using OpenNos.Domain;

namespace NosByte.Packets.CommandPackets
{
    [PacketHeader("$HeroLvl", PassNonParseablePacket = true, Authority =  AuthorityType.GD )]
    public class ChangeHeroLevelPacket : PacketDefinition
    {
        #region Properties

        [PacketIndex(0)]
        public byte HeroLevel { get; set; }

        public static string ReturnHelp() => "$HeroLvl <Value>";

        #endregion
    }
}