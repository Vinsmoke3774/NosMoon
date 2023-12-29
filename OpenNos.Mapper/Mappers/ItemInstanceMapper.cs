using OpenNos.DAL.EF;
using OpenNos.Data;
using System;
using System.Collections.Generic;

namespace OpenNos.Mapper.Mappers
{
    public class ItemInstanceMapper : IMapper<ItemInstanceDTO, ItemInstance>
    {
        public ItemInstance Map(ItemInstanceDTO input)
        {
            if (input == null)
            {
                return null;
            }

            var result = new ItemInstance
            {
                Ammo = input.Ammo,
                Amount = input.Amount,
                BoundCharacterId = input.BoundCharacterId,
                Cellon = input.Cellon,
                CharacterId = input.CharacterId,
                CloseDefence = input.CloseDefence,
                Concentrate = input.Concentrate,
                CriticalDodge = input.CriticalDodge,
                CriticalLuckRate = input.CriticalLuckRate,
                CriticalRate = input.CriticalRate,
                DamageMaximum = input.DamageMaximum,
                DamageMinimum = input.DamageMinimum,
                DarkElement = input.DarkElement,
                DarkResistance = input.DarkResistance,
                DefenceDodge = input.DefenceDodge,
                Design = input.Design,
                DistanceDefence = input.DistanceDefence,
                DistanceDefenceDodge = input.DistanceDefenceDodge,
                DurabilityPoint = input.DurabilityPoint,
                ElementRate = input.ElementRate,
                EquipmentSerialId = input.EquipmentSerialId,
                FireElement = input.FireElement,
                FireResistance = input.FireResistance,
                HitRate = input.HitRate,
                HoldingVNum = input.HoldingVNum,
                HP = input.HP,
                Id = input.Id,
                IsEmpty = input.IsEmpty,
                IsFixed = input.IsFixed,
                IsPartnerEquipment = input.IsPartnerEquipment,
                ItemDeleteTime = input.ItemDeleteTime,
                ItemVNum = input.ItemVNum,
                LightElement = input.LightElement,
                LightResistance = input.LightResistance,
                MagicDefence = input.MagicDefence,
                MaxElementRate = input.MaxElementRate,
                MP = input.MP,
                Rare = input.Rare,
                ShellRarity = input.ShellRarity,
                SlDamage = input.SlDamage,
                SlDefence = input.SlDefence,
                SlElement = input.SlElement,
                SlHP = input.SlHP,
                Slot = input.Slot,
                SpDamage = input.SpDamage,
                SpDark = input.SpDark,
                SpDefence = input.SpDefence,
                SpElement = input.SpElement,
                SpFire = input.SpFire,
                SpHP = input.SpHP,
                SpLevel = input.SpLevel,
                SpLight = input.SpLight,
                SpStoneUpgrade = input.SpStoneUpgrade,
                SpWater = input.SpWater,
                Type = input.Type,
                Upgrade = input.Upgrade,
                WaterElement = input.WaterElement,
                WaterResistance = input.WaterResistance,
                XP = input.XP,
                IsBreaked = input.IsBreaked,
                RuneAmount = input.RuneAmount,
                FusionVnum = input.FusionVnum,
                WingsBuff = input.WingsBuff,
            };

            if (result.Id.Equals(default(Guid)))
            {
                result.Id = Guid.NewGuid();
            }

            return result;
        }

        public ItemInstanceDTO Map(ItemInstance input)
        {
            if (input == null)
            {
                return null;
            }

            var result = new ItemInstanceDTO
            {
                Ammo = input.Ammo ?? 0,
                Amount = (short)input.Amount,
                BoundCharacterId = input.BoundCharacterId,
                Cellon = input.Cellon ?? 0,
                CharacterId = input.CharacterId,
                CloseDefence = input.CloseDefence ?? 0,
                Concentrate = input.Concentrate ?? 0,
                CriticalDodge = input.CriticalDodge ?? 0,
                CriticalLuckRate = input.CriticalLuckRate ?? 0,
                CriticalRate = input.CriticalRate ?? 0,
                DamageMaximum = input.DamageMaximum ?? 0,
                DamageMinimum = input.DamageMinimum ?? 0,
                DarkElement = input.DarkElement ?? 0,
                DarkResistance = input.DarkResistance ?? 0,
                DefenceDodge = input.DefenceDodge ?? 0,
                Design = input.Design,
                DistanceDefence = input.DistanceDefence ?? 0,
                DistanceDefenceDodge = input.DistanceDefenceDodge ?? 0,
                DurabilityPoint = input.DurabilityPoint,
                ElementRate = input.ElementRate ?? 0,
                EquipmentSerialId = input.EquipmentSerialId ?? Guid.Empty,
                FireElement = input.FireElement ?? 0,
                FireResistance = input.FireResistance ?? 0,
                HitRate = input.HitRate ?? 0,
                HoldingVNum = input.HoldingVNum ?? 0,
                HP = input.HP ?? 0,
                Id = input.Id,
                IsEmpty = input.IsEmpty ?? false,
                IsFixed = input.IsFixed ?? false,
                IsPartnerEquipment = input.IsPartnerEquipment ?? false,
                ItemDeleteTime = input.ItemDeleteTime,
                ItemVNum = input.ItemVNum,
                LightElement = input.LightElement ?? 0,
                LightResistance = input.LightResistance ?? 0,
                MagicDefence = input.MagicDefence ?? 0,
                MaxElementRate = input.MaxElementRate ?? 0,
                MP = input.MP ?? 0,
                Rare = (sbyte)input.Rare,
                ShellRarity = input.ShellRarity ?? 0,
                SlDamage = input.SlDamage ?? 0,
                SlDefence = input.SlDefence ?? 0,
                SlElement = input.SlElement ?? 0,
                SlHP = input.SlHP ?? 0,
                Slot = input.Slot,
                SpDamage = input.SpDamage ?? 0,
                SpDark = input.SpDark ?? 0,
                SpDefence = input.SpDefence ?? 0,
                SpElement = input.SpElement ?? 0,
                SpFire = input.SpFire ?? 0,
                SpHP = input.SpHP ?? 0,
                SpLevel = input.SpLevel ?? 0,
                SpLight = input.SpLight ?? 0,
                SpStoneUpgrade = input.SpStoneUpgrade ?? 0,
                SpWater = input.SpWater ?? 0,
                Type = input.Type,
                Upgrade = input.Upgrade,
                WaterElement = input.WaterElement ?? 0,
                WaterResistance = input.WaterResistance ?? 0,
                XP = input.XP ?? 0,
                IsBreaked = input.IsBreaked,
                RuneAmount = input.RuneAmount,
                FusionVnum = input.FusionVnum,
                WingsBuff = input.WingsBuff,
            };

            if (result.Id.Equals(default(Guid)))
            {
                result.Id = Guid.NewGuid();
            }

            return result;
        }

        public IEnumerable<ItemInstanceDTO> Map(IEnumerable<ItemInstance> input)
        {
            var result = new List<ItemInstanceDTO>();

            foreach (var data in input)
            {
                result.Add(Map(data));
            }

            return result;
        }

        public IEnumerable<ItemInstance> Map(IEnumerable<ItemInstanceDTO> input)
        {
            var result = new List<ItemInstance>();

            foreach (var data in input)
            {
                result.Add(Map(data));
            }

            return result;
        }
    }
}