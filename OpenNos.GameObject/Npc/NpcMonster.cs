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

using System;
using System.Collections.Generic;
using OpenNos.Data;
using OpenNos.GameObject.Networking;

namespace OpenNos.GameObject.Npc
{
    public class NpcMonster : NpcMonsterDTO
    {
        #region Instantiation

        public NpcMonster()
        {
        }

        public NpcMonster(NpcMonsterDTO input)
        {
            AmountRequired = input.AmountRequired;
            AttackClass = input.AttackClass;
            AttackUpgrade = input.AttackUpgrade;
            BasicArea = input.BasicArea;
            BasicCooldown = input.BasicCooldown;
            BasicRange = input.BasicRange;
            BasicSkill = input.BasicSkill;
            Catch = input.Catch;
            CloseDefence = input.CloseDefence;
            Concentrate = input.Concentrate;
            CriticalChance = input.CriticalChance;
            CriticalRate = input.CriticalRate;
            DamageMaximum = input.DamageMaximum;
            DamageMinimum = input.DamageMinimum;
            DarkResistance = input.DarkResistance;
            DefenceDodge = input.DefenceDodge;
            DefenceUpgrade = input.DefenceUpgrade;
            DistanceDefence = input.DistanceDefence;
            DistanceDefenceDodge = input.DistanceDefenceDodge;
            Element = input.Element;
            ElementRate = input.ElementRate;
            FireResistance = input.FireResistance;
            HeroLevel = input.HeroLevel;
            IsHostile = input.IsHostile;
            JobXP = input.JobXP;
            Level = input.Level;
            LightResistance = input.LightResistance;
            MagicDefence = input.MagicDefence;
            MaxHP = input.MaxHP;
            MaxMP = input.MaxMP;
            MonsterType = input.MonsterType;
            Name = input.Name;
            NoAggresiveIcon = input.NoAggresiveIcon;
            NoticeRange = input.NoticeRange;
            NpcMonsterVNum = input.NpcMonsterVNum;
            OriginalNpcMonsterVNum = input.OriginalNpcMonsterVNum;
            Race = input.Race;
            RaceType = input.RaceType;
            RespawnTime = input.RespawnTime;
            Speed = input.Speed;
            VNumRequired = input.VNumRequired;
            WaterResistance = input.WaterResistance;
            XP = input.XP;
            EvolutionVNum = input.EvolutionVNum;
            EvolutionChance = input.EvolutionChance;
            BuffId = input.BuffId;
            IsPercent = input.IsPercent;
            TakeDamages = input.TakeDamages;
            GiveDamagePercentage = input.GiveDamagePercentage;
            GroupAttack = input.GroupAttack;
            HpData = input.HpData;
            MpData = input.MpData;
            PetInfo1 = input.PetInfo1;
            PetInfo2 = input.PetInfo2;
            PetInfo3 = input.PetInfo3;
            PetInfo4 = input.PetInfo4;
            Winfo1 = input.Winfo1;
            Winfo2 = input.Winfo2;
            Winfo3 = input.Winfo3;
            WeaponData1 = input.WeaponData1;
            WeaponData2 = input.WeaponData2;
            WeaponData3 = input.WeaponData3;
            WeaponData4 = input.WeaponData4;
            WeaponData5 = input.WeaponData5;
            WeaponData6 = input.WeaponData6;
            WeaponData7 = input.WeaponData7;
            Ainfo1 = input.Ainfo1;
            Ainfo2 = input.Ainfo2;
            ArmorData1 = input.ArmorData1;
            ArmorData2 = input.ArmorData2;
            ArmorData3 = input.ArmorData3;
            ArmorData4 = input.ArmorData4;
            ArmorData5 = input.ArmorData5;
        }

        #endregion

        #region Properties

        public List<BCard> BCards { get; set; }

        public List<DropDTO> Drops { get; set; }

        public short FirstX { get; set; }

        public short FirstY { get; set; }

        public DateTime LastEffect { get; private set; }

        public DateTime LastMove { get; private set; }

        public List<NpcMonsterSkill> Skills { get; set; }

        public List<TeleporterDTO> Teleporters { get; set; }

        #endregion

        #region Methods

        public string GenerateEInfo() => $"e_info 10 {(OriginalNpcMonsterVNum > 0 ? OriginalNpcMonsterVNum : NpcMonsterVNum)} {Level} {Element} {AttackClass} {ElementRate} {AttackUpgrade} {DamageMinimum} {DamageMaximum} {Concentrate} {CriticalChance} {CriticalRate} {DefenceUpgrade} {CloseDefence} {DefenceDodge} {DistanceDefence} {DistanceDefenceDodge} {MagicDefence} {FireResistance} {WaterResistance} {LightResistance} {DarkResistance} {MaxHP} {MaxMP} -1 {Name.Replace(' ', '^')}";

        public float GetRes(int skillelement)
        {
            switch (skillelement)
            {
                case 0:
                    return FireResistance / 100;

                case 1:
                    return WaterResistance / 100;

                case 2:
                    return LightResistance / 100;

                case 3:
                    return DarkResistance / 100;

                default:
                    return 0f;
            }
        }

        /// <summary>
        /// Intializes the GameObject, will be injected by AutoMapper after Entity -&gt; GO mapping
        /// </summary>
        public void Initialize()
        {
            Teleporters = ServerManager.Instance.GetTeleportersByNpcVNum(NpcMonsterVNum);
            Drops = ServerManager.Instance.GetDropsByMonsterVNum(NpcMonsterVNum);
            LastEffect = LastMove = DateTime.Now;
            Skills = ServerManager.Instance.GetNpcMonsterSkillsByMonsterVNum(OriginalNpcMonsterVNum > 0 ? OriginalNpcMonsterVNum : NpcMonsterVNum);
            if (Skills.Count == 0 && OriginalNpcMonsterVNum > 0)
            {
                Skills = ServerManager.Instance.GetNpcMonsterSkillsByMonsterVNum(NpcMonsterVNum);
            }
        }

        #endregion
    }
}