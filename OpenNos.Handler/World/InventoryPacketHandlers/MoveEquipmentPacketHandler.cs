using System;
using System.IO;
using System.Linq;
using NosByte.Packets.ClientPackets;
using OpenNos.Core;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Extension.CharacterExtensions;
using OpenNos.GameObject.Helpers;

namespace OpenNos.Handler.World.InventoryPacketHandlers
{
    public class MoveEquipmentPacketHandler : IPacketHandler
    {
        private ClientSession Session { get; }

        public MoveEquipmentPacketHandler(ClientSession session) => Session = session;

        /// <summary>
        /// mve packet
        /// </summary>
        /// <param name="mvePacket"></param>
        public void MoveEquipment(MvePacket mvePacket)
        {
            if (mvePacket != null)
            {
                lock (Session.Character.Inventory)
                {
                    if (mvePacket.Slot.Equals(mvePacket.DestinationSlot)
                        && mvePacket.InventoryType.Equals(mvePacket.DestinationInventoryType))
                    {
                        return;
                    }

                    InventoryType[] allowedInventoryTypes = { InventoryType.Equipment, InventoryType.Specialist, InventoryType.Costume };

                    if (allowedInventoryTypes.All(s => s != mvePacket.InventoryType) || allowedInventoryTypes.All(s => s != mvePacket.DestinationInventoryType))
                    {
                        return;
                    }

                    var backPackExtension = 0;

                    if (Session.Character.HasStaticBonus(StaticBonusType.EreniaMedal))
                    {
                        backPackExtension += 8;
                    }

                    if (Session.Character.HasStaticBonus(StaticBonusType.AdventurerMedal))
                    {
                        backPackExtension += 4;
                    }

                    var backPackCapacity = 48 + ((Session.Character.HaveBackpack() ? 1 : 0) * 12) + ((Session.Character.HaveExtension() ? 1 : 0) * 60) + backPackExtension;
                    backPackCapacity = backPackCapacity > 120 ? 120 : backPackCapacity;


                    if (mvePacket.DestinationSlot > backPackCapacity)
                    {
                        return;
                    }

                    if (Session.Character.InExchangeOrTrade)
                    {
                        return;
                    }

                    ItemInstance sourceItem =
                        Session.Character.Inventory.LoadBySlotAndType(mvePacket.Slot, mvePacket.InventoryType);
                    if (sourceItem?.Item.ItemType == ItemType.Specialist
                        || sourceItem?.Item.ItemType == ItemType.Fashion)
                    {
                        ItemInstance inv = Session.Character.Inventory.MoveInInventory(mvePacket.Slot,
                            mvePacket.InventoryType, mvePacket.DestinationInventoryType, mvePacket.DestinationSlot,
                            false);
                        if (inv != null)
                        {
                            Session.SendPacket(inv.GenerateInventoryAdd());
                            Session.SendPacket(
                                UserInterfaceHelper.Instance.GenerateInventoryRemove(mvePacket.InventoryType,
                                    mvePacket.Slot));
                        }
                    }
                }
            }
        }
    }
}
