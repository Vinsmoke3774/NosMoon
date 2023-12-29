using NosByte.Packets.ClientPackets;
using OpenNos.Core;
using OpenNos.Core.Logger;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Helpers;
using System;

namespace OpenNos.Handler.World.InventoryPacketHandlers
{
    public class DepositPacketHandler : IPacketHandler
    {
        private ClientSession Session { get; set; }

        public DepositPacketHandler(ClientSession session) => Session = session;

        /// <summary>
        /// deposit packet
        /// </summary>
        /// <param name="depositPacket"></param>
        public void Deposit(DepositPacket depositPacket)
        {
            if (Session.Account.Authority < AuthorityType.GM)
            {
                Session.SendPacket(UserInterfaceHelper.GenerateInfo("The warehouse has been disabled for now due to big issues. We're looking into it."));
                return;
            }

            if (depositPacket != null)
            {
                if (depositPacket.Inventory == InventoryType.Bazaar
                    || depositPacket.Inventory == InventoryType.FamilyWareHouse
                    || depositPacket.Inventory == InventoryType.Miniland)
                {
                    return;
                }

                if (DateTime.Now <= Session.Character.LastDeposit.AddSeconds(2))
                {
                    return;
                }

                Session.Character.LastDeposit = DateTime.Now;

                ItemInstance item =
                    Session.Character.Inventory.LoadBySlotAndType(depositPacket.Slot, depositPacket.Inventory);
                ItemInstance itemdest = Session.Character.Inventory.LoadBySlotAndType(depositPacket.NewSlot,
                    depositPacket.PartnerBackpack ? InventoryType.PetWarehouse : InventoryType.Warehouse);

                // check if the destination slot is out of range
                if (depositPacket.NewSlot >= (depositPacket.PartnerBackpack
                        ? (Session.Character.StaticBonusList.Any(s => s.StaticBonusType == StaticBonusType.PetBackPack)
                            ? 50
                            : 0)
                        : Session.Character.WareHouseSize))
                {
                    return;
                }

                // check if the character is allowed to move the item
                if (Session.Character.InExchangeOrTrade)
                {
                    return;
                }

                // actually move the item from source to destination
                Session.Character.Inventory.DepositItem(depositPacket.Inventory, depositPacket.Slot,
                    depositPacket.Amount, depositPacket.NewSlot, ref item, ref itemdest, depositPacket.PartnerBackpack);
                Logger.Log.LogUserEvent("STASH_DEPOSIT", Session.GenerateIdentity(),
                    $"[Deposit]OldIIId: {item?.Id} NewIIId: {itemdest?.Id} Amount: {depositPacket.Amount} PartnerBackpack: {depositPacket.PartnerBackpack}");
            }
        }
    }
}
