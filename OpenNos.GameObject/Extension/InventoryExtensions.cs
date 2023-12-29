using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure.Annotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNos.Core;
using OpenNos.Domain;
using OpenNos.GameObject.Helpers;

namespace OpenNos.GameObject.Extension
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public static class InventoryExtensions
    {
        private const int MAX_SHOP_ITEMS = 79;

        public static IEnumerable<ExchangeListItem> MapToExchangeList(this string data)
        {
            var result = new List<ExchangeListItem>();

            if (string.IsNullOrEmpty(data))
            {
                return result;
            }

            var split = data.Split(' ');

            for (int index = 3; index <= split.Length; index += 3)
            {
                byte.TryParse(split[index - 3], out var type);
                short.TryParse(split[index - 2], out var slot);
                short.TryParse(split[index - 1], out var quantity);

                result.Add(new ExchangeListItem
                {
                    Type = (InventoryType)type,
                    Slot = slot,
                    Quantity = quantity
                });
            }


            return result;
        }

        public static IEnumerable<PersonalShopItem> MapToPersonnalShopItem(this string data, ClientSession session)
        {
            var result = new List<PersonalShopItem>();
            
            if (string.IsNullOrEmpty(data))
            {
                return result;
            }

            var split = data.Split(' ');

            for (int index = 0; index < MAX_SHOP_ITEMS; index += 4)
            {
                Enum.TryParse(split[index], out InventoryType type);
                short.TryParse(split[index + 1], out var slot);
                short.TryParse(split[index + 2], out var quantity);
                long.TryParse(split[index + 3], out var gold);

                if (gold <= 0 || quantity <= 0)
                {
                    continue;
                }

                var itemInstance = session.Character.Inventory.LoadBySlotAndType(slot, type);

                if (itemInstance == null)
                {
                    continue;
                }

                if (!itemInstance.Item.IsTradable || itemInstance.IsBound)
                {
                    session.SendPacket(UserInterfaceHelper.GenerateMsg(
                        Language.Instance.GetMessageFromKey("SHOP_ONLY_TRADABLE_ITEMS"), 0));
                    session.SendPacket("shop_end 0");
                    return new List<PersonalShopItem>();
                }

                result.Add(new PersonalShopItem
                {
                    ItemInstance = itemInstance,
                    Price = gold,
                    SellAmount = quantity,
                    ShopSlot = slot
                });
            }

            return result;
        }
    }
}
