/*
 * This file is part of the OpenNos Emulator Project. See AUTHORS file for Copyright information
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 */

using OpenNos.Domain;
using System;

namespace OpenNos.GameObject
{
    public class MonsterMapItem : MapItem
    {
        #region Instantiation

        public MonsterMapItem(short x, short y, short itemVNum, int amount = 1, long ownerId = -1, bool isQuest = false, byte minRarity = 0, byte maxRarity = 0) : base(x, y)
        {
            ItemVNum = itemVNum;
            if (amount < 1000)
            {
                Amount = (short)amount;
            }
            GoldAmount = amount;
            OwnerId = ownerId;
            IsQuest = isQuest;
            MinRarity = minRarity;
            MaxRarity = maxRarity;
        }

        #endregion

        #region Properties

        public sealed override short Amount { get; set; }

        public int GoldAmount { get; }

        public sealed override short ItemVNum { get; set; }

        public long? OwnerId { get; }

        public byte MinRarity { get; set; }

        public byte MaxRarity { get; set; }

        #endregion

        #region Methods

        public override ItemInstance GetItemInstance()
        {
            if (_itemInstance == null && OwnerId != null)
            {
                _itemInstance = Inventory.InstantiateItemInstance(ItemVNum, OwnerId.Value, Amount);
            }
            return _itemInstance;
        }

        public void RarifyV2()
        {
            ItemInstance instance = GetItemInstance();
            if (instance?.Item?.Type == InventoryType.Equipment && (instance?.Item?.ItemType == ItemType.Weapon || instance?.Item?.ItemType == ItemType.Armor || instance?.Item?.ItemType == ItemType.Shell))
            {
                var probability = new Random().Next(1, 101);
                byte rarity;
                if (probability <= 3) rarity = 7;
                else if (probability <= 10) rarity = 6;
                else if (probability <= 85) rarity = 5;
                else rarity = (byte)new Random().Next(MinRarity, MaxRarity);
                instance?.RarifyItem(null, RarifyMode.Drop, RarifyProtection.None, forceRare: rarity);
            }
        }

        public void Rarify(ClientSession session)
        {
            ItemInstance instance = GetItemInstance();
            if (instance?.Item?.Type == InventoryType.Equipment && (instance?.Item?.ItemType == ItemType.Weapon || instance?.Item?.ItemType == ItemType.Armor))
            {
                instance?.RarifyItem(session, RarifyMode.Drop, RarifyProtection.None);
            }
        }

        #endregion
    }
}