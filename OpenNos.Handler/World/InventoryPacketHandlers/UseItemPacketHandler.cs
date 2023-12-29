using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NosByte.Packets.ClientPackets;
using OpenNos.Core;
using OpenNos.GameObject;

namespace OpenNos.Handler.World.InventoryPacketHandlers
{
    public class UseItemPacketHandler : IPacketHandler
    {
        private ClientSession Session { get; set; }

        public UseItemPacketHandler(ClientSession session) => Session = session;

        /// <summary>
        /// u_i packet
        /// </summary>
        /// <param name="useItemPacket"></param>
        public void UseItem(UseItemPacket useItemPacket)
        {
            if (useItemPacket == null || (byte)useItemPacket.Type >= 9)
            {
                return;
            }

            if (Session.Character.InExchangeOrTrade)
            {
                return;
            }

            ItemInstance itemInstance = Session.Character.Inventory.LoadBySlotAndType(useItemPacket.Slot, useItemPacket.Type);

            string[] packet = useItemPacket.OriginalContent.Split(' ', '^');

            if (packet.Length >= 2 && packet[1].Length > 0)
            {
                itemInstance?.Item.Use(Session, ref itemInstance, packet[1][0] == '#' ? (byte)255 : (byte)0, packet);
            }
        }
    }
}
