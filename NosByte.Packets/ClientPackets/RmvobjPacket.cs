﻿////<auto-generated <- Codemaid exclusion for now (PacketIndex Order is important for maintenance)

using OpenNos.Core;
using OpenNos.Core.Serializing;

namespace NosByte.Packets.ClientPackets
{
    [PacketHeader("rmvobj")]
    public class RmvobjPacket : PacketDefinition
    {
        #region Properties

        [PacketIndex(0)]
        public short Slot { get; set; }

        #endregion
    }
}