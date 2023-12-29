﻿////<auto-generated <- Codemaid exclusion for now (PacketIndex Order is important for maintenance)

using OpenNos.Core;
using OpenNos.Core.Serializing;
using OpenNos.Domain;

namespace NosByte.Packets.Lock
{
    [PacketHeader("$ChangeLock", "$ChLock", PassNonParseablePacket = true, Authority =  AuthorityType.User)]
    public class ChangeLockPacket : PacketDefinition
    {
        [PacketIndex(0)]
        public string Code { get; set; }

        [PacketIndex(1)]
        public string NewCode { get; set; }

        #region Properties

        public static string ReturnHelp() => "$ChangeLock <Code> <NewCode>";

        #endregion
    }
}