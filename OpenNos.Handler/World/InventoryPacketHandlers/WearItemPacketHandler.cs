using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NosByte.Packets.ClientPackets;
using OpenNos.Core;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Extension.CharacterExtensions;
using OpenNos.GameObject.Helpers;
using OpenNos.Handler.SharedMethods;

namespace OpenNos.Handler.World.InventoryPacketHandlers
{
    public class WearItemPacketHandler : IPacketHandler
    {
        private ClientSession Session { get; set; }

        public WearItemPacketHandler(ClientSession session) => Session = session;

        /// <summary>
        /// wear packet
        /// </summary>
        /// <param name="wearPacket"></param>
        public void Wear(WearPacket wearPacket)
        {
            if (wearPacket == null || Session.Character.ExchangeInfo?.ExchangeList.Count > 0 || Session.Character.Speed == 0)
            {
                return;
            }

            if (Session.HasCurrentMapInstance && Session.CurrentMapInstance.UserShops.FirstOrDefault(mapshop => mapshop.Value.OwnerId.Equals(Session.Character.CharacterId)).Value == null)
            {
                ItemInstance inv = Session.Character.Inventory.LoadBySlotAndType(wearPacket.InventorySlot, InventoryType.Equipment);

                if (inv?.Item != null)
                {
                    inv.Item.Use(Session, ref inv, wearPacket.Type);
                    Session.Character.LoadSpeed();
                    Session.SendPacket(StaticPacketHelper.GenerateEff(UserType.Player, Session.Character.CharacterId, 123));

                    ItemInstance ring = Session.Character.Inventory.LoadBySlotAndType((byte)EquipmentType.Ring, InventoryType.Wear);
                    ItemInstance bracelet = Session.Character.Inventory.LoadBySlotAndType((byte)EquipmentType.Bracelet, InventoryType.Wear);
                    ItemInstance necklace = Session.Character.Inventory.LoadBySlotAndType((byte)EquipmentType.Necklace, InventoryType.Wear);
                    Session.Character.CellonOptions.Clear();
                    if (ring != null)
                    {
                        Session.Character.CellonOptions.AddRange(ring.CellonOptions);
                    }
                    if (bracelet != null)
                    {
                        Session.Character.CellonOptions.AddRange(bracelet.CellonOptions);
                    }
                    if (necklace != null)
                    {
                        Session.Character.CellonOptions.AddRange(necklace.CellonOptions);
                    }

                    if (Session.Character.UseSp)
                    {
                        Session.LoadSPStats();
                    }
                    Session.SendPacket(Session.Character.GenerateStat());
                }
            }
        }
    }
}
