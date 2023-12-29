using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNos.Core;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Extension.CharacterExtensions;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;

namespace OpenNos.Handler.SharedMethods
{
    public static class SharedNpcMethods
    {
        internal static bool BuyValidate(this ClientSession session, ClientSession clientSession, KeyValuePair<long, MapShop> shop, short slot, short amount)
        {
            if (!clientSession.HasCurrentMapInstance)
            {
                return false;
            }

            PersonalShopItem shopitem = clientSession.CurrentMapInstance.UserShops[shop.Key].Items
                .Find(i => i.ShopSlot.Equals(slot));
            if (shopitem == null)
            {
                return false;
            }

            Guid id = shopitem.ItemInstance.Id;

            ClientSession shopOwnerSession = ServerManager.Instance.GetSessionByCharacterId(shop.Value.OwnerId);
            if (shopOwnerSession == null)
            {
                return false;
            }

            if (amount > shopitem.SellAmount)
            {
                amount = shopitem.SellAmount;
            }

            ItemInstance sellerItem = shopOwnerSession.Character.Inventory.GetItemInstanceById(id);
            if (sellerItem == null || sellerItem.Amount < amount)
            {
                return false;
            }

            List<ItemInstance> inv = shopitem.ItemInstance.Type == InventoryType.Equipment
                ? clientSession.Character.Inventory.AddToInventory(shopitem.ItemInstance)
                : clientSession.Character.Inventory.AddNewToInventory(shopitem.ItemInstance.ItemVNum, amount,
                    shopitem.ItemInstance.Type);

            if (inv.Count == 0)
            {
                return false;
            }

            shopOwnerSession.Character.Gold += shopitem.Price * amount;
            shopOwnerSession.SendPacket(shopOwnerSession.Character.GenerateGold());
            shopOwnerSession.SendPacket(UserInterfaceHelper.GenerateShopMemo(1,
                string.Format(Language.Instance.GetMessageFromKey("BUY_ITEM"), session.Character.Name,
                    shopitem.ItemInstance.Item.Name, amount)));
            clientSession.CurrentMapInstance.UserShops[shop.Key].Sell += shopitem.Price * amount;

            if (shopitem.ItemInstance.Type != InventoryType.Equipment)
            {
                // remove sold amount of items
                shopOwnerSession.Character.Inventory.RemoveItemFromInventory(id, amount);

                // remove sold amount from sellamount
                shopitem.SellAmount -= amount;
            }
            else
            {
                // remove equipment
                shopOwnerSession.Character.Inventory.TryRemove(shopitem.ItemInstance.Id, out _);

                // send empty slot to owners inventory
                shopOwnerSession.SendPacket(
                    UserInterfaceHelper.Instance.GenerateInventoryRemove(shopitem.ItemInstance.Type,
                        shopitem.ItemInstance.Slot));

                // remove the sell amount
                shopitem.SellAmount = 0;
            }

            // remove item from shop if the amount the user wanted to sell has been sold
            if (shopitem.SellAmount == 0)
            {
                clientSession.CurrentMapInstance.UserShops[shop.Key].Items.Remove(shopitem);
            }

            // update currently sold item
            shopOwnerSession.SendPacket($"sell_list {shop.Value.Sell} {slot}.{amount}.{shopitem.SellAmount}");

            // end shop
            if (!clientSession.CurrentMapInstance.UserShops[shop.Key].Items.Any(s => s.SellAmount > 0))
            {
                shopOwnerSession.Character.CloseShop();
            }

            return true;
        }

        internal static void LoadShopItem(this ClientSession session, long owner, KeyValuePair<long, MapShop> shop)
        {
            string packetToSend = $"n_inv 1 {owner} 0 0";

            if (shop.Value?.Items != null)
            {
                foreach (PersonalShopItem item in shop.Value.Items)
                {
                    if (item != null)
                    {
                        if (item.ItemInstance.Item.Type == InventoryType.Equipment)
                        {
                            packetToSend +=
                                $" 0.{item.ShopSlot}.{item.ItemInstance.ItemVNum}.{item.ItemInstance.Rare}.{item.ItemInstance.Upgrade}.{item.Price}";
                        }
                        else
                        {
                            packetToSend +=
                                $" {(byte)item.ItemInstance.Item.Type}.{item.ShopSlot}.{item.ItemInstance.ItemVNum}.{item.SellAmount}.{item.Price}.-1";
                        }
                    }
                    else
                    {
                        packetToSend += " -1";
                    }
                }
            }

#warning Check this
            packetToSend +=
                " -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1";

            session.SendPacket(packetToSend);
        }
    }
}
