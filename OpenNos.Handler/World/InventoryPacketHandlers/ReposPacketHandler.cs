using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NosByte.Packets.ClientPackets;
using OpenNos.Core;
using OpenNos.Core.Logger;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Helpers;

namespace OpenNos.Handler.World.InventoryPacketHandlers
{
    public class ReposPacketHandler : IPacketHandler
    {
        private ClientSession Session { get; set; }

        public ReposPacketHandler(ClientSession session) => Session = session;

        /// <summary>
        /// repos packet
        /// </summary>
        /// <param name="reposPacket"></param>
        public void Repos(ReposPacket reposPacket)
        {
            if (reposPacket != null)
            {
                Logger.Log.LogUserEvent("STASH_REPOS", Session.GenerateIdentity(),
                    $"[ItemReposition]OldSlot: {reposPacket.OldSlot} NewSlot: {reposPacket.NewSlot} Amount: {reposPacket.Amount} PartnerBackpack: {reposPacket.PartnerBackpack}");

                if (reposPacket.Amount <= 0)
                {
                    //Dupe fixed here
                    return;
                }

                if (reposPacket.OldSlot == reposPacket.NewSlot)
                {
                    return;
                }

                // check if the destination slot is out of range
                if (reposPacket.NewSlot >= (reposPacket.PartnerBackpack
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
                Session.Character.Inventory.MoveItem(
                    reposPacket.PartnerBackpack ? InventoryType.PetWarehouse : InventoryType.Warehouse,
                    reposPacket.PartnerBackpack ? InventoryType.PetWarehouse : InventoryType.Warehouse,
                    reposPacket.OldSlot, reposPacket.Amount, reposPacket.NewSlot, out ItemInstance previousInventory,
                    out ItemInstance newInventory);

                if (newInventory == null)
                {
                    return;
                }

                Session.SendPacket(reposPacket.PartnerBackpack
                    ? newInventory.GeneratePStash()
                    : newInventory.GenerateStash());
                Session.SendPacket(previousInventory != null
                    ? (reposPacket.PartnerBackpack
                        ? previousInventory.GeneratePStash()
                        : previousInventory.GenerateStash())
                    : (reposPacket.PartnerBackpack
                        ? UserInterfaceHelper.Instance.GeneratePStashRemove(reposPacket.OldSlot)
                        : UserInterfaceHelper.Instance.GenerateStashRemove(reposPacket.OldSlot)));
            }
        }
    }
}
