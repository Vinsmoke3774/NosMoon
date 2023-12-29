using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NosByte.Packets.ClientPackets;
using OpenNos.Core;
using OpenNos.Domain;
using OpenNos.GameObject;

namespace OpenNos.Handler.World.InventoryPacketHandlers
{
    public class SortOpenPacketHandler : IPacketHandler
    {
        private ClientSession Session { get; set; }

        public SortOpenPacketHandler(ClientSession session) => Session = session;

        /// <summary>
        /// sortopen packet
        /// </summary>
        /// <param name="sortOpenPacket"></param>
        public void SortOpen(SortOpenPacket sortOpenPacket)
        {
            if (sortOpenPacket != null)
            {
                bool gravity = true;
                while (gravity)
                {
                    gravity = false;
                    for (short i = 0; i < 2; i++)
                    {
                        for (short x = 0; x < 44; x++)
                        {
                            InventoryType type = i == 0 ? InventoryType.Specialist : InventoryType.Costume;
                            if (Session.Character.Inventory.LoadBySlotAndType(x, type) == null
                                && Session.Character.Inventory.LoadBySlotAndType((short)(x + 1), type)
                                != null)
                            {
                                Session.Character.Inventory.MoveItem(type, type, (short)(x + 1), 1, x,
                                    out ItemInstance _, out ItemInstance invdest);
                                Session.SendPacket(invdest.GenerateInventoryAdd());
                                Session.Character.DeleteItem(type, (short)(x + 1));
                                gravity = true;
                            }
                        }

                        Session.Character.Inventory.Reorder(Session,
                            i == 0 ? InventoryType.Specialist : InventoryType.Costume);
                    }
                }
            }
        }
    }
}
