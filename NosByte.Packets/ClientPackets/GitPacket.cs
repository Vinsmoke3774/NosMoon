﻿////<auto-generated <- Codemaid exclusion for now (PacketIndex Order is important for maintenance)

using OpenNos.Core;
using OpenNos.Core.Serializing;

namespace NosByte.Packets.ClientPackets
{
    [PacketHeader("git")]
    public class GitPacket : PacketDefinition
    {
        #region Properties

        [PacketIndex(0)]
        public int ButtonId { get; set; }

       
        #endregion
    }
}