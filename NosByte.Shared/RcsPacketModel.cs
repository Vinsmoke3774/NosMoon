using System;
using NosByte.Packets.ClientPackets;

namespace NosByte.Shared
{
    public class RcsPacketModel
    {
        public CSListPacket Packet { get; set; }

        public long CharacterId { get; set; }
    }
}
