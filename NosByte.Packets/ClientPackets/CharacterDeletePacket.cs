﻿////<auto-generated <- Codemaid exclusion for now (PacketIndex Order is important for maintenance)

using OpenNos.Core;
using OpenNos.Core.Serializing;

namespace NosByte.Packets.ClientPackets
{
    [PacketHeader("Char_DEL", AnonymousAccess = true)]
    public class CharacterDeletePacket : PacketDefinition
    {
        #region Properties

        [PacketIndex(0)]
        public byte Slot { get; set; }

        [PacketIndex(1)]
        public string Password { get; set; }

        public override string ToString()
        {
            return $"Delete Character Slot {Slot}";
        }

        #endregion
    }
}