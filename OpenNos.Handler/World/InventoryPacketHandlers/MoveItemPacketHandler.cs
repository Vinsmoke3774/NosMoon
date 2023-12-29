using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NosByte.Packets.ClientPackets;
using OpenNos.Core;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Extension.CharacterExtensions;
using OpenNos.GameObject.Helpers;

namespace OpenNos.Handler.World.InventoryPacketHandlers
{
    public class MoveItemPacketHandler : IPacketHandler
    {
        private ClientSession Session { get; set; }

        public MoveItemPacketHandler(ClientSession session) => Session = session;

        /// <summary>
        /// mvi packet
        /// </summary>
        /// <param name="mviPacket"></param>
        public void MoveItem(MviPacket mviPacket)
        {
            if (mviPacket != null)
            {
                if (mviPacket.Amount <= 0 ||
                    mviPacket.InventoryType == InventoryType.Bazaar ||
                    mviPacket.InventoryType == InventoryType.Wear ||
                    mviPacket.InventoryType == InventoryType.FamilyWareHouse ||
                    mviPacket.InventoryType == InventoryType.Warehouse ||
                    mviPacket.Slot == mviPacket.DestinationSlot)
                {
                    return;
                }

                if (mviPacket.InventoryType >= (InventoryType)13 &&
                    mviPacket.InventoryType <= (InventoryType)24 &&
                    Session.Character.Mates.ToList().Count(s => s.MateType == MateType.Partner) <
                    (byte)(mviPacket.InventoryType - 12))
                {
                    return;
                }

                if (mviPacket.Slot == mviPacket.DestinationSlot)
                {
                    return;
                }

                lock (Session.Character.Inventory)
                {

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


                    // check if the destination slot is out of range
                    if (mviPacket.DestinationSlot > backPackCapacity)
                    {
                        return;
                    }

                    // check if the character is allowed to move the item
                    if (Session.Character.InExchangeOrTrade)
                    {
                        return;
                    }

                    // actually move the item from source to destination
                    Session.Character.Inventory.MoveItem(mviPacket.InventoryType, mviPacket.InventoryType,
                        mviPacket.Slot, mviPacket.Amount, mviPacket.DestinationSlot, out ItemInstance previousInventory,
                        out ItemInstance newInventory);
                    if (newInventory == null)
                    {
                        return;
                    }

                    Session.SendPacket(newInventory.GenerateInventoryAdd());

                    Session.SendPacket(previousInventory != null
                        ? previousInventory.GenerateInventoryAdd()
                        : UserInterfaceHelper.Instance.GenerateInventoryRemove(mviPacket.InventoryType,
                            mviPacket.Slot));
                }
            }
        }
    }
}
