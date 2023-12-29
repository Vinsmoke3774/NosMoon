using NosByte.Packets.ClientPackets;
using OpenNos.Core;
using OpenNos.Core.Logger;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Helpers;
using System;

namespace OpenNos.Handler.World.InventoryPacketHandlers
{
    public class WithdrawItemPacketHandler : IPacketHandler
    {
        private ClientSession Session { get; set; }

        public WithdrawItemPacketHandler(ClientSession session) => Session = session;

        /// <summary>
        /// withdraw packet
        /// </summary>
        /// <param name="withdrawPacket"></param>
        public void Withdraw(WithdrawPacket withdrawPacket)
        {
            if (withdrawPacket == null)
            {
                return;
            }
            if (DateTime.Now <= Session.Character.LastWithdraw.AddSeconds(2))
            {
                return;
            }

            Session.Character.LastWithdraw = DateTime.Now;

            ItemInstance previousInventory = Session.Character.Inventory.LoadBySlotAndType(withdrawPacket.Slot,
                withdrawPacket.PetBackpack ? InventoryType.PetWarehouse : InventoryType.Warehouse);
            if (withdrawPacket.Amount <= 0 || previousInventory == null
                || withdrawPacket.Amount > previousInventory.Amount
                || !Session.Character.Inventory.CanAddItem(previousInventory.ItemVNum))
            {
                return;
            }

            ItemInstance item2 = previousInventory.DeepCopy();
            item2.Id = Guid.NewGuid();
            item2.Amount = withdrawPacket.Amount;
            Logger.Log.LogUserEvent("STASH_WITHDRAW", Session.GenerateIdentity(),
                $"[Withdraw]OldIIId: {previousInventory.Id} NewIIId: {item2.Id} Amount: {withdrawPacket.Amount} PartnerBackpack: {withdrawPacket.PetBackpack}");
            Session.Character.Inventory.RemoveItemFromInventory(previousInventory.Id, withdrawPacket.Amount);
            Session.Character.Inventory.AddToInventory(item2, item2.Item.Type);
            Session.Character.Inventory.LoadBySlotAndType(withdrawPacket.Slot,
                withdrawPacket.PetBackpack ? InventoryType.PetWarehouse : InventoryType.Warehouse);
            if (previousInventory.Amount > 0)
            {
                Session.SendPacket(withdrawPacket.PetBackpack ? previousInventory.GeneratePStash() : previousInventory.GenerateStash());
            }
            else
            {
                Session.SendPacket(withdrawPacket.PetBackpack
                    ? UserInterfaceHelper.Instance.GeneratePStashRemove(withdrawPacket.Slot)
                    : UserInterfaceHelper.Instance.GenerateStashRemove(withdrawPacket.Slot));
            }
        }
    }
}
