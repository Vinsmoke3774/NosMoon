﻿////<auto-generated <- Codemaid exclusion for now (PacketIndex Order is important for maintenance)

using OpenNos.Core;
using OpenNos.Core.Serializing;

namespace NosByte.Packets.ClientPackets
{
    [PacketHeader("withdraw")]
    public class WithdrawPacket : PacketDefinition
    {
        #region Properties        

        [PacketIndex(0)]
        public short Slot { get; set; }

        [PacketIndex(1)]
        public short Amount { get; set; }

        [PacketIndex(2)]
        public bool PetBackpack { get; set; }

        #endregion
    }
}