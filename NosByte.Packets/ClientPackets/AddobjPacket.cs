﻿////<auto-generated <- Codemaid exclusion for now (PacketIndex Order is important for maintenance)

using OpenNos.Core;
using OpenNos.Core.Serializing;

namespace NosByte.Packets.ClientPackets
{
    [PacketHeader("addobj")]
    public class AddObjPacket : PacketDefinition
    {
        #region Properties


        [PacketIndex(0)]
        public short Slot { get; set; }
        [PacketIndex(1)]
        public short PositionX { get; set; }
        [PacketIndex(2)]
        public short PositionY { get; set; }
        #endregion
    }
}