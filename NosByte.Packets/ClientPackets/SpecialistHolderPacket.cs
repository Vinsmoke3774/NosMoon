﻿////<auto-generated <- Codemaid exclusion for now (PacketIndex Order is important for maintenance)

using OpenNos.Core;
using OpenNos.Core.Serializing;

namespace NosByte.Packets.ClientPackets
{
    [PacketHeader("s_carrier")]
    public class SpecialistHolderPacket : PacketDefinition
    {
        #region Properties

        [PacketIndex(0)]
        public short Slot { get; set; }

        [PacketIndex(1)]
        public byte HolderSlot { get; set; }

        [PacketIndex(2)]
        public byte HolderType { get; set; }

        #endregion
    }
}
