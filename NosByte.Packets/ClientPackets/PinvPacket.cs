﻿////<auto-generated <- Codemaid exclusion for now (PacketIndex Order is important for maintenance)

using OpenNos.Core;
using OpenNos.Core.Serializing;

namespace NosByte.Packets.ClientPackets
{
    [PacketHeader("$pinv")]
    public class PinvPacket : PacketDefinition
    {
        #region Properties

        [PacketIndex(0)]
        public string CharacterName { get; set; }

        #endregion
    }
}