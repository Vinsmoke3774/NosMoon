using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NosByte.Packets.ClientPackets;
using OpenNos.Core;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Networking;

namespace OpenNos.Handler.World.InventoryPacketHandlers
{
    public class EquipmentInfoPacketHandler : IPacketHandler
    {
        private ClientSession Session { get; set; }

        public EquipmentInfoPacketHandler(ClientSession session) => Session = session;

        /// <summary>
        /// eqinfo packet
        /// </summary>
        /// <param name="equipmentInfoPacket"></param>
        public void EquipmentInfo(EquipmentInfoPacket equipmentInfoPacket)
        {
            if (equipmentInfoPacket != null)
            {
                bool isNpcShopItem = false;
                ItemInstance inventory = null;
                switch (equipmentInfoPacket.Type)
                {
                    case 0:
                        inventory = Session.Character.Inventory.LoadBySlotAndType(equipmentInfoPacket.Slot,
                            InventoryType.Wear);
                        break;

                    case 1:
                        inventory = Session.Character.Inventory.LoadBySlotAndType(equipmentInfoPacket.Slot,
                            InventoryType.Equipment);
                        break;

                    case 2:
                        isNpcShopItem = true;
                        if (ServerManager.GetItem(equipmentInfoPacket.Slot) != null)
                        {
                            inventory = new ItemInstance(equipmentInfoPacket.Slot, 1);
                            break;
                        }

                        return;

                    case 5:
                        if (Session.Character.ExchangeInfo != null)
                        {
                            ClientSession sess =
                                ServerManager.Instance.GetSessionByCharacterId(Session.Character.ExchangeInfo
                                    .TargetCharacterId);
                            if (sess?.Character.ExchangeInfo?.ExchangeList?.ElementAtOrDefault(equipmentInfoPacket
                                    .Slot) != null)
                            {
                                Guid id = sess.Character.ExchangeInfo.ExchangeList[equipmentInfoPacket.Slot].Id;

                                inventory = sess.Character.Inventory.GetItemInstanceById(id);
                            }
                        }

                        break;

                    case 6:
                        if (equipmentInfoPacket.ShopOwnerId != null)
                        {
                            KeyValuePair<long, MapShop> shop =
                                Session.CurrentMapInstance.UserShops.FirstOrDefault(mapshop =>
                                    mapshop.Value.OwnerId.Equals(equipmentInfoPacket.ShopOwnerId));
                            PersonalShopItem item =
                                shop.Value?.Items.Find(i => i.ShopSlot.Equals(equipmentInfoPacket.Slot));
                            if (item != null)
                            {
                                inventory = item.ItemInstance;
                            }
                        }

                        break;

                    case 7:
                        inventory = Session.Character.Inventory.LoadBySlotAndType(equipmentInfoPacket.MateSlot,
                            (InventoryType)(12 + equipmentInfoPacket.Slot));
                        break;

                    case 10:
                        inventory = Session.Character.Inventory.LoadBySlotAndType(equipmentInfoPacket.Slot,
                            InventoryType.Specialist);
                        break;

                    case 11:
                        inventory = Session.Character.Inventory.LoadBySlotAndType(equipmentInfoPacket.Slot,
                            InventoryType.Costume);
                        break;
                }

                if (inventory?.Item != null)
                {
                    if (inventory.IsEmpty || isNpcShopItem)
                    {
                        Session.SendPacket(inventory.GenerateEInfo());
                        return;
                    }

                    Session.SendPacket(inventory.Item.EquipmentSlot != EquipmentType.Sp ? inventory.GenerateEInfo() :
                        inventory.Item.SpType == 0 && inventory.Item.ItemSubType == 4 ? inventory.GeneratePslInfo() :
                        inventory.GenerateSlInfo(Session));
                }
            }
        }
    }
}
