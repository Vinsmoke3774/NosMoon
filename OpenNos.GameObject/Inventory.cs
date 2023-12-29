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

using OpenNos.Core;
using OpenNos.Core.Logger;
using OpenNos.DAL;
using OpenNos.Data;
using OpenNos.Domain;
using OpenNos.GameObject.Extension.CharacterExtensions;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace OpenNos.GameObject
{
    public class Inventory : ConcurrentDictionary<Guid, ItemInstance>
    {
        #region Members

        private const short DefaultBackpackSize = 48;

        private const short MaxItemAmount = 9999;

        private readonly object _lockObject = new object();

        #endregion

        #region Instantiation

        public Inventory(Character character) => Owner = character;

        #endregion

        #region Properties

        private Character Owner { get; }

        #endregion

        #region Methods

        public static ItemInstance InstantiateItemInstance(short vnum, long ownerId, short amount = 1)
        {
            ItemInstance newItem = new ItemInstance { ItemVNum = vnum, Amount = amount, CharacterId = ownerId };
            if (newItem.Item != null)
            {
                switch (newItem.Item.Type)
                {
                    case InventoryType.Miniland:
                        newItem.DurabilityPoint = newItem.Item.MinilandObjectPoint / 2;
                        break;

                    case InventoryType.Equipment:
                        newItem = newItem.Item.ItemType == ItemType.Specialist ? new ItemInstance
                        {
                            ItemVNum = vnum,
                            SpLevel = 1,
                            Amount = 1
                        } : new ItemInstance
                        {
                            ItemVNum = vnum,
                            Amount = 1,
                            DurabilityPoint = newItem.Item.Effect != 790 && (newItem.Item.EffectValue < 863 || newItem.Item.EffectValue > 872) && !new int[] { 3951, 3952, 3953, 3954, 3955, 7427, 4544, 4294, 852, 1025 }.Contains(newItem.Item.EffectValue) ? newItem.Item.EffectValue : 0
                        };
                        break;
                }
            }

            switch (vnum)
            {
                case 990:
                case 997:
                case 991:
                case 996:
                case 992:
                case 995:
                    newItem.IsPartnerEquipment = true;
                    newItem.HoldingVNum = (short)(vnum == 990 ? 18 : vnum == 991 ? 32 : vnum == 992 ? 46 : vnum == 997 ? 94 : vnum == 996 ? 107 : 120);
                    break;
            }

            // set default itemType
            if (newItem.Item != null)
            {
                newItem.Type = newItem.Item.Type;
            }

            return newItem;
        }

        public ItemInstance AddIntoBazaarInventory(InventoryType inventory, byte slot, short amount)
        {
            ItemInstance inv = LoadBySlotAndType(slot, inventory);
            if (inv == null || amount > inv.Amount)
            {
                return null;
            }

            ItemInstance invcopy = inv.DeepCopy();
            invcopy.Id = Guid.NewGuid();
            if (inv.Item.Type == InventoryType.Equipment)
            {
                for (short i = 0; i < 255; i++)
                {
                    if (LoadBySlotAndType(i, InventoryType.Bazaar) == null)
                    {
                        invcopy.Type = InventoryType.Bazaar;
                        invcopy.Slot = i;
                        invcopy.CharacterId = Owner.CharacterId;
                        DeleteFromSlotAndType(inv.Slot, inv.Type);
                        PutItem(invcopy);
                        break;
                    }
                }
                Owner.Session.SendPacket(UserInterfaceHelper.Instance.GenerateInventoryRemove(inventory, slot));
                return invcopy;
            }
            if (amount >= inv.Amount)
            {
                for (short i = 0; i < 255; i++)
                {
                    if (LoadBySlotAndType(i, InventoryType.Bazaar) == null)
                    {
                        invcopy.Type = InventoryType.Bazaar;
                        invcopy.Slot = i;
                        invcopy.CharacterId = Owner.CharacterId;
                        DeleteFromSlotAndType(inv.Slot, inv.Type);
                        PutItem(invcopy);
                        break;
                    }
                }
                Owner.Session.SendPacket(UserInterfaceHelper.Instance.GenerateInventoryRemove(inventory, slot));
                return invcopy;
            }

            invcopy.Amount = amount;
            inv.Amount -= amount;

            for (short i = 0; i < 255; i++)
            {
                if (LoadBySlotAndType(i, InventoryType.Bazaar) == null)
                {
                    invcopy.Type = InventoryType.Bazaar;
                    invcopy.Slot = i;
                    invcopy.CharacterId = Owner.CharacterId;
                    PutItem(invcopy);
                    break;
                }
            }

            Owner.Session.SendPacket(inv.GenerateInventoryAdd());
            return invcopy;
        }

        public List<ItemInstance> AddNewToInventory(short vnum, short amount = 1, InventoryType? type = null, sbyte rare = 0, byte upgrade = 0, short design = 0,
            bool addStartingShell = false, bool addStartingShell2 = false, bool addStartingShell3 = false)
        {
            if (Owner != null)
            {
                ItemInstance newItem = InstantiateItemInstance(vnum, Owner.CharacterId, amount);
                newItem.Rare = rare;
                newItem.Upgrade = upgrade == 0 ? newItem.Item.ItemType == ItemType.Shell ? (byte)ServerManager.RandomNumber(50, 80) : upgrade : upgrade;
                newItem.Design = design;

                switch (newItem.Item.EquipmentSlot)
                {
                    case EquipmentType.Boots:
                    case EquipmentType.Gloves:
                        newItem.FireResistance = (short)(newItem.Item.FireResistance * upgrade);
                        newItem.DarkResistance = (short)(newItem.Item.DarkResistance * upgrade);
                        newItem.LightResistance = (short)(newItem.Item.LightResistance * upgrade);
                        newItem.WaterResistance = (short)(newItem.Item.WaterResistance * upgrade);
                        break;
                }

                if (addStartingShell)
                {
                    newItem.ShellEffects.Add(new ShellEffectDTO
                    {
                        EffectLevel = ShellEffectLevelType.ANormal,
                        Effect = 26,
                        Value = 10,
                        EquipmentSerialId = newItem.EquipmentSerialId
                    });

                    newItem.ShellEffects.Add(new ShellEffectDTO
                    {
                        EffectLevel = ShellEffectLevelType.ANormal,
                        Effect = 29,
                        Value = 10,
                        EquipmentSerialId = newItem.EquipmentSerialId
                    });

                    newItem.ShellEffects.Add(new ShellEffectDTO
                    {
                        EffectLevel = ShellEffectLevelType.SNormal,
                        Effect = 30,
                        Value = 10,
                        EquipmentSerialId = newItem.EquipmentSerialId
                    });

                    newItem.ShellEffects.Add(new ShellEffectDTO
                    {
                        EffectLevel = ShellEffectLevelType.SPVP,
                        Effect = 34,
                        Value = 30,
                        EquipmentSerialId = newItem.EquipmentSerialId
                    });

                    newItem.ShellEffects.Add(new ShellEffectDTO
                    {
                        EffectLevel = ShellEffectLevelType.SPVP,
                        Effect = 35,
                        Value = 30,
                        EquipmentSerialId = newItem.EquipmentSerialId
                    });
                }

                if (addStartingShell2)
                {
                    newItem.ShellEffects.Add(new ShellEffectDTO
                    {
                        EffectLevel = ShellEffectLevelType.SNormal,
                        Effect = 54,
                        Value = 10,
                        EquipmentSerialId = newItem.EquipmentSerialId
                    });

                    newItem.ShellEffects.Add(new ShellEffectDTO
                    {
                        EffectLevel = ShellEffectLevelType.SPVP,
                        Effect = 83,
                        Value = 20,
                        EquipmentSerialId = newItem.EquipmentSerialId
                    });

                    newItem.ShellEffects.Add(new ShellEffectDTO
                    {
                        EffectLevel = ShellEffectLevelType.SPVP,
                        Effect = 87,
                        Value = 20,
                        EquipmentSerialId = newItem.EquipmentSerialId
                    });
                }

                if (addStartingShell3)
                {
                    var option = new CellonOptionDTO
                    {
                        EquipmentSerialId = newItem.EquipmentSerialId,
                        Level = 8,
                        Type = CellonOptionType.HPMax,
                        Value = 1100
                    };

                    var option2 = new CellonOptionDTO
                    {
                        EquipmentSerialId = newItem.EquipmentSerialId,
                        Level = 8,
                        Type = CellonOptionType.MPMax,
                        Value = 1100
                    };

                    var option3 = new CellonOptionDTO
                    {
                        EquipmentSerialId = newItem.EquipmentSerialId,
                        Level = 8,
                        Type = CellonOptionType.CritReduce,
                        Value = 30
                    };

                    newItem.CellonOptions.Add(option);
                    newItem.CellonOptions.Add(option2);
                    newItem.CellonOptions.Add(option3);
                    DAOFactory.CellonOptionDAO.InsertOrUpdate(option);
                    DAOFactory.CellonOptionDAO.InsertOrUpdate(option2);
                    DAOFactory.CellonOptionDAO.InsertOrUpdate(option3);
                }

                return AddToInventory(newItem, type);
            }
            return new List<ItemInstance>();
        }

        public List<ItemInstance> AddToInventory(ItemInstance newItem, InventoryType? type = null)
        {
            List<ItemInstance> invlist = new List<ItemInstance>();
            if (Owner != null)
            {
                ItemInstance inv = null;

                // override type if necessary
                if (type.HasValue)
                {
                    newItem.Type = type.Value;
                }

                if (newItem.Item.Effect == 420 && newItem.Item.EffectValue == 911)
                {
                    newItem.BoundCharacterId = Owner.CharacterId;
                    newItem.DurabilityPoint = (int)newItem.Item.ItemValidTime;
                }

                // check if item can be stapled
                if (newItem.Type != InventoryType.Bazaar && (newItem.Item.Type == InventoryType.Etc || newItem.Item.Type == InventoryType.Main))
                {
                    List<ItemInstance> slotNotFull = Values.Where(i => i.Type != InventoryType.Bazaar && i.Type != InventoryType.PetWarehouse && i.Type != InventoryType.Warehouse && i.Type != InventoryType.FamilyWareHouse && i.ItemVNum.Equals(newItem.ItemVNum) && i.Amount < MaxItemAmount).ToList();
                    int freeslot = BackpackSize() - Values.Count(s => s.Type == newItem.Type);
                    if (freeslot < 0) freeslot = 0;
                    if (newItem.Amount <= (freeslot * MaxItemAmount) + slotNotFull.Sum(s => MaxItemAmount - s.Amount))
                    {
                        foreach (ItemInstance slot in slotNotFull)
                        {
                            int max = slot.Amount + newItem.Amount;
                            max = max > MaxItemAmount ? MaxItemAmount : max;
                            newItem.Amount = (short)(slot.Amount + newItem.Amount - max);
                            newItem.Amount = (short)(newItem.Amount < 0 ? 0 : newItem.Amount);
                            Logger.Log.LogUserEvent("ITEM_CREATE", Owner.GenerateIdentity(), $"IIId: {slot.Id} ItemVNum: {slot.ItemVNum} Amount: {max - slot.Amount} MapId: {Owner.MapInstance?.Map.MapId} MapX: {Owner.PositionX} MapY: {Owner.PositionY}");

                            slot.Amount = (short)max;
                            invlist.Add(slot);
                            Owner.Session?.SendPacket(slot.GenerateInventoryAdd());
                        }
                    }
                }
                if (newItem.Amount > 0)
                {
                    // create new item
                    short? freeSlot = newItem.Type == InventoryType.Wear ? (LoadBySlotAndType((short)newItem.Item.EquipmentSlot, InventoryType.Wear) == null
                                                                         ? (short?)newItem.Item.EquipmentSlot
                                                                         : null)
                                                                         : GetFreeSlot(newItem.Type);
                    if (freeSlot.HasValue)
                    {
                        inv = AddToInventoryWithSlotAndType(newItem, newItem.Type, freeSlot.Value);
                        invlist.Add(inv);
                    }
                }
            }
            return invlist;
        }

        /// <summary>
        /// Add iteminstance to inventory with specified slot and type, iteminstance will be overridden.
        /// </summary>
        /// <param name="itemInstance"></param>
        /// <param name="type"></param>
        /// <param name="slot"></param>
        /// <returns></returns>
        public ItemInstance AddToInventoryWithSlotAndType(ItemInstance itemInstance, InventoryType type, short slot)
        {
            if (Owner != null)
            {
                Logger.Log.LogUserEvent("ITEM_CREATE", Owner.GenerateIdentity(), $"IIId: {itemInstance.Id} ItemVNum: {itemInstance.ItemVNum} Amount: {itemInstance.Amount} MapId: {Owner.MapInstance?.Map.MapId} MapX: {Owner.PositionX} MapY: {Owner.PositionY}");

                itemInstance.Slot = slot;
                itemInstance.Type = type;
                itemInstance.CharacterId = Owner.CharacterId;

                if (ContainsKey(itemInstance.Id))
                {
                    Logger.Log.Error(null, new InvalidOperationException("Cannot add the same ItemInstance twice to inventory."));
                    return null;
                }

                string inventoryPacket = itemInstance.GenerateInventoryAdd();
                if (!string.IsNullOrEmpty(inventoryPacket))
                {
                    Owner.Session?.SendPacket(inventoryPacket);
                }

                if (Values.Any(s => s.Slot == slot && s.Type == type))
                {
                    return null;
                }
                this[itemInstance.Id] = itemInstance;
                return itemInstance;
            }
            return null;
        }

        public int BackpackSize()
        {
            var backPackExtension = 0;

            if (Owner.HasStaticBonus(StaticBonusType.EreniaMedal))
            {
                backPackExtension += 8;
            }

            if (Owner.HasStaticBonus(StaticBonusType.AdventurerMedal))
            {
                backPackExtension += 4;
            }

            var backPackCapacity = DefaultBackpackSize + ((Owner.HaveBackpack() ? 1 : 0) * 12) + ((Owner.HaveExtension() ? 1 : 0) * 60) + backPackExtension;

            return backPackCapacity > 120 ? 120 : backPackCapacity;
        }

        public bool CanAddItem(short itemVnum) => CanAddItem(ServerManager.GetItem(itemVnum).Type);

        public int CountItem(int itemVNum) => Values.Where(s => s.ItemVNum == itemVNum && s.Type != InventoryType.Wear && s.Type != InventoryType.FamilyWareHouse && s.Type != InventoryType.Bazaar && s.Type != InventoryType.Warehouse && s.Type != InventoryType.PetWarehouse).Sum(i => i.Amount);

        public int CountItemInAnInventory(InventoryType inv)
        {
            return Values.Count(s => s.Type == inv);
        }

        public int CountItemInInventory(int itemVNum, short slot) => Values.Where(s => s.ItemVNum == itemVNum && s.Slot == slot && s.Type != InventoryType.Wear && s.Type != InventoryType.FamilyWareHouse && s.Type != InventoryType.Bazaar && s.Type != InventoryType.Warehouse && s.Type != InventoryType.PetWarehouse).Sum(i => i.Amount);

        public Tuple<short, InventoryType> DeleteById(Guid id)
        {
            if (Owner != null && ContainsKey(id))
            {
                Tuple<short, InventoryType> removedPlace;
                ItemInstance inv = this[id];

                if (inv != null)
                {
                    removedPlace = new Tuple<short, InventoryType>(inv.Slot, inv.Type);
                    TryRemove(inv.Id, out _);
                }
                else
                {
                    Logger.Log.Error(null, new InvalidOperationException("Expected item wasn't deleted, Type or Slot did not match!"));
                    return null;
                }

                return removedPlace;
            }
            return null;
        }

        public void DeleteFromSlotAndType(short slot, InventoryType type)
        {
            if (Owner != null)
            {
                ItemInstance inv = Values.FirstOrDefault(i => i.Slot.Equals(slot) && i.Type.Equals(type));

                if (inv != null)
                {
                    if (Owner.Session.Character.MinilandObjects.Any(s => s.ItemInstanceId == inv.Id))
                    {
                        return;
                    }

                    TryRemove(inv.Id, out _);
                }
                else
                {
                    Logger.Log.Error(null, new InvalidOperationException("Expected item wasn't deleted, Type or Slot did not match!"));
                }
            }
        }

        public void DepositItem(InventoryType inventory, byte slot, short amount, byte newSlot, ref ItemInstance item, ref ItemInstance itemdest, bool partnerBackpack)
        {
            if (item == null || amount > item.Amount || amount <= 0)
            {
                return;
            }

            MoveItem(inventory, partnerBackpack ? InventoryType.PetWarehouse : InventoryType.Warehouse, slot, amount, newSlot, out item, out itemdest);
            Owner.Session.SendPacket(item != null ? item.GenerateInventoryAdd() : UserInterfaceHelper.Instance.GenerateInventoryRemove(inventory, slot));

            if (itemdest != null)
            {
                Owner.Session.SendPacket(partnerBackpack ? itemdest.GeneratePStash() : itemdest.GenerateStash());
            }
        }

        public bool EnoughPlace(List<ItemInstance> itemInstances)
        {
            Dictionary<InventoryType, int> place = new Dictionary<InventoryType, int>();
            foreach (IGrouping<short, ItemInstance> itemgroup in itemInstances.GroupBy(s => s.ItemVNum))
            {
                if (itemgroup.FirstOrDefault()?.Type is InventoryType type)
                {
                    List<ItemInstance> listitem = Values.Where(i => i.Type == type).ToList();
                    if (!place.ContainsKey(type))
                    {
                        place.Add(type, (type != InventoryType.Miniland ? BackpackSize() : 50) - listitem.Count);
                    }

                    int amount = itemgroup.Sum(s => s.Amount);
                    int rest = amount % (type == InventoryType.Equipment ? 1 : MaxItemAmount);
                    bool needanotherslot = listitem.Where(s => s.ItemVNum == itemgroup.Key).Sum(s => MaxItemAmount - s.Amount) <= rest;
                    place[type] -= (amount / (type == InventoryType.Equipment ? 1 : MaxItemAmount)) + (needanotherslot ? 1 : 0);

                    if (place[type] < 0)
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        public void FDepositItem(InventoryType inventory, byte slot, short amount, byte newSlot, ref ItemInstance item, ref ItemInstance itemdest)
        {
            if (item != null && amount <= item.Amount && amount > 0 && item.Item.IsTradable && !item.IsBound)
            {
                FamilyCharacter fhead = Owner.Family?.FamilyCharacters.Find(s => s.Authority == FamilyAuthority.Head);
                if (fhead == null)
                {
                    return;
                }
                MoveItem(inventory, InventoryType.FamilyWareHouse, slot, amount, newSlot, out item, out itemdest);
                itemdest.CharacterId = fhead.CharacterId;
                DAOFactory.ItemInstanceDAO.InsertOrUpdate(itemdest);
                Owner.Family.SendPacket(item != null ? item.GenerateInventoryAdd() : UserInterfaceHelper.Instance.GenerateInventoryRemove(inventory, slot));
                if (itemdest != null)
                {
                    Owner.Family.SendPacket(itemdest.GenerateFStash());
                    Owner.Family?.InsertFamilyLog(FamilyLogType.WareHouseAdded, Owner.Name, message: $"{itemdest.ItemVNum}|{amount}");
                    DeleteById(itemdest.Id);
                }
            }
        }

        public ItemInstance GetItemInstanceById(Guid id)
        {
            if (!ContainsKey(id))
            {
                return null;
            }

            return this[id];
        } 

        public ItemInstance LoadBySlotAndType(short? slot, InventoryType type)
        {
            ItemInstance retItem = null;

            try
            {
                lock (_lockObject)
                {
                    retItem = Values.SingleOrDefault(i => i.Slot.Equals(slot) && i.Type.Equals(type));
                }
            }
            catch (InvalidOperationException ioEx)
            {
                Logger.Log.Debug("Multiple items in slot, Splitting...");

                bool isFirstItem = true;

                foreach (ItemInstance item in Values.Where(i => i.Slot.Equals(slot) && i.Type.Equals(type)))
                {
                    if (isFirstItem)
                    {
                        retItem = item;
                        isFirstItem = false;
                        continue;
                    }

                    ItemInstance itemInstance = Values.FirstOrDefault(i => i.Slot.Equals(slot) && i.Type.Equals(type));

                    if (itemInstance != null)
                    {
                        short? freeSlot = GetFreeSlot(type);

                        if (freeSlot.HasValue)
                        {
                            itemInstance.Slot = freeSlot.Value;
                        }
                        else
                        {
                            TryRemove(itemInstance.Id, out _);
                        }
                    }
                }
            }
            return retItem;
        }

        public ItemInstance LoadByVNum(short vNum) => Values.FirstOrDefault(i => i.ItemVNum.Equals(vNum));

        /// <summary>
        /// Moves one item from one <see cref="Inventory"/> to another. Example: Equipment &lt;-&gt;
        /// Wear, Equipment &lt;-&gt; Costume, Equipment &lt;-&gt; Specialist
        /// </summary>
        /// <param name="sourceSlot"></param>
        /// <param name="sourceType"></param>
        /// <param name="targetType"></param>
        /// <param name="targetSlot"></param>
        /// <param name="wear"></param>
        public ItemInstance MoveInInventory(short sourceSlot, InventoryType sourceType, InventoryType targetType, short? targetSlot = null, bool wear = true)
        {
            ItemInstance sourceInstance = LoadBySlotAndType(sourceSlot, sourceType);

            if (sourceInstance == null && wear)
            {
                Logger.Log.Error(null, new InvalidOperationException("SourceInstance to move does not exist."));
                return null;
            }
            if (Owner != null && sourceInstance != null)
            {
                if (targetSlot.HasValue)
                {
                    if (wear)
                    {
                        // swap
                        ItemInstance targetInstance = LoadBySlotAndType(targetSlot.Value, targetType);

                        sourceInstance.Slot = targetSlot.Value;
                        sourceInstance.Type = targetType;

                        targetInstance.Slot = sourceSlot;
                        targetInstance.Type = sourceType;
                    }
                    else
                    {
                        // move source to target
                        short? freeTargetSlot = GetFreeSlot(targetType);
                        if (freeTargetSlot.HasValue)
                        {
                            sourceInstance.Slot = freeTargetSlot.Value;
                            sourceInstance.Type = targetType;
                        }
                    }

                    return sourceInstance;
                }

                // check for free target slot
                short? nextFreeSlot;
                switch (targetType)
                {
                    case InventoryType.FirstPartnerInventory:
                    case InventoryType.SecondPartnerInventory:
                    case InventoryType.ThirdPartnerInventory:
                    case InventoryType.FourthPartnerInventory:
                    case InventoryType.FifthPartnerInventory:
                    case InventoryType.SixthPartnerInventory:
                    case InventoryType.SeventhPartnerInventory:
                    case InventoryType.EighthPartnerInventory:
                    case InventoryType.NinthPartnerInventory:
                    case InventoryType.TenthPartnerInventory:
                    case InventoryType.Wear:
                        nextFreeSlot = (LoadBySlotAndType((short)sourceInstance.Item.EquipmentSlot, targetType) == null
                        ? (short)sourceInstance.Item.EquipmentSlot
                        : (short)-1);
                        break;

                    default:
                        nextFreeSlot = GetFreeSlot(targetType);
                        break;
                }
                if (nextFreeSlot.HasValue)
                {
                    sourceInstance.Type = targetType;
                    sourceInstance.Slot = nextFreeSlot.Value;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }

            return sourceInstance;
        }

        public Inventory DeepCopy() => (Inventory) MemberwiseClone();

        public void MoveItem(InventoryType sourcetype, InventoryType desttype, short sourceSlot, short amount, short destinationSlot, out ItemInstance sourceInventory, out ItemInstance destinationInventory)
        {
            Logger.Log.LogUserEvent("ITEM_MOVE", Owner.GenerateIdentity(), $"SourceType: {sourcetype.ToString()} DestType: {desttype.ToString()} SourceSlot: {sourceSlot} Amount: {amount} DestSlot: {destinationSlot}");

            // Load source and destination slots
            sourceInventory = LoadBySlotAndType(sourceSlot, sourcetype);
            destinationInventory = LoadBySlotAndType(destinationSlot, desttype);
            if (sourceInventory != null && amount <= sourceInventory.Amount)
            {
                if (destinationInventory == null)
                {
                    if (sourceInventory.Amount == amount)
                    {
                        sourceInventory.Slot = destinationSlot;
                        sourceInventory.Type = desttype;
                    }
                    else
                    {
                        ItemInstance itemDest = sourceInventory.DeepCopy();
                        sourceInventory.Amount -= amount;
                        itemDest.Amount = amount;
                        itemDest.Type = desttype;
                        itemDest.Id = Guid.NewGuid();
                        AddToInventoryWithSlotAndType(itemDest, desttype, destinationSlot);
                    }
                }
                else
                {
                    if (destinationInventory.ItemVNum == sourceInventory.ItemVNum && (byte)sourceInventory.Item.Type != 0)
                    {
                        if (destinationInventory.Amount + amount > MaxItemAmount)
                        {
                            int saveItemCount = destinationInventory.Amount;
                            destinationInventory.Amount = MaxItemAmount;
                            sourceInventory.Amount = (short)(saveItemCount + sourceInventory.Amount - MaxItemAmount);
                        }
                        else
                        {
                            destinationInventory.Amount += amount;
                            sourceInventory.Amount -= amount;

                            // item with amount of 0 should be removed
                            if (sourceInventory.Amount == 0)
                            {
                                DeleteFromSlotAndType(sourceInventory.Slot, sourceInventory.Type);
                            }
                        }
                    }
                    else
                    {
                        if (destinationInventory.Type == sourceInventory.Type || destinationInventory.Item.Type == sourcetype)
                        {
                            // add and remove save inventory
                            destinationInventory = TakeItem(destinationInventory.Slot, destinationInventory.Type);
                            if (destinationInventory == null)
                            {
                                return;
                            }

                            destinationInventory.Slot = sourceSlot;
                            destinationInventory.Type = sourcetype;
                        }
                        else
                        {
                            if (GetFreeSlot(destinationInventory.Item.Type) is short freeSlot)
                            {
                                destinationInventory.Slot = freeSlot;
                                destinationInventory.Type = destinationInventory.Item.Type;
                                Owner.Session.SendPacket(destinationInventory.GenerateInventoryAdd());

                                // add and remove save inventory
                                destinationInventory = TakeItem(destinationInventory.Slot, destinationInventory.Type);
                                if (destinationInventory == null)
                                {
                                    return;
                                }
                            }
                            else
                            {
                                Owner.Session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("NOT_ENOUGH_PLACE"), 0));
                                return;
                            }
                        }

                        sourceInventory = TakeItem(sourceInventory.Slot, sourceInventory.Type);
                        if (sourceInventory == null)
                        {
                            return;
                        }

                        sourceInventory.Slot = destinationSlot;
                        sourceInventory.Type = desttype;
                        PutItem(destinationInventory);
                        PutItem(sourceInventory);
                    }
                }
            }
            sourceInventory = LoadBySlotAndType(sourceSlot, sourcetype);
            destinationInventory = LoadBySlotAndType(destinationSlot, desttype);
        }

        public void RemoveItemAmount(int vnum, int amount = 1)
        {
            if (Owner == null)
            {
                return;
            }

            int remainingAmount = amount;

            foreach (var item in Values.Where(s => s.ItemVNum == vnum && s.Type != InventoryType.Wear && s.Type != InventoryType.Bazaar && s.Type != InventoryType.Warehouse && s.Type != InventoryType.PetWarehouse && s.Type != InventoryType.FamilyWareHouse).OrderBy(i => i.Slot))
            {
                if (remainingAmount > 0)
                {
                    if (item.Amount > remainingAmount)
                    {
                        // Amount completely removed
                        item.Amount -= (short)remainingAmount;
                        remainingAmount = 0;
                        Owner.Session.SendPacket(item.GenerateInventoryAdd());
                    }
                    else
                    {
                        // Amount partly removed
                        remainingAmount -= item.Amount;
                        DeleteById(item.Id);
                        Owner.Session.SendPacket(UserInterfaceHelper.Instance.GenerateInventoryRemove(item.Type, item.Slot));
                    }
                }
                else
                {
                    // Amount to remove reached
                    break;
                }
            }
        }

        public void RemoveItemFromInventory(Guid id, short amount = 1)
        {
            if (Owner != null)
            {
                ItemInstance inv = Values.FirstOrDefault(i => i.Id.Equals(id));
                if (inv != null)
                {
                    inv.Amount -= amount;
                    if (inv.Amount <= 0)
                    {
                        Owner.Session.SendPacket(UserInterfaceHelper.Instance.GenerateInventoryRemove(inv.Type, inv.Slot));
                        TryRemove(inv.Id, out _);
                        return;
                    }
                    Owner.Session.SendPacket(inv.GenerateInventoryAdd());
                }
            }
        }

        /// <summary>
        /// Reorders item in given inventorytype
        /// </summary>
        /// <param name="session"></param>
        /// <param name="inventoryType"></param>
        public void Reorder(ClientSession session, InventoryType inventoryType)
        {
            List<ItemInstance> itemsByInventoryType = new List<ItemInstance>();
            switch (inventoryType)
            {
                case InventoryType.Costume:
                    itemsByInventoryType = Values.Where(s => s.Type == InventoryType.Costume).OrderBy(s => s.ItemVNum).ToList();
                    break;

                case InventoryType.Specialist:
                    itemsByInventoryType = Values.Where(s => s.Type == InventoryType.Specialist).OrderBy(s => s.Item.LevelJobMinimum).ToList();
                    break;

                default:
                    itemsByInventoryType = Values.Where(s => s.Type == inventoryType).OrderBy(s => s.ItemVNum).ToList();
                    break;
            }
            GenerateClearInventory(inventoryType);
            for (short i = 0; i < itemsByInventoryType.Count; i++)
            {
                ItemInstance item = itemsByInventoryType[i];
                item.Slot = i;
                this[item.Id].Slot = i;
                session.SendPacket(item.GenerateInventoryAdd());
            }
        }

        private bool CanAddItem(InventoryType type) => Owner != null && GetFreeSlot(type).HasValue;

        private void GenerateClearInventory(InventoryType type)
        {
            if (Owner != null)
            {
                for (short i = 0; i < BackpackSize(); i++)
                {
                    if (LoadBySlotAndType(i, type) != null)
                    {
                        Owner.Session.SendPacket(UserInterfaceHelper.Instance.GenerateInventoryRemove(type, i));
                    }
                }
            }
        }

        /// <summary>
        /// Gets free slots in given inventory type
        /// </summary>
        /// <param name="type"></param>
        /// <returns>short?; based on given inventory type</returns>
        private short? GetFreeSlot(InventoryType type)
        {
            IEnumerable<int> itemInstanceSlotsByType = Values.Where(i => i.Type == type).OrderBy(i => i.Slot).Select(i => (int)i.Slot);
            IEnumerable<int> instanceSlotsByType = itemInstanceSlotsByType as int[] ?? itemInstanceSlotsByType.ToArray();
            int backpackSize = BackpackSize();
            int maxRange = (type != InventoryType.Miniland ? backpackSize : 50) + 1;
            int? nextFreeSlot = instanceSlotsByType.Any() ? Enumerable.Range(0, maxRange).Except(instanceSlotsByType).Cast<int?>().FirstOrDefault() : 0;
            return (short?)nextFreeSlot < (type != InventoryType.Miniland ? backpackSize : 50) ? (short?)nextFreeSlot : null;
        }

        /// <summary>
        /// Puts a Single ItemInstance to the Inventory
        /// </summary>
        /// <param name="itemInstance"></param>
        private void PutItem(ItemInstance itemInstance) => this[itemInstance.Id] = itemInstance;

        /// <summary>
        /// Takes a Single Inventory including ItemInstance from the List and removes it.
        /// </summary>
        /// <param name="slot"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private ItemInstance TakeItem(short slot, InventoryType type)
        {
            ItemInstance itemInstance = Values.SingleOrDefault(i => i.Slot == slot && i.Type == type);
            if (itemInstance != null)
            {
                TryRemove(itemInstance.Id, out _);
                return itemInstance;
            }
            return null;
        }

        #endregion
    }
}