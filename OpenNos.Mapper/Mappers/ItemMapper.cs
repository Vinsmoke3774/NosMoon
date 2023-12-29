
using System;
using System.Collections.Generic;
using OpenNos.Core.Logger;
using OpenNos.DAL.EF;
using OpenNos.Data;
using OpenNos.Domain;

namespace OpenNos.Mapper.Mappers
{
    public class ItemMapper : IMapper<ItemDTO, Item>
    {
        public Item Map(ItemDTO input)
        {
            try
            {
                if (input == null)
                {
                    Logger.Log("ItemDTO input is null.");
                    return null;
                }

                var item = new Item
                {
                    BasicUpgrade = input.BasicUpgrade,
                    CellonLvl = input.CellonLvl,
                    Class = input.Class,
                    CloseDefence = input.CloseDefence,
                    Color = input.Color,
                    Concentrate = input.Concentrate,
                    CriticalLuckRate = input.CriticalLuckRate,
                    CriticalRate = input.CriticalRate,
                    DamageMaximum = input.DamageMaximum,
                    DamageMinimum = input.DamageMinimum,
                    DarkElement = input.DarkElement,
                    DarkResistance = input.DarkResistance,
                    DefenceDodge = input.DefenceDodge,
                    DistanceDefence = input.DistanceDefence,
                    DistanceDefenceDodge = input.DistanceDefenceDodge,
                    Effect = input.Effect,
                    EffectValue = input.EffectValue,
                    Element = input.Element,
                    ElementRate = input.ElementRate,
                    EquipmentSlot = input.EquipmentSlot,
                    FireElement = input.FireElement,
                    FireResistance = input.FireResistance,
                    Height = input.Height,
                    HitRate = input.HitRate,
                    Hp = input.Hp,
                    HpRegeneration = input.HpRegeneration,
                    IsBlocked = input.IsBlocked,
                    IsColored = input.IsColored,
                    IsConsumable = input.IsConsumable,
                    IsDroppable = input.IsDroppable,
                    IsHeroic = input.IsHeroic,
                    IsHolder = input.IsHolder,
                    IsMinilandObject = input.IsMinilandObject,
                    IsSoldable = input.IsSoldable,
                    IsTradable = input.IsTradable,
                    ItemSubType = input.ItemSubType,
                    ItemType = (byte)input.ItemType,
                    ItemValidTime = input.ItemValidTime,
                    LevelJobMinimum = input.LevelJobMinimum,
                    LevelMinimum = input.LevelMinimum,
                    LightElement = input.LightElement,
                    LightResistance = input.LightResistance,
                    MagicDefence = input.MagicDefence,
                    MaxCellon = input.MaxCellon,
                    MaxCellonLvl = input.MaxCellonLvl,
                    MaxElementRate = input.MaxElementRate,
                    MaximumAmmo = input.MaximumAmmo,
                    MinilandObjectPoint = input.MinilandObjectPoint,
                    MoreHp = input.MoreHp,
                    MoreMp = input.MoreMp,
                    Morph = input.Morph,
                    Mp = input.Mp,
                    MpRegeneration = input.MpRegeneration,
                    Name = input.Name,
                    Price = input.Price,
                    SellToNpcPrice = input.SellToNpcPrice,
                    PvpDefence = input.PvpDefence,
                    PvpStrength = input.PvpStrength,
                    ReduceOposantResistance = input.ReduceOposantResistance,
                    ReputationMinimum = input.ReputationMinimum,
                    ReputPrice = input.ReputPrice,
                    SecondaryElement = input.SecondaryElement,
                    Sex = input.Sex,
                    Speed = input.Speed,
                    SpType = input.SpType,
                    Type = (byte)input.Type,
                    VNum = input.VNum,
                    WaitDelay = input.WaitDelay,
                    WaterElement = input.WaterElement,
                    WaterResistance = input.WaterResistance,
                    Width = input.Width,
                    MorphSp = input.MorphSp,
                    IsFamily = input.IsFamily,
                    FamilyRank = input.FamilyRank,
                    FamilyLevel = input.FamilyLevel,
                    TsMapId = input.TsMapId
                };

                Logger.Log("Mapping completed successfully for ItemDTO with ID: " + input.VNum);
                return item;
            }
            catch (Exception ex)
            {
                Logger.LogError("Error during mapping ItemDTO with ID: " + input?.VNum, ex);
                throw;
            }
        }

        public ItemDTO Map(Item input)
        {
            try
            {
                if (input == null)
                {
                    Logger.Log("Item input is null.");
                    return null;
                }

                var itemDto = new ItemDTO
                {
                    BasicUpgrade = input.BasicUpgrade,
                    CellonLvl = input.CellonLvl,
                    Class = input.Class,
                    CloseDefence = input.CloseDefence,
                    Color = input.Color,
                    Concentrate = input.Concentrate,
                    CriticalLuckRate = input.CriticalLuckRate,
                    CriticalRate = input.CriticalRate,
                    DamageMaximum = input.DamageMaximum,
                    DamageMinimum = input.DamageMinimum,
                    DarkElement = input.DarkElement,
                    DarkResistance = input.DarkResistance,
                    DefenceDodge = input.DefenceDodge,
                    DistanceDefence = input.DistanceDefence,
                    DistanceDefenceDodge = input.DistanceDefenceDodge,
                    Effect = input.Effect,
                    EffectValue = input.EffectValue,
                    Element = input.Element,
                    ElementRate = input.ElementRate,
                    EquipmentSlot = input.EquipmentSlot,
                    FireElement = input.FireElement,
                    FireResistance = input.FireResistance,
                    Height = input.Height,
                    HitRate = input.HitRate,
                    Hp = input.Hp,
                    HpRegeneration = input.HpRegeneration,
                    IsBlocked = input.IsBlocked,
                    IsColored = input.IsColored,
                    IsConsumable = input.IsConsumable,
                    IsDroppable = input.IsDroppable,
                    IsHeroic = input.IsHeroic,
                    IsHolder = input.IsHolder,
                    IsMinilandObject = input.IsMinilandObject,
                    IsSoldable = input.IsSoldable,
                    IsTradable = input.IsTradable,
                    ItemSubType = input.ItemSubType,
                    ItemType = (ItemType)input.ItemType,
                    ItemValidTime = input.ItemValidTime,
                    LevelJobMinimum = input.LevelJobMinimum,
                    LevelMinimum = input.LevelMinimum,
                    LightElement = input.LightElement,
                    LightResistance = input.LightResistance,
                    MagicDefence = input.MagicDefence,
                    MaxCellon = input.MaxCellon,
                    MaxCellonLvl = input.MaxCellonLvl,
                    MaxElementRate = input.MaxElementRate,
                    MaximumAmmo = input.MaximumAmmo,
                    MinilandObjectPoint = input.MinilandObjectPoint,
                    MoreHp = input.MoreHp,
                    MoreMp = input.MoreMp,
                    Morph = input.Morph,
                    Mp = input.Mp,
                    MpRegeneration = input.MpRegeneration,
                    Name = input.Name,
                    Price = input.Price,
                    SellToNpcPrice = input.SellToNpcPrice,
                    PvpDefence = input.PvpDefence,
                    PvpStrength = input.PvpStrength,
                    ReduceOposantResistance = input.ReduceOposantResistance,
                    ReputationMinimum = input.ReputationMinimum,
                    ReputPrice = input.ReputPrice,
                    SecondaryElement = input.SecondaryElement,
                    Sex = input.Sex,
                    Speed = input.Speed,
                    SpType = input.SpType,
                    Type = (InventoryType)input.Type,
                    VNum = input.VNum,
                    WaitDelay = input.WaitDelay,
                    WaterElement = input.WaterElement,
                    WaterResistance = input.WaterResistance,
                    Width = input.Width,
                    MorphSp = input.MorphSp,
                    IsFamily = input.IsFamily,
                    FamilyRank = input.FamilyRank,
                    FamilyLevel = input.FamilyLevel,
                    TsMapId = input.TsMapId
                };

                Logger.Log("Mapping completed successfully for Item with ID: " + input.VNum);
                return itemDto;
            }
            catch (Exception ex)
            {
                Logger.LogError("Error during mapping Item with ID: " + input?.VNum, ex);
                throw;
            }
        }

        public IEnumerable<ItemDTO> Map(IEnumerable<Item> input)
        {
            var result = new List<ItemDTO>();

            foreach (var data in input)
            {
                try
                {
                    result.Add(Map(data));
                }
                catch (Exception ex)
                {
                    Logger.LogError("Error during mapping in Map(IEnumerable<Item>) for Item with ID: " + data?.VNum, ex);
                    // Decidi se continuare o interrompere il loop in base al comportamento desiderato
                }
            }

            return result;
        }

        public IEnumerable<Item> Map(IEnumerable<ItemDTO> input)
        {
            var result = new List<Item>();

            foreach (var data in input)
            {
                try
                {
                    result.Add(Map(data));
                }
                catch (Exception ex)
                {
                    Logger.LogError("Error during mapping in Map(IEnumerable<ItemDTO>) for ItemDTO with ID: " + data?.VNum, ex);
                    // Decidi se continuare o interrompere il loop in base al comportamento desiderato
                }
            }

            return result;
        }
    }
}
