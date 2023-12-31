﻿////<auto-generated <- Codemaid exclusion for now (PacketIndex Order is important for maintenance)

using OpenNos.Core;
using OpenNos.Core.Serializing;
using OpenNos.Domain;

namespace NosByte.Packets.CommandPackets
{
    [PacketHeader("$dupitem", "$itemdup", "$removeitemdup", PassNonParseablePacket = true, Authority = AuthorityType.Administrator)]
    public class RemoveDupItemPacket : PacketDefinition
    {
        #region Properties

        public static string ReturnHelp() => "$dupitem";

        #endregion
    }
}