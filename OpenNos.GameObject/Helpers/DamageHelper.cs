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
using OpenNos.GameObject.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using NosByte.Shared;
using OpenNos.Core;
using OpenNos.GameObject.Extension.CharacterExtensions;
using static OpenNos.Domain.BCardType;

namespace OpenNos.GameObject.Helpers
{
    public class DamageHelper
    {
        #region Members

        private static DamageHelper _instance;

        #endregion

        #region Properties

        public static DamageHelper Instance => _instance ?? (_instance = new DamageHelper());

        #endregion

        #region Methods

        /// <summary>
        /// Calculates the damage attacker inflicts defender
        /// </summary>
        /// <param name="attacker">The attacking Entity</param>
        /// <param name="defender">The defending Entity</param>
        /// <param name="skill">The used Skill</param>
        /// <param name="hitMode">reference to HitMode</param>
        /// <param name="onyxWings"></param>
        /// <returns>Damage</returns>
        public int CalculateDamage(BattleEntity attacker, BattleEntity defender, Skill skill, ref int hitMode,
            ref bool onyxWings, ref bool zephyrWings, bool attackGreaterDistance = false)
        {
            if (defender != null && defender.IsEggPet)
            {
                hitMode = 2;
                return 0;
            }

            if (!attacker.CanAttackEntity(defender))
            {
                hitMode = 2;
                return 0;
            }

            if (attacker.Character?.Timespace != null && attacker.Character.Timespace.SpNeeded?[(byte)attacker.Character.Class] != 0)
            {
                ItemInstance specialist = attacker.Character.Inventory?.LoadBySlotAndType((byte)EquipmentType.Sp, InventoryType.Wear);
                if (specialist == null || specialist.ItemVNum != attacker.Character?.Timespace.SpNeeded?[(byte)attacker.Character.Class])
                {
                    hitMode = 2;
                    return 0;
                }
            }

            if (skill != null && SkillHelper.IsSelfAttack(skill.SkillVNum))
            {
                hitMode = 0;
                return 0;
            }

            if (skill != null && SkillHelper.Instance.IsNoDamage(skill.SkillVNum))
            {
                hitMode = 0;
                return 0;
            }

            int maxRange = skill != null ? skill.Range > 0 ? skill.Range : skill.TargetRange : attacker.Mate != null ? attacker.Mate.Monster.BasicRange > 0 ? attacker.Mate.Monster.BasicRange : 10 : attacker.MapMonster != null ? attacker.MapMonster.Monster.BasicRange > 0 ? attacker.MapMonster.Monster.BasicRange : 10 : attacker.MapNpc != null ? attacker.MapNpc.Npc.BasicRange > 0 ? attacker.MapNpc.Npc.BasicRange : 10 : 0;

            if (skill != null && skill.HitType == 1 && skill.TargetType == 1 && skill.TargetRange > 0)
            {
                maxRange = skill.TargetRange;
            }

            if (skill != null && skill.HitType == 2 && skill.TargetType == 1 && skill.TargetRange > 0)
            {
                maxRange = skill.TargetRange;
            }

            if (skill != null && (skill.CastEffect == 4657 || skill.CastEffect == 4940))
            {
                maxRange = 3;
            }

            if (attacker.EntityType == defender.EntityType && attacker.MapEntityId == defender.MapEntityId || attacker.Character == null && attacker.Mate == null &&
                 Map.GetDistance(new MapCell { X = attacker.PositionX, Y = attacker.PositionY }, new MapCell { X = defender.PositionX, Y = defender.PositionY }) > maxRange)
                if (skill == null || skill.TargetRange != 0 || skill.Range != 0 && !attackGreaterDistance)
                {
                    hitMode = 2;
                    return 0;
                }

            if (skill != null && skill.BCards.Any(s => s.Type == (byte)CardType.DrainAndSteal && s.SubType == (byte)AdditionalTypes.DrainAndSteal.ConvertEnemyHPToMP))
            {
                return 0;
            }

            if (attacker.Character != null
                && ((attacker.Character.UseSp
                && attacker.Character.Inventory.LoadBySlotAndType((byte)EquipmentType.Sp, InventoryType.Wear) is ItemInstance attackerSp
                && skill?.Element == 0
                && attackerSp.ItemVNum != 900
                && attackerSp.ItemVNum != 907
                && attackerSp.ItemVNum != 908
                && attackerSp.ItemVNum != 4099
                && attackerSp.ItemVNum != 4100
                && ((skill == null || defender.MapMonster?.Monster.Race != 5 || !skill.BCards.Any(s => s.Type == (byte)CardType.LightAndShadow && s.SubType == (byte)AdditionalTypes.LightAndShadow.InflictDamageOnUndead))
                && skill?.SkillVNum != 1065 && skill?.SkillVNum != 1248)
                || (defender.MapMonster?.Owner?.MapEntityId == attacker.MapEntityId && !defender.IsMateTrainer(defender.MapMonster.MonsterVNum))))
                || skill?.SkillVNum >= 235 && skill?.SkillVNum <= 237 || skill?.SkillVNum == 274 || skill?.SkillVNum == 276 || skill?.SkillVNum == 892 || skill?.SkillVNum == 916
                || skill?.SkillVNum == 1129 || skill?.SkillVNum == 1133 || skill?.SkillVNum == 1137 || skill?.SkillVNum == 1138 || skill?.SkillVNum == 1329
                || attacker.MapMonster?.MonsterVNum == 1438 || attacker.MapMonster?.MonsterVNum == 1439)
            {
                hitMode = 0;
                return 0;
            }

            if (defender.Character != null && defender.Character.HasGodMode)
            {
                hitMode = 0;
                return 0;
            }

            if (defender.MapMonster != null && skill?.SkillVNum != 888 && MonsterHelper.IsNamaju(defender.MapMonster.MonsterVNum))
            {
                hitMode = 0;
                return 0;
            }

            if (attacker.MapMonster?.MonsterVNum == 1436)
            {
                hitMode = 0;
                return 0;
            }

            if (attacker.HasBuff(CardType.NoDefeatAndNoDamage, (byte)AdditionalTypes.NoDefeatAndNoDamage.NeverCauseDamage)
             || defender.HasBuff(CardType.NoDefeatAndNoDamage, (byte)AdditionalTypes.NoDefeatAndNoDamage.NeverReceiveDamage))
            {
                hitMode = 0;
                return 0;
            }

            int totalDamage = 0;
            bool percentDamage = false;

            BattleEntity realAttacker = attacker;

            if (attacker.MapMonster?.Owner?.Character != null && !attacker.IsMateTrainer(attacker.MapMonster.MonsterVNum))
            {
                if (attacker.DamageMinimum == 0 || MonsterHelper.UseOwnerEntity(attacker.MapMonster.MonsterVNum))
                {
                    attacker = new BattleEntity(attacker.MapMonster.Owner.Character, skill);
                }
            }

            List<BCard> attackerBCards = attacker.BCards.ToList();
            List<BCard> defenderBCards = defender.BCards.ToList();

            if (attacker.Character != null)
            {
                List<CharacterSkill> skills = attacker.Character.GetSkills();

                //Upgrade Skills
                if (skill != null && skills.FirstOrDefault(s => s.SkillVNum == skill.SkillVNum) is CharacterSkill charSkill)
                {
                    attackerBCards.AddRange(charSkill.GetSkillBCards());
                }
                else // Passive Skills are getted on GetSkillBCards()
                {
                    if (skill?.BCards != null)
                    {
                        attackerBCards.AddRange(skill.BCards);
                    }

                    //Passive Skills
                    attackerBCards.AddRange(PassiveSkillHelper.Instance.PassiveSkillToBCards(attacker.Character.Skills?.Where(s => s.Skill.SkillType == 0)));
                }
                attackerBCards.AddRange(attacker.Character.EffectFromTitle.ToList());
            }
            else
            {
                if (skill?.BCards != null)
                {
                    attackerBCards.AddRange(skill.BCards);
                }
            }

            int[] GetAttackerBenefitingBuffs(CardType type, byte subtype, bool castTypeNotZero = false)
            {
                int value1 = 0;
                int value2 = 0;
                int value3 = 0;
                int temp = 0;

                int[] tmp = GetBuff(attacker.Level, attacker.Buffs.GetAllItems(), attackerBCards, type, subtype, BuffType.Good,
                    ref temp, castTypeNotZero);
                value1 += tmp[0];
                value2 += tmp[1];
                value3 += tmp[2];
                tmp = GetBuff(attacker.Level, attacker.Buffs.GetAllItems(), attackerBCards, type, subtype, BuffType.Neutral,
                    ref temp, castTypeNotZero);
                value1 += tmp[0];
                value2 += tmp[1];
                value3 += tmp[2];
                tmp = GetBuff(defender.Level, defender.Buffs.GetAllItems(), defenderBCards, type, subtype, BuffType.Bad, ref temp, castTypeNotZero);
                value1 += tmp[0];
                value2 += tmp[1];
                value3 += tmp[2];

                if (value1 < 0) value1 *= -1;
                if (value2 < 0) value2 *= -1;
                if (value3 < 0) value3 *= -1;
                if (temp < 0) temp *= -1;

                return new[] { value1, value2, value3, temp };
            }

            int[] GetDefenderBenefitingBuffs(CardType type, byte subtype)
            {
                int value1 = 0;
                int value2 = 0;
                int value3 = 0;
                int temp = 0;

                int[] tmp = GetBuff(defender.Level, defender.Buffs.GetAllItems(), defenderBCards, type, subtype, BuffType.Good,
                    ref temp);
                value1 += tmp[0];
                value2 += tmp[1];
                value3 += tmp[2];
                tmp = GetBuff(defender.Level, defender.Buffs.GetAllItems(), defenderBCards, type, subtype, BuffType.Neutral,
                    ref temp);
                value1 += tmp[0];
                value2 += tmp[1];
                value3 += tmp[2];
                tmp = GetBuff(attacker.Level, attacker.Buffs.GetAllItems(), attackerBCards, type, subtype, BuffType.Bad, ref temp);
                value1 += tmp[0];
                value2 += tmp[1];
                value3 += tmp[2];

                if (value1 < 0) value1 *= -1;
                if (value2 < 0) value2 *= -1;
                if (value3 < 0) value3 *= -1;
                if (temp < 0) temp *= -1;

                return new[] { value1, value2, value3, temp };
            }

            int GetShellWeaponEffectValue(ShellWeaponEffectType effectType)
            {
                return attacker.ShellWeaponEffects?.Where(s => s.Effect == (byte)effectType).FirstOrDefault()?.Value ?? 0;
            }

            int GetShellArmorEffectValue(ShellArmorEffectType effectType)
            {
                return defender.ShellArmorEffects?.Where(s => s.Effect == (byte)effectType).FirstOrDefault()?.Value ?? 0;
            }

            #region Basic Buff Initialisation

            attacker.Morale += GetAttackerBenefitingBuffs(CardType.Morale, (byte)AdditionalTypes.Morale.MoraleIncreased)[0];
            attacker.Morale -= GetDefenderBenefitingBuffs(CardType.Morale, (byte)AdditionalTypes.Morale.MoraleDecreased)[0] * -1;
            defender.Morale += GetDefenderBenefitingBuffs(CardType.Morale, (byte)AdditionalTypes.Morale.MoraleIncreased)[0];
            defender.Morale -= GetAttackerBenefitingBuffs(CardType.Morale, (byte)AdditionalTypes.Morale.MoraleDecreased)[0] * -1;

            attacker.AttackUpgrade += (short)GetAttackerBenefitingBuffs(CardType.AttackPower, (byte)AdditionalTypes.AttackPower.AttackLevelIncreased)[0];
            attacker.AttackUpgrade -= (short)GetDefenderBenefitingBuffs(CardType.AttackPower, (byte)AdditionalTypes.AttackPower.AttackLevelDecreased)[0]; //BIZARRE
            defender.DefenseUpgrade += (short)GetDefenderBenefitingBuffs(CardType.Defence, (byte)AdditionalTypes.Defence.DefenceLevelIncreased)[0];
            defender.DefenseUpgrade -= (short)GetAttackerBenefitingBuffs(CardType.Defence, (byte)AdditionalTypes.Defence.DefenceLevelDecreased)[0]; //BIZARRE

            if (attacker.AttackUpgrade > 10)
            {
                attacker.AttackUpgrade = 10;
            }
            if (defender.DefenseUpgrade > 10)
            {
                defender.DefenseUpgrade = 10;
            }

            int[] attackerpercentdamage = GetAttackerBenefitingBuffs(CardType.RecoveryAndDamagePercent, (byte)AdditionalTypes.RecoveryAndDamagePercent.HPRecovered, true);
            int[] attackerpercentdamage2 = GetAttackerBenefitingBuffs(CardType.RecoveryAndDamagePercent, (byte)AdditionalTypes.RecoveryAndDamagePercent.DecreaseEnemyHP);
            int[] defenderpercentdefense = GetDefenderBenefitingBuffs(CardType.RecoveryAndDamagePercent, (byte)AdditionalTypes.RecoveryAndDamagePercent.DecreaseSelfHP);

            if (attackerpercentdamage[3] != 0)
            {
                totalDamage = defender.HpMax / 100 * attackerpercentdamage[2];
                percentDamage = percentDamage = true;
            }

            if (attackerpercentdamage2[0] != 0)
            {
                totalDamage = defender.HpMax / 100 * Math.Abs(attackerpercentdamage2[0]);
                percentDamage = percentDamage = true;
            }

            if (defenderpercentdefense[0] != 0)
            {
                totalDamage = defender.HpMax / 100 * Math.Abs(defenderpercentdefense[0]);
                percentDamage = percentDamage = true;
            }

            if (defender.MapMonster != null && defender.MapMonster.Monster.TakeDamages > 0)
            {
                if (attacker.Character != null)
                {
                    totalDamage = defender.MapMonster.Monster.TakeDamages;
                }
                else if (attacker.Mate != null)
                {
                    totalDamage = defender.MapMonster.Monster.TakeDamages / 4;
                }
                percentDamage = percentDamage = true;
            }


            if (skill?.SkillVNum == 529 && (defender.Character?.PyjamaDead == true || defender.Mate?.Owner.PyjamaDead == true))
            {
                totalDamage = (int)(defender.HpMax * 0.8D);
                percentDamage = percentDamage = true;
            }

            if (defender.MapMonster != null &&
                MonsterHelper.PercentMonsters.Any(s => s.Equals(defender.MapMonster.MonsterVNum)))
            {
                var fixedDmg = MonsterHelper.GetFixedDamage(defender.MapMonster.MonsterVNum);

                if (defender.MapMonster?.MonsterVNum == 533 && attacker.Character?.SpInstance?.ItemVNum == 900 && attacker.Character.UseSp)
                {
                    fixedDmg = (int)((double)fixedDmg * 1.5D);

                }
                totalDamage = fixedDmg;
                percentDamage = true;
            }

            /*
             *
             * Percentage Boost categories:
             *  1.: Adds to Total Damage
             *  2.: Adds to Normal Damage
             *  3.: Adds to Base Damage
             *  4.: Adds to Defense
             *  5.: Adds to Element
             *
             * Buff Effects get added, whereas
             * Shell Effects get multiplied afterwards.
             *
             * Simplified Example on Defense (Same for Attack):
             *  - 1k Defense
             *  - Costume(+5% Defense)
             *  - Defense Potion(+20% Defense)
             *  - S-Defense Shell with 20% Boost
             *
             * Calculation:
             *  1000 * 1.25 * 1.2 = 1500
             *  Def    Buff   Shell Total
             *
             * Keep in Mind that after each step, one has
             * to round the current value down if necessary
             *
             * Static Boost categories:
             *  1.: Adds to Total Damage
             *  2.: Adds to Normal Damage
             *  3.: Adds to Base Damage
             *  4.: Adds to Defense
             *  5.: Adds to Element
             *
             */

            attacker.Morale -= defender.Morale;

            int hitrate = attacker.Hitrate + attacker.Morale;

            #region Definitions

            double boostCategory1 = 1;
            double boostCategory2 = 1;
            double boostCategory3 = 1;
            double boostCategory4 = 1;
            double boostCategory5 = 1;
            double shellBoostCategory1 = 1;
            double shellBoostCategory2 = 1;
            double shellBoostCategory3 = 1;
            double shellBoostCategory4 = 1;
            double shellBoostCategory5 = 1;
            int staticBoostCategory1 = 0;
            int staticBoostCategory2 = 0;
            int staticBoostCategory3 = 0;
            int staticBoostCategory4 = 0;
            int staticBoostCategory5 = 0;

            #endregion

            #region Type 1

            #region Static

            // None for now

            #endregion

            #region Boost

            shellBoostCategory1 += GetShellWeaponEffectValue(ShellWeaponEffectType.PercentageTotalDamage) / 100D;

            if ((attacker.EntityType == EntityType.Player || attacker.EntityType == EntityType.Mate) && (defender.EntityType == EntityType.Player || defender.EntityType == EntityType.Mate))
            {
                shellBoostCategory1 += GetShellWeaponEffectValue(ShellWeaponEffectType.PercentageDamageInPVP) / 100D;
            }

            if ((attacker?.EntityType == EntityType.Player || attacker?.EntityType == EntityType.Mate) && (defender?.EntityType == EntityType.Monster))
            {
                shellBoostCategory1 += GetShellWeaponEffectValue(ShellWeaponEffectType.DamageincreasedtotheSmallMonster) / 100D; //WIP: Find RaceTypes
                shellBoostCategory1 += GetShellWeaponEffectValue(ShellWeaponEffectType.DamageIncreasedtotheEnemy) / 100D;
                shellBoostCategory1 += GetShellWeaponEffectValue(ShellWeaponEffectType.DamageincreasedtotheBigMonster) / 100D;
            }

            #endregion

            #endregion

            #region Type 2

            #region Static

            if (attacker.Character != null && attacker.Character.Invisible)
            {
                staticBoostCategory2 += GetAttackerBenefitingBuffs(CardType.LightAndShadow, (byte)AdditionalTypes.LightAndShadow.AdditionalDamageWhenHidden)[0];
            }

            #endregion

            #region Boost

            boostCategory2 += GetAttackerBenefitingBuffs(CardType.Damage, (byte)AdditionalTypes.Damage.DamageIncreased)[0] / 100D;
            boostCategory2 -= GetDefenderBenefitingBuffs(CardType.Damage, (byte)AdditionalTypes.Damage.DamageDecreased)[0] / 100D * -1;
            boostCategory2 += GetAttackerBenefitingBuffs(CardType.Item, (byte)AdditionalTypes.Item.AttackIncreased)[0] / 100D;
            boostCategory2 -= GetAttackerBenefitingBuffs(CardType.Item, (byte)AdditionalTypes.Item.AttackDecreased)[0] / 100D * -1;
            boostCategory2 += GetDefenderBenefitingBuffs(CardType.Item, (byte)AdditionalTypes.Item.DefenceIncreased)[0] / 100D;
            boostCategory2 -= GetAttackerBenefitingBuffs(CardType.IncreaseDamageVsChar, (byte)AdditionalTypes.IncreaseDamageVsChar.DecreaseDamagePercent)[0] / 100D * -1;
            boostCategory2 += GetAttackerBenefitingBuffs(CardType.IncreaseDamageVsChar, (byte)AdditionalTypes.IncreaseDamageVsChar.IncreaseDamagePercent)[0] / 100D;

            if ((attacker.EntityType == EntityType.Player || attacker.EntityType == EntityType.Mate) && (defender.EntityType == EntityType.Player || defender.EntityType == EntityType.Mate))
            {
                boostCategory2 += GetAttackerBenefitingBuffs(CardType.SpecialisationBuffResistance, (byte)AdditionalTypes.SpecialisationBuffResistance.IncreaseDamageInPVP)[0] / 100D;
                boostCategory2 -= GetDefenderBenefitingBuffs(CardType.SpecialisationBuffResistance, (byte)AdditionalTypes.SpecialisationBuffResistance.DecreaseDamageInPVP)[0] / 100D * -1;
                boostCategory2 += GetAttackerBenefitingBuffs(CardType.LeonaPassiveSkill, (byte)AdditionalTypes.LeonaPassiveSkill.AttackIncreasedInPVP)[0] / 100D;
                boostCategory2 -= GetDefenderBenefitingBuffs(CardType.LeonaPassiveSkill, (byte)AdditionalTypes.LeonaPassiveSkill.AttackDecreasedInPVP)[0] / 100D * -1;
            }

            if (defender.MapMonster != null)
            {
                if (GetAttackerBenefitingBuffs(CardType.LeonaPassiveSkill, (byte)AdditionalTypes.LeonaPassiveSkill.IncreaseDamageAgainst) is int[] IncreaseDamageAgainst && IncreaseDamageAgainst[1] > 0 && defender.MapMonster.Monster.RaceType == IncreaseDamageAgainst[0])
                {
                    boostCategory2 += IncreaseDamageAgainst[1] / 100D;
                }
            }

            #endregion

            #endregion

            #region Type 3

            #region Static

            staticBoostCategory3 += GetAttackerBenefitingBuffs(CardType.AttackPower,
                (byte)AdditionalTypes.AttackPower.AllAttacksIncreased)[0];
            staticBoostCategory3 -= GetDefenderBenefitingBuffs(CardType.AttackPower,
                (byte)AdditionalTypes.AttackPower.AllAttacksDecreased)[0] * -1;

            #endregion

            #region MA

            if (defender.HasBuff(694))
            {
                defender?.AddBuff(new Buff(703, defender.Level), defender);
                defender?.RemoveBuff(694);
            }

            #endregion

            #region Soft-Damage

            int[] soft = GetAttackerBenefitingBuffs(CardType.IncreaseDamage,
                (byte)AdditionalTypes.IncreaseDamage.IncreasingPropability);

            if (attacker == realAttacker && ServerManager.RandomNumber() < soft[0])
            {
                boostCategory3 += attacker?.Character?.Session?.CurrentMapInstance?.IsPVP == true ? soft[1] / 200D * 1.4 : soft[1] / 100D;
                attacker.MapInstance?.Broadcast(StaticPacketHelper.GenerateEff(realAttacker.UserType, realAttacker.MapEntityId, 15));
            }

            #endregion

            #region skinSoft

            int[] skin = GetAttackerBenefitingBuffs(CardType.EffectSummon,
                (byte)AdditionalTypes.EffectSummon.DamageBoostOnHigherLvl);

            if (attacker == realAttacker && ServerManager.RandomNumber() < skin[0] && defender.EntityType == EntityType.Monster)
            {
                boostCategory3 += skin[1] / 100D;
                attacker.MapInstance?.Broadcast(StaticPacketHelper.GenerateEff(realAttacker.UserType, realAttacker.MapEntityId, 21));
            }

            #endregion

            #region leech

            int[] leechHpFromEnemy = attacker.GetBuff(CardType.AbsorptionAndPowerSkill, (byte)AdditionalTypes.AbsorptionAndPowerSkill.LeechHpFromEnemy);

            if (!defender.HasBuff(72) || hitMode != 4) // Doesn't need it's own BCard for now.
            {
                if (attacker.Hp < attacker.HpMax && defender.Hp > leechHpFromEnemy[1])
                {
                    if (ServerManager.RandomNumber() < leechHpFromEnemy[0])
                    {
                        attacker?.Character?.Session?.CurrentMapInstance?.Broadcast(attacker?.GenerateRc(leechHpFromEnemy[1]));
                        defender?.Character?.Session?.CurrentMapInstance?.Broadcast(defender?.GenerateDm(leechHpFromEnemy[1]));
                        attacker?.MapMonster?.MapInstance?.Broadcast(attacker?.GenerateRc(leechHpFromEnemy[1]));
                        defender?.MapMonster?.MapInstance?.Broadcast(defender?.GenerateDm(leechHpFromEnemy[1]));
                        int leechAmount = leechHpFromEnemy[1];
                        attacker.Hp += leechAmount;
                        defender.Hp -= leechAmount;
                    }
                }
            }


            #endregion

            #region Type 4

            #region Static

            //ItemInstance amulet = attacker.Character.Inventory.LoadBySlotAndType((byte)EquipmentType.MainWeapon, InventoryType.Wear);

            //staticBoostCategory4 += (int)Math.Round((double)(amulet.SlElement * GetDefenderBenefitingBuffs(CardType.AttackPower, (byte)AdditionalTypes.Defence.AllIncreased)[0]) / 100);

            staticBoostCategory4 += GetDefenderBenefitingBuffs(CardType.Defence, (byte)AdditionalTypes.Defence.AllIncreased)[0];
            staticBoostCategory4 -= GetAttackerBenefitingBuffs(CardType.Defence, (byte)AdditionalTypes.Defence.AllDecreased)[0] * -1;

            int temp2 = 0;
            staticBoostCategory4 -= GetBuff(defender.Level, defender.Buffs.GetAllItems(), defenderBCards, CardType.Defence, (byte)AdditionalTypes.Defence.AllDecreased, BuffType.Good, ref temp2)[0];

            #endregion

            #region Boost

            boostCategory4 += GetDefenderBenefitingBuffs(CardType.DodgeAndDefencePercent, (byte)AdditionalTypes.DodgeAndDefencePercent.DefenceIncreased)[0] / 100D;
            boostCategory4 -= GetAttackerBenefitingBuffs(CardType.DodgeAndDefencePercent, (byte)AdditionalTypes.DodgeAndDefencePercent.DefenceReduced)[0] / 100D * -1;
            shellBoostCategory4 += GetShellArmorEffectValue(ShellArmorEffectType.PercentageTotalDefence) / 100D;

            if ((attacker.EntityType == EntityType.Player || attacker.EntityType == EntityType.Mate)
                && (defender.EntityType == EntityType.Player || defender.EntityType == EntityType.Mate))
            {
                boostCategory4 += GetDefenderBenefitingBuffs(CardType.LeonaPassiveSkill, (byte)AdditionalTypes.LeonaPassiveSkill.DefenceIncreasedInPVP)[0] / 100D;
                boostCategory4 -= GetAttackerBenefitingBuffs(CardType.LeonaPassiveSkill, (byte)AdditionalTypes.LeonaPassiveSkill.DefenceDecreasedInPVP)[0] / 100D * -1;
                shellBoostCategory4 -= GetShellWeaponEffectValue(ShellWeaponEffectType.ReducesPercentageEnemyDefenceInPVP) * 2 / 100D;
                shellBoostCategory4 += GetShellArmorEffectValue(ShellArmorEffectType.PercentageAllPVPDefence) / 100D;
            }

            int[] chanceAllIncreased = GetAttackerBenefitingBuffs(CardType.Block, (byte)AdditionalTypes.Block.ChanceAllIncreased);
            int[] chanceAllDecreased = GetDefenderBenefitingBuffs(CardType.Block, (byte)AdditionalTypes.Block.ChanceAllDecreased);

            if (ServerManager.RandomNumber() < chanceAllIncreased[0])
            {
                boostCategory1 += chanceAllIncreased[1] / 100D;
            }

            if (ServerManager.RandomNumber() < -chanceAllDecreased[0])
            {
                boostCategory1 -= chanceAllDecreased[1] / 100D * -1;
            }

            #endregion

            #endregion

            #region Type 5

            #region Static

            staticBoostCategory5 +=
                GetAttackerBenefitingBuffs(CardType.Element, (byte)AdditionalTypes.Element.AllIncreased)[0];
            staticBoostCategory5 -=
                GetDefenderBenefitingBuffs(CardType.Element, (byte)AdditionalTypes.Element.AllDecreased)[0] * -1;
            staticBoostCategory5 += GetShellWeaponEffectValue(ShellWeaponEffectType.IncreasedElementalProperties);

            #endregion

            #endregion

            #region All Type Class Dependant

            int[] chanceIncreased = null;
            int[] chanceDecreased = null;

            switch (attacker.AttackType)
            {
                case AttackType.Melee:
                    chanceIncreased = GetAttackerBenefitingBuffs(CardType.Block, (byte)AdditionalTypes.Block.ChanceMeleeIncreased);
                    chanceDecreased = GetDefenderBenefitingBuffs(CardType.Block, (byte)AdditionalTypes.Block.ChanceMeleeDecreased);
                    boostCategory2 += GetAttackerBenefitingBuffs(CardType.Damage, (byte)AdditionalTypes.Damage.MeleeIncreased)[0] / 100D;
                    boostCategory2 -= GetDefenderBenefitingBuffs(CardType.Damage, (byte)AdditionalTypes.Damage.MeleeDecreased)[0] / 100D * -1;
                    staticBoostCategory3 += GetAttackerBenefitingBuffs(CardType.AttackPower, (byte)AdditionalTypes.AttackPower.MeleeAttacksIncreased)[0];
                    staticBoostCategory3 -= GetDefenderBenefitingBuffs(CardType.AttackPower, (byte)AdditionalTypes.AttackPower.MeleeAttacksDecreased)[0] * -1;
                    staticBoostCategory4 += GetShellArmorEffectValue(ShellArmorEffectType.CloseDefence);
                    staticBoostCategory4 += GetDefenderBenefitingBuffs(CardType.Defence, (byte)AdditionalTypes.Defence.MeleeIncreased)[0];
                    staticBoostCategory4 -= GetAttackerBenefitingBuffs(CardType.Defence, (byte)AdditionalTypes.Defence.MeleeDecreased)[0] * -1;
                    break;

                case AttackType.Range:
                    chanceIncreased = GetAttackerBenefitingBuffs(CardType.Block, (byte)AdditionalTypes.Block.ChanceRangedIncreased);
                    chanceDecreased = GetDefenderBenefitingBuffs(CardType.Block, (byte)AdditionalTypes.Block.ChanceRangedDecreased);
                    boostCategory2 += GetAttackerBenefitingBuffs(CardType.Damage, (byte)AdditionalTypes.Damage.RangedIncreased)[0] / 100D;
                    boostCategory2 -= GetDefenderBenefitingBuffs(CardType.Damage, (byte)AdditionalTypes.Damage.RangedDecreased)[0] / 100D * -1;
                    staticBoostCategory3 += GetAttackerBenefitingBuffs(CardType.AttackPower, (byte)AdditionalTypes.AttackPower.MeleeAttacksIncreased)[0];
                    staticBoostCategory3 -= GetDefenderBenefitingBuffs(CardType.AttackPower, (byte)AdditionalTypes.AttackPower.MeleeAttacksDecreased)[0] * -1;
                    staticBoostCategory4 += GetShellArmorEffectValue(ShellArmorEffectType.DistanceDefence);
                    staticBoostCategory4 += GetDefenderBenefitingBuffs(CardType.Defence, (byte)AdditionalTypes.Defence.RangedIncreased)[0];
                    staticBoostCategory4 -= GetAttackerBenefitingBuffs(CardType.Defence, (byte)AdditionalTypes.Defence.RangedDecreased)[0] * -1;
                    break;

                case AttackType.Magical:
                    chanceIncreased = GetAttackerBenefitingBuffs(CardType.Block, (byte)AdditionalTypes.Block.ChanceMagicalIncreased);
                    chanceDecreased = GetDefenderBenefitingBuffs(CardType.Block, (byte)AdditionalTypes.Block.ChanceMagicalDecreased);
                    boostCategory2 += GetAttackerBenefitingBuffs(CardType.Damage, (byte)AdditionalTypes.Damage.MagicalIncreased)[0] / 100D;
                    boostCategory2 -= GetDefenderBenefitingBuffs(CardType.Damage, (byte)AdditionalTypes.Damage.MagicalDecreased)[0] / 100D * -1;
                    boostCategory2 += GetAttackerBenefitingBuffs(CardType.IncreaseDamageVsChar, (byte)AdditionalTypes.IncreaseDamageVsChar.IncreaseMagicalDamage)[0] / 100D;
                    staticBoostCategory3 += GetAttackerBenefitingBuffs(CardType.AttackPower, (byte)AdditionalTypes.AttackPower.MeleeAttacksIncreased)[0];
                    staticBoostCategory3 -= GetDefenderBenefitingBuffs(CardType.AttackPower, (byte)AdditionalTypes.AttackPower.MeleeAttacksDecreased)[0] * -1;
                    staticBoostCategory4 += GetShellArmorEffectValue(ShellArmorEffectType.MagicDefence);
                    staticBoostCategory4 += GetDefenderBenefitingBuffs(CardType.Defence, (byte)AdditionalTypes.Defence.MagicalIncreased)[0];
                    staticBoostCategory4 -= GetAttackerBenefitingBuffs(CardType.Defence, (byte)AdditionalTypes.Defence.MagicalDecreased)[0] * -1;
                    break;
            }

            if (ServerManager.RandomNumber() < chanceIncreased[0])
            {
                boostCategory1 += chanceIncreased[1] / 100D;
            }

            if (ServerManager.RandomNumber() < -chanceDecreased[0])
            {
                boostCategory1 -= chanceDecreased[1] / 100D * -1;
            }

            #endregion

            #region Element Dependant

            switch (realAttacker.Element)
            {
                case 0:
                    if (defender?.Character?.Morph == 27 && defender?.Character?.CanGetNewBuffElement == false)
                    {
                        defender.Character.Session.SendPacket($"mslot 1195 0");
                        defender.Character.CanGetNewBuffElement = true;
                    }
                    break;

                case 1:

                    if (defender?.Character?.Morph == 27 && defender?.Character?.CanGetNewBuffElement == false)
                    {
                        defender.Character.Session.SendPacket($"mslot 1191 0");
                        defender.Character.CanGetNewBuffElement = true;
                    }

                    defender.FireResistance += GetDefenderBenefitingBuffs(CardType.ElementResistance, (byte)AdditionalTypes.ElementResistance.AllIncreased)[0];
                    defender.FireResistance -= GetAttackerBenefitingBuffs(CardType.ElementResistance, (byte)AdditionalTypes.ElementResistance.AllDecreased)[0] * -1;
                    defender.FireResistance += GetDefenderBenefitingBuffs(CardType.ElementResistance, (byte)AdditionalTypes.ElementResistance.FireIncreased)[0];
                    defender.FireResistance -= GetAttackerBenefitingBuffs(CardType.ElementResistance, (byte)AdditionalTypes.ElementResistance.FireDecreased)[0] * -1;
                    defender.FireResistance += GetDefenderBenefitingBuffs(CardType.EnemyElementResistance, (byte)AdditionalTypes.EnemyElementResistance.AllIncreased)[0];
                    defender.FireResistance -= GetAttackerBenefitingBuffs(CardType.EnemyElementResistance, (byte)AdditionalTypes.EnemyElementResistance.AllDecreased)[0] * -1;
                    defender.FireResistance += GetDefenderBenefitingBuffs(CardType.EnemyElementResistance, (byte)AdditionalTypes.EnemyElementResistance.FireIncreased)[0];
                    defender.FireResistance -= GetAttackerBenefitingBuffs(CardType.EnemyElementResistance, (byte)AdditionalTypes.EnemyElementResistance.FireDecreased)[0] * -1;

                    defender.FireResistance += (int)Math.Round(defender.FireResistance * GetDefenderBenefitingBuffs(CardType.EnemyElementResistancePercent, (byte)AdditionalTypes.EnemyElementResistancePercent.AllIncreased)[0] / 100D);
                    defender.FireResistance -= (int)Math.Round(defender.FireResistance * GetDefenderBenefitingBuffs(CardType.EnemyElementResistancePercent, (byte)AdditionalTypes.EnemyElementResistancePercent.AllDecreased)[0] / 100D * -1);
                    defender.FireResistance += (int)Math.Round(defender.FireResistance * GetDefenderBenefitingBuffs(CardType.EnemyElementResistancePercent, (byte)AdditionalTypes.EnemyElementResistancePercent.FireIncreased)[0] / 100D);
                    defender.FireResistance -= (int)Math.Round(defender.FireResistance * GetDefenderBenefitingBuffs(CardType.EnemyElementResistancePercent, (byte)AdditionalTypes.EnemyElementResistancePercent.FireDecreased)[0] / 100D * -1);

                    if (defender.HasBuff(CardType.ChanceChangeResistance, (byte)AdditionalTypes.ChanceChangeResistance.AllIncreased))
                    {
                        int[] chances = GetDefenderBenefitingBuffs(CardType.ChanceChangeResistance, (byte)AdditionalTypes.ChanceChangeResistance.AllIncreased);

                        if (ServerManager.RandomNumber() < chances[0])
                        {
                            defender.FireResistance += GetDefenderBenefitingBuffs(CardType.ChanceChangeResistance, (byte)AdditionalTypes.ChanceChangeResistance.AllIncreased)[1];
                        }
                    }

                    if (defender.HasBuff(CardType.ChanceChangeResistance, (byte)AdditionalTypes.ChanceChangeResistance.AllDecreased))
                    {
                        int[] chances = GetAttackerBenefitingBuffs(CardType.ChanceChangeResistance, (byte)AdditionalTypes.ChanceChangeResistance.AllDecreased);

                        if (ServerManager.RandomNumber() < chances[0])
                        {
                            defender.FireResistance -= GetAttackerBenefitingBuffs(CardType.ChanceChangeResistance, (byte)AdditionalTypes.ChanceChangeResistance.AllDecreased)[1] * -1;
                        }
                    }

                    if (defender.HasBuff(CardType.ChanceChangeResistance, (byte)AdditionalTypes.ChanceChangeResistance.FireIncreased))
                    {
                        int[] chances = GetDefenderBenefitingBuffs(CardType.ChanceChangeResistance, (byte)AdditionalTypes.ChanceChangeResistance.FireIncreased);

                        if (ServerManager.RandomNumber() < chances[0])
                        {
                            defender.FireResistance += GetDefenderBenefitingBuffs(CardType.ChanceChangeResistance, (byte)AdditionalTypes.ChanceChangeResistance.FireIncreased)[1];
                        }
                    }

                    if (defender.HasBuff(CardType.ChanceChangeResistance, (byte)AdditionalTypes.ChanceChangeResistance.FireDecreased))
                    {
                        int[] chances = GetAttackerBenefitingBuffs(CardType.ChanceChangeResistance, (byte)AdditionalTypes.ChanceChangeResistance.FireDecreased);

                        if (ServerManager.RandomNumber() < chances[0])
                        {
                            defender.FireResistance -= GetAttackerBenefitingBuffs(CardType.ChanceChangeResistance, (byte)AdditionalTypes.ChanceChangeResistance.FireDecreased)[1] * -1;
                        }
                    }

                    if ((attacker.EntityType == EntityType.Player || attacker.EntityType == EntityType.Mate) && (defender.EntityType == EntityType.Player || defender.EntityType == EntityType.Mate))
                    {
                        defender.FireResistance -= GetShellWeaponEffectValue(ShellWeaponEffectType.ReducesEnemyFireResistanceInPVP);
                        defender.FireResistance -= GetShellWeaponEffectValue(ShellWeaponEffectType.ReducesEnemyAllResistancesInPVP);
                    }

                    defender.FireResistance += GetShellArmorEffectValue(ShellArmorEffectType.IncreasedFireResistence);
                    defender.FireResistance += GetShellArmorEffectValue(ShellArmorEffectType.IncreasedAllResistence);
                    staticBoostCategory5 += GetShellWeaponEffectValue(ShellWeaponEffectType.IncreasedFireProperties);
                    boostCategory5 += GetAttackerBenefitingBuffs(CardType.IncreaseDamage, (byte)AdditionalTypes.IncreaseDamage.FireIncreased)[0] / 100D;
                    staticBoostCategory5 += GetAttackerBenefitingBuffs(CardType.Element, (byte)AdditionalTypes.Element.FireIncreased)[0];
                    staticBoostCategory5 -= GetDefenderBenefitingBuffs(CardType.Element, (byte)AdditionalTypes.Element.FireDecreased)[0] * -1;

                    if (defender.HasBuff(CardType.IncreaseRes, (byte)AdditionalTypes.IncreaseRes.Increase300Res))
                    {
                        var data = attacker.GetBuff(CardType.IncreaseRes, (byte)AdditionalTypes.IncreaseRes.Increase300Res)[0];
                        if (data < ServerManager.RandomNumber())
                        {
                            defender.FireResistance += attacker.GetBuff(CardType.IncreaseRes, (byte)AdditionalTypes.IncreaseRes.Increase300Res)[1];
                        }
                    }
                    break;

                case 2:

                    if (defender?.Character?.Morph == 27 && defender?.Character?.CanGetNewBuffElement == false)
                    {
                        defender.Character.Session.SendPacket($"mslot 1192 0");
                        defender.Character.CanGetNewBuffElement = true;
                    }

                    defender.WaterResistance += GetDefenderBenefitingBuffs(CardType.ElementResistance, (byte)AdditionalTypes.ElementResistance.AllIncreased)[0];
                    defender.WaterResistance -= GetAttackerBenefitingBuffs(CardType.ElementResistance, (byte)AdditionalTypes.ElementResistance.AllDecreased)[0] * -1;
                    defender.WaterResistance += GetDefenderBenefitingBuffs(CardType.ElementResistance, (byte)AdditionalTypes.ElementResistance.WaterIncreased)[0];
                    defender.WaterResistance -= GetAttackerBenefitingBuffs(CardType.ElementResistance, (byte)AdditionalTypes.ElementResistance.WaterDecreased)[0] * -1;
                    defender.WaterResistance += GetDefenderBenefitingBuffs(CardType.EnemyElementResistance, (byte)AdditionalTypes.EnemyElementResistance.AllIncreased)[0];
                    defender.WaterResistance -= GetAttackerBenefitingBuffs(CardType.EnemyElementResistance, (byte)AdditionalTypes.EnemyElementResistance.AllDecreased)[0] * -1;
                    defender.WaterResistance += GetDefenderBenefitingBuffs(CardType.EnemyElementResistance, (byte)AdditionalTypes.EnemyElementResistance.WaterIncreased)[0];
                    defender.WaterResistance -= GetAttackerBenefitingBuffs(CardType.EnemyElementResistance, (byte)AdditionalTypes.EnemyElementResistance.WaterDecreased)[0] * -1;

                    defender.WaterResistance += (int)Math.Round(defender.WaterResistance *
                        GetDefenderBenefitingBuffs(CardType.EnemyElementResistancePercent, (byte)AdditionalTypes.EnemyElementResistancePercent.AllIncreased)[0] / 100D);
                    defender.WaterResistance -= (int)Math.Round(defender.WaterResistance *
                        GetAttackerBenefitingBuffs(CardType.EnemyElementResistancePercent, (byte)AdditionalTypes.EnemyElementResistancePercent.AllDecreased)[0] / 100D * -1);
                    defender.WaterResistance += (int)Math.Round(defender.WaterResistance *
                        GetDefenderBenefitingBuffs(CardType.EnemyElementResistancePercent, (byte)AdditionalTypes.EnemyElementResistancePercent.WaterIncreased)[0] / 100D);
                    defender.WaterResistance -= (int)Math.Round(defender.WaterResistance *
                        GetAttackerBenefitingBuffs(CardType.EnemyElementResistancePercent, (byte)AdditionalTypes.EnemyElementResistancePercent.WaterDecreased)[0] / 100D * -1);

                    if (defender.HasBuff(CardType.ChanceChangeResistance, (byte)AdditionalTypes.ChanceChangeResistance.AllIncreased))
                    {
                        int[] chances = GetDefenderBenefitingBuffs(CardType.ChanceChangeResistance, (byte)AdditionalTypes.ChanceChangeResistance.AllIncreased);

                        if (ServerManager.RandomNumber() < chances[0])
                        {
                            defender.WaterResistance += GetDefenderBenefitingBuffs(CardType.ChanceChangeResistance, (byte)AdditionalTypes.ChanceChangeResistance.AllIncreased)[1];
                        }
                    }

                    if (defender.HasBuff(CardType.ChanceChangeResistance, (byte)AdditionalTypes.ChanceChangeResistance.AllDecreased))
                    {
                        int[] chances = GetAttackerBenefitingBuffs(CardType.ChanceChangeResistance, (byte)AdditionalTypes.ChanceChangeResistance.AllDecreased);

                        if (ServerManager.RandomNumber() < chances[0])
                        {
                            defender.WaterResistance -= GetAttackerBenefitingBuffs(CardType.ChanceChangeResistance, (byte)AdditionalTypes.ChanceChangeResistance.AllDecreased)[1] * -1;
                        }
                    }

                    if (defender.HasBuff(CardType.ChanceChangeResistance, (byte)AdditionalTypes.ChanceChangeResistance.WaterIncreased))
                    {
                        int[] chances = GetDefenderBenefitingBuffs(CardType.ChanceChangeResistance, (byte)AdditionalTypes.ChanceChangeResistance.WaterIncreased);

                        if (ServerManager.RandomNumber() < chances[0])
                        {
                            defender.WaterResistance += GetDefenderBenefitingBuffs(CardType.ChanceChangeResistance, (byte)AdditionalTypes.ChanceChangeResistance.WaterIncreased)[1];
                        }
                    }

                    if (defender.HasBuff(CardType.ChanceChangeResistance, (byte)AdditionalTypes.ChanceChangeResistance.WaterDecreased))
                    {
                        int[] chances = GetAttackerBenefitingBuffs(CardType.ChanceChangeResistance, (byte)AdditionalTypes.ChanceChangeResistance.WaterDecreased);

                        if (ServerManager.RandomNumber() < chances[0])
                        {
                            defender.WaterResistance -= GetAttackerBenefitingBuffs(CardType.ChanceChangeResistance, (byte)AdditionalTypes.ChanceChangeResistance.WaterDecreased)[1] * -1;
                        }
                    }

                    if ((attacker.EntityType == EntityType.Player || attacker.EntityType == EntityType.Mate) && (defender.EntityType == EntityType.Player || defender.EntityType == EntityType.Mate))
                    {
                        defender.WaterResistance -= GetShellWeaponEffectValue(ShellWeaponEffectType.ReducesEnemyWaterResistanceInPVP);
                        defender.WaterResistance -= GetShellWeaponEffectValue(ShellWeaponEffectType.ReducesEnemyAllResistancesInPVP);
                    }

                    defender.WaterResistance += GetShellArmorEffectValue(ShellArmorEffectType.IncreasedWaterResistence);
                    defender.WaterResistance += GetShellArmorEffectValue(ShellArmorEffectType.IncreasedAllResistence);
                    staticBoostCategory5 += GetShellWeaponEffectValue(ShellWeaponEffectType.IncreasedWaterProperties);
                    boostCategory5 += GetAttackerBenefitingBuffs(CardType.IncreaseDamage,(byte)AdditionalTypes.IncreaseDamage.WaterIncreased)[0] / 100D;
                    staticBoostCategory5 += GetAttackerBenefitingBuffs(CardType.Element, (byte)AdditionalTypes.Element.WaterIncreased)[0];
                    staticBoostCategory5 -= GetDefenderBenefitingBuffs(CardType.Element, (byte)AdditionalTypes.Element.WaterDecreased)[0] * -1;

                    if (defender.HasBuff(CardType.IncreaseRes, (byte)AdditionalTypes.IncreaseRes.Increase300Res))
                    {
                        var data = attacker.GetBuff(CardType.IncreaseRes, (byte)AdditionalTypes.IncreaseRes.Increase300Res)[0];
                        if (data < ServerManager.RandomNumber())
                        {
                            defender.WaterResistance += attacker.GetBuff(CardType.IncreaseRes, (byte)AdditionalTypes.IncreaseRes.Increase300Res)[1];
                        }
                    }
                    break;

                case 3:

                    if (defender?.Character?.Morph == 27 && defender?.Character?.CanGetNewBuffElement == false)
                    {
                        defender.Character.Session.SendPacket($"mslot 1193 0");
                        defender.Character.CanGetNewBuffElement = true;
                    }

                    defender.LightResistance += GetDefenderBenefitingBuffs(CardType.ElementResistance, (byte)AdditionalTypes.ElementResistance.AllIncreased)[0];
                    defender.LightResistance -= GetAttackerBenefitingBuffs(CardType.ElementResistance, (byte)AdditionalTypes.ElementResistance.AllDecreased)[0] * -1;
                    defender.LightResistance += GetDefenderBenefitingBuffs(CardType.ElementResistance, (byte)AdditionalTypes.ElementResistance.LightIncreased)[0];
                    defender.LightResistance -= GetAttackerBenefitingBuffs(CardType.ElementResistance, (byte)AdditionalTypes.ElementResistance.LightDecreased)[0] * -1;
                    defender.LightResistance += GetDefenderBenefitingBuffs(CardType.EnemyElementResistance, (byte)AdditionalTypes.EnemyElementResistance.AllIncreased)[0];
                    defender.LightResistance -= GetAttackerBenefitingBuffs(CardType.EnemyElementResistance, (byte)AdditionalTypes.EnemyElementResistance.AllDecreased)[0] * -1;
                    defender.LightResistance += GetDefenderBenefitingBuffs(CardType.EnemyElementResistance, (byte)AdditionalTypes.EnemyElementResistance.LightIncreased)[0];
                    defender.LightResistance -= GetAttackerBenefitingBuffs(CardType.EnemyElementResistance, (byte)AdditionalTypes.EnemyElementResistance.LightDecreased)[0] * -1;

                    defender.LightResistance += GetDefenderBenefitingBuffs(CardType.EnemyElementResistancePercent, (byte)AdditionalTypes.EnemyElementResistancePercent.AllIncreased)[0];
                    defender.LightResistance -= GetAttackerBenefitingBuffs(CardType.EnemyElementResistancePercent, (byte)AdditionalTypes.EnemyElementResistancePercent.AllDecreased)[0] * -1;
                    defender.LightResistance += GetDefenderBenefitingBuffs(CardType.EnemyElementResistancePercent, (byte)AdditionalTypes.EnemyElementResistancePercent.LightIncreased)[0];
                    defender.LightResistance -= GetAttackerBenefitingBuffs(CardType.EnemyElementResistancePercent, (byte)AdditionalTypes.EnemyElementResistancePercent.LightDecreased)[0] * -1;

                    defender.LightResistance += (int)Math.Round(defender.LightResistance *
                        GetDefenderBenefitingBuffs(CardType.EnemyElementResistancePercent, (byte)AdditionalTypes.EnemyElementResistancePercent.AllIncreased)[0] / 100D);
                    defender.LightResistance -= (int)Math.Round(defender.LightResistance *
                        GetDefenderBenefitingBuffs(CardType.EnemyElementResistancePercent, (byte)AdditionalTypes.EnemyElementResistancePercent.AllDecreased)[0] / 100D * -1);
                    defender.LightResistance += (int)Math.Round(defender.LightResistance *
                        GetDefenderBenefitingBuffs(CardType.EnemyElementResistancePercent, (byte)AdditionalTypes.EnemyElementResistancePercent.LightIncreased)[0] / 100D);
                    defender.LightResistance -= (int)Math.Round(defender.LightResistance *
                        GetDefenderBenefitingBuffs(CardType.EnemyElementResistancePercent, (byte)AdditionalTypes.EnemyElementResistancePercent.LightDecreased)[0] / 100D * -1);

                    if (defender.HasBuff(CardType.ChanceChangeResistance, (byte)AdditionalTypes.ChanceChangeResistance.AllIncreased))
                    {
                        int[] chances = GetDefenderBenefitingBuffs(CardType.ChanceChangeResistance, (byte)AdditionalTypes.ChanceChangeResistance.AllIncreased);

                        if (ServerManager.RandomNumber() < chances[0])
                        {
                            defender.LightResistance += GetDefenderBenefitingBuffs(CardType.ChanceChangeResistance, (byte)AdditionalTypes.ChanceChangeResistance.AllIncreased)[1];
                        }
                    }

                    if (defender.HasBuff(CardType.ChanceChangeResistance, (byte)AdditionalTypes.ChanceChangeResistance.AllDecreased))
                    {
                        int[] chances = GetAttackerBenefitingBuffs(CardType.ChanceChangeResistance, (byte)AdditionalTypes.ChanceChangeResistance.AllDecreased);

                        if (ServerManager.RandomNumber() < chances[0])
                        {
                            defender.LightResistance -= GetAttackerBenefitingBuffs(CardType.ChanceChangeResistance, (byte)AdditionalTypes.ChanceChangeResistance.AllDecreased)[1] * -1;
                        }
                    }

                    if (defender.HasBuff(CardType.ChanceChangeResistance, (byte)AdditionalTypes.ChanceChangeResistance.LightIncreased))
                    {
                        int[] chances = GetDefenderBenefitingBuffs(CardType.ChanceChangeResistance, (byte)AdditionalTypes.ChanceChangeResistance.LightIncreased);

                        if (ServerManager.RandomNumber() < chances[0])
                        {
                            defender.LightResistance += GetDefenderBenefitingBuffs(CardType.ChanceChangeResistance, (byte)AdditionalTypes.ChanceChangeResistance.LightIncreased)[1];
                        }
                    }

                    if (defender.HasBuff(CardType.ChanceChangeResistance, (byte)AdditionalTypes.ChanceChangeResistance.LightDecreased))
                    {
                        int[] chances = GetAttackerBenefitingBuffs(CardType.ChanceChangeResistance, (byte)AdditionalTypes.ChanceChangeResistance.LightDecreased);

                        if (ServerManager.RandomNumber() < chances[0])
                        {
                            defender.LightResistance -= GetAttackerBenefitingBuffs(CardType.ChanceChangeResistance, (byte)AdditionalTypes.ChanceChangeResistance.LightDecreased)[1] * -1;
                        }
                    }

                    if ((attacker.EntityType == EntityType.Player || attacker.EntityType == EntityType.Mate) && (defender.EntityType == EntityType.Player || defender.EntityType == EntityType.Mate))
                    {
                        defender.LightResistance -= GetShellWeaponEffectValue(ShellWeaponEffectType.ReducesEnemyLightResistanceInPVP);
                        defender.LightResistance -= GetShellWeaponEffectValue(ShellWeaponEffectType.ReducesEnemyAllResistancesInPVP);
                    }

                    defender.LightResistance += GetShellArmorEffectValue(ShellArmorEffectType.IncreasedLightResistence);
                    defender.LightResistance += GetShellArmorEffectValue(ShellArmorEffectType.IncreasedAllResistence);
                    staticBoostCategory5 += GetShellWeaponEffectValue(ShellWeaponEffectType.IncreasedLightProperties);

                    boostCategory5 += GetAttackerBenefitingBuffs(CardType.IncreaseDamage, (byte)AdditionalTypes.IncreaseDamage.LightIncreased)[0] / 100D;
                    staticBoostCategory5 += GetAttackerBenefitingBuffs(CardType.Element, (byte)AdditionalTypes.Element.LightIncreased)[0];
                    staticBoostCategory5 -= GetDefenderBenefitingBuffs(CardType.Element, (byte)AdditionalTypes.Element.Light5Decreased)[0] * -1;

                    if (defender.HasBuff(CardType.IncreaseRes, (byte)AdditionalTypes.IncreaseRes.Increase300Res))
                    {
                        var data = attacker.GetBuff(CardType.IncreaseRes, (byte)AdditionalTypes.IncreaseRes.Increase300Res)[0];

                        if (data < ServerManager.RandomNumber())
                        {
                            defender.LightResistance += attacker.GetBuff(CardType.IncreaseRes, (byte)AdditionalTypes.IncreaseRes.Increase300Res)[1];
                        }
                    }

                    break;

                case 4:

                    if (defender?.Character?.Morph == 27 && defender?.Character?.CanGetNewBuffElement == false)
                    {
                        defender.Character.Session.SendPacket($"mslot 1194 0");
                        defender.Character.CanGetNewBuffElement = true;
                    }

                    defender.ShadowResistance += GetDefenderBenefitingBuffs(CardType.ElementResistance, (byte)AdditionalTypes.ElementResistance.AllIncreased)[0];
                    defender.ShadowResistance -= GetAttackerBenefitingBuffs(CardType.ElementResistance, (byte)AdditionalTypes.ElementResistance.AllDecreased)[0] * -1;
                    defender.ShadowResistance += GetDefenderBenefitingBuffs(CardType.ElementResistance, (byte)AdditionalTypes.ElementResistance.DarkIncreased)[0];
                    defender.ShadowResistance -= GetAttackerBenefitingBuffs(CardType.ElementResistance, (byte)AdditionalTypes.ElementResistance.DarkDecreased)[0] * -1;
                    defender.ShadowResistance += GetDefenderBenefitingBuffs(CardType.EnemyElementResistance, (byte)AdditionalTypes.EnemyElementResistance.AllIncreased)[0];
                    defender.ShadowResistance -= GetAttackerBenefitingBuffs(CardType.EnemyElementResistance, (byte)AdditionalTypes.EnemyElementResistance.AllDecreased)[0] * -1;
                    defender.ShadowResistance += GetDefenderBenefitingBuffs(CardType.EnemyElementResistance, (byte)AdditionalTypes.EnemyElementResistance.DarkIncreased)[0];
                    defender.ShadowResistance -= GetAttackerBenefitingBuffs(CardType.EnemyElementResistance, (byte)AdditionalTypes.EnemyElementResistance.DarkDecreased)[0] * -1;

                    defender.ShadowResistance += GetDefenderBenefitingBuffs(CardType.EnemyElementResistancePercent, (byte)AdditionalTypes.EnemyElementResistancePercent.AllIncreased)[0];
                    defender.ShadowResistance -= GetAttackerBenefitingBuffs(CardType.EnemyElementResistancePercent, (byte)AdditionalTypes.EnemyElementResistancePercent.AllDecreased)[0] * -1;
                    defender.ShadowResistance += GetDefenderBenefitingBuffs(CardType.EnemyElementResistancePercent, (byte)AdditionalTypes.EnemyElementResistancePercent.DarkIncreased)[0];
                    defender.ShadowResistance -= GetAttackerBenefitingBuffs(CardType.EnemyElementResistancePercent, (byte)AdditionalTypes.EnemyElementResistancePercent.DarkDecreased)[0] * -1;

                    defender.ShadowResistance += (int)Math.Round(defender.ShadowResistance *
                        GetDefenderBenefitingBuffs(CardType.EnemyElementResistancePercent, (byte)AdditionalTypes.EnemyElementResistancePercent.AllIncreased)[0] / 100D);
                    defender.ShadowResistance -= (int)Math.Round(defender.ShadowResistance *
                        GetDefenderBenefitingBuffs(CardType.EnemyElementResistancePercent, (byte)AdditionalTypes.EnemyElementResistancePercent.AllDecreased)[0] / 100D * -1);
                    defender.ShadowResistance += (int)Math.Round(defender.ShadowResistance *
                        GetDefenderBenefitingBuffs(CardType.EnemyElementResistancePercent, (byte)AdditionalTypes.EnemyElementResistancePercent.DarkIncreased)[0] / 100D);
                    defender.ShadowResistance -= (int)Math.Round(defender.ShadowResistance *
                        GetDefenderBenefitingBuffs(CardType.EnemyElementResistancePercent, (byte)AdditionalTypes.EnemyElementResistancePercent.DarkDecreased)[0] / 100D * -1);

                    if (defender.HasBuff(CardType.ChanceChangeResistance, (byte)AdditionalTypes.ChanceChangeResistance.AllIncreased))
                    {
                        int[] chances = GetDefenderBenefitingBuffs(CardType.ChanceChangeResistance, (byte)AdditionalTypes.ChanceChangeResistance.AllIncreased);

                        if (ServerManager.RandomNumber() < chances[0])
                        {
                            defender.ShadowResistance += GetDefenderBenefitingBuffs(CardType.ChanceChangeResistance, (byte)AdditionalTypes.ChanceChangeResistance.AllIncreased)[1];
                        }
                    }

                    if (defender.HasBuff(CardType.ChanceChangeResistance, (byte)AdditionalTypes.ChanceChangeResistance.AllDecreased))
                    {
                        int[] chances = GetAttackerBenefitingBuffs(CardType.ChanceChangeResistance, (byte)AdditionalTypes.ChanceChangeResistance.AllDecreased);

                        if (ServerManager.RandomNumber() < chances[0])
                        {
                            defender.ShadowResistance -= GetAttackerBenefitingBuffs(CardType.ChanceChangeResistance, (byte)AdditionalTypes.ChanceChangeResistance.AllDecreased)[1] * -1;
                        }
                    }

                    if (defender.HasBuff(CardType.ChanceChangeResistance, (byte)AdditionalTypes.ChanceChangeResistance.DarkIncreased))
                    {
                        int[] chances = GetDefenderBenefitingBuffs(CardType.ChanceChangeResistance, (byte)AdditionalTypes.ChanceChangeResistance.DarkIncreased);

                        if (ServerManager.RandomNumber() < chances[0])
                        {
                            defender.ShadowResistance += GetDefenderBenefitingBuffs(CardType.ChanceChangeResistance, (byte)AdditionalTypes.ChanceChangeResistance.DarkIncreased)[1];
                        }
                    }

                    if (defender.HasBuff(CardType.ChanceChangeResistance, (byte)AdditionalTypes.ChanceChangeResistance.DarkDecreased))
                    {
                        int[] chances = GetAttackerBenefitingBuffs(CardType.ChanceChangeResistance, (byte)AdditionalTypes.ChanceChangeResistance.DarkDecreased);

                        if (ServerManager.RandomNumber() < chances[0])
                        {
                            defender.ShadowResistance -= GetAttackerBenefitingBuffs(CardType.ChanceChangeResistance, (byte)AdditionalTypes.ChanceChangeResistance.DarkDecreased)[1] * -1;
                        }
                    }

                    if ((attacker.EntityType == EntityType.Player || attacker.EntityType == EntityType.Mate) && (defender.EntityType == EntityType.Player || defender.EntityType == EntityType.Mate))
                    {
                        defender.ShadowResistance -= GetShellWeaponEffectValue(ShellWeaponEffectType.ReducesEnemyDarkResistanceInPVP);
                        defender.ShadowResistance -= GetShellWeaponEffectValue(ShellWeaponEffectType.ReducesEnemyAllResistancesInPVP);
                    }

                    defender.ShadowResistance += GetShellArmorEffectValue(ShellArmorEffectType.IncreasedDarkResistence);
                    defender.ShadowResistance += GetShellArmorEffectValue(ShellArmorEffectType.IncreasedAllResistence);
                    staticBoostCategory5 += GetShellWeaponEffectValue(ShellWeaponEffectType.IncreasedDarkProperties);

                    boostCategory5 += GetAttackerBenefitingBuffs(CardType.IncreaseDamage, (byte)AdditionalTypes.IncreaseDamage.DarkIncreased)[0] / 100D;

                    int[] darkElementDamageIncreaseChance = GetDefenderBenefitingBuffs(CardType.DarkCloneSummon, (byte)AdditionalTypes.DarkCloneSummon.DarkElementDamageIncreaseChance);

                    if (ServerManager.RandomNumber() < darkElementDamageIncreaseChance[0])
                    {
                        boostCategory5 += darkElementDamageIncreaseChance[1] / 100D;
                    }

                    staticBoostCategory5 += GetAttackerBenefitingBuffs(CardType.Element, (byte)AdditionalTypes.Element.DarkIncreased)[0];
                    staticBoostCategory5 -= GetDefenderBenefitingBuffs(CardType.Element, (byte)AdditionalTypes.Element.DarkDecreased)[0] * -1;

                    if (defender.HasBuff(CardType.IncreaseRes, (byte)AdditionalTypes.IncreaseRes.Increase300Res))
                    {
                        var data = attacker.GetBuff(CardType.IncreaseRes, (byte)AdditionalTypes.IncreaseRes.Increase300Res)[0];
                        if (data < ServerManager.RandomNumber())
                        {
                            defender.ShadowResistance += attacker.GetBuff(CardType.IncreaseRes, (byte)AdditionalTypes.IncreaseRes.Increase300Res)[1];
                        }
                    }
                    break;
            }

            #endregion

            #endregion

            #region Attack Type Related Variables

            switch (attacker.AttackType)
            {
                case AttackType.Melee:
                    defender.Defense = defender.MeleeDefense;
                    defender.ArmorDefense = defender.ArmorMeleeDefense;
                    defender.Dodge = defender.MeleeDefenseDodge;
                    staticBoostCategory3 += GetAttackerBenefitingBuffs(CardType.AttackPower, (byte)AdditionalTypes.AttackPower.MeleeAttacksIncreased)[0];
                    if (GetDefenderBenefitingBuffs(CardType.Target, (byte)AdditionalTypes.Target.MeleeHitRateIncreased)[0] is int MeleeHitRateIncreased)
                    {
                        if (MeleeHitRateIncreased != 0)
                        {
                            hitrate += MeleeHitRateIncreased;
                        }
                    }
                    break;

                case AttackType.Range:
                    defender.Defense = defender.RangeDefense;
                    defender.ArmorDefense = defender.ArmorRangeDefense;
                    defender.Dodge = defender.RangeDefenseDodge;
                    staticBoostCategory3 += GetAttackerBenefitingBuffs(CardType.AttackPower, (byte)AdditionalTypes.AttackPower.RangedAttacksIncreased)[0];
                    if (GetDefenderBenefitingBuffs(CardType.Target, (byte)AdditionalTypes.Target.RangedHitRateIncreased)[0] is int RangedHitRateIncreased)
                    {
                        if (RangedHitRateIncreased != 0)
                        {
                            hitrate += RangedHitRateIncreased;
                        }
                    }
                    break;

                case AttackType.Magical:
                    defender.Defense = defender.MagicalDefense;
                    defender.ArmorDefense = defender.ArmorMagicalDefense;
                    staticBoostCategory3 += GetAttackerBenefitingBuffs(CardType.AttackPower, (byte)AdditionalTypes.AttackPower.MagicalAttacksIncreased)[0];
                    if (GetDefenderBenefitingBuffs(CardType.Target, (byte)AdditionalTypes.Target.MagicalConcentrationIncreased)[0] is int MagicalConcentrationIncreased)
                    {
                        if (MagicalConcentrationIncreased != 0)
                        {
                            hitrate += MagicalConcentrationIncreased;
                        }
                    }
                    break;
            }

            #endregion

            #region Attack Type Attack Disabled

            bool AttackDisabled = false;

            switch (attacker.AttackType)
            {
                case AttackType.Melee:
                    if (attacker.HasBuff(CardType.SpecialAttack, (byte)AdditionalTypes.SpecialAttack.MeleeDisabled))
                    {
                        AttackDisabled = true;
                    }
                    break;

                case AttackType.Range:
                    if (attacker.HasBuff(CardType.SpecialAttack, (byte)AdditionalTypes.SpecialAttack.RangedDisabled))
                    {
                        AttackDisabled = true;
                    }
                    break;

                case AttackType.Magical:
                    if (attacker.HasBuff(CardType.SpecialAttack, (byte)AdditionalTypes.SpecialAttack.MagicDisabled))
                    {
                        AttackDisabled = true;
                    }
                    break;
            }

            if (AttackDisabled)
            {
                hitMode = 2;

                if (skill != null && attacker.Character != null)
                {
                    Observable.Timer(TimeSpan.FromSeconds(1)).SafeSubscribe(o =>
                    {
                        CharacterSkill ski = attacker.Character.GetSkills()?.Find(s => s.Skill?.CastId == skill.CastId && s.Skill?.UpgradeSkill == 0);

                        if (ski?.Skill != null)
                        {
                            ski.LastUse = DateTime.Now.AddMilliseconds(ski.Skill.Cooldown * 100 * -1);
                            attacker.Character.Session?.SendPacket(StaticPacketHelper.SkillReset(skill.CastId));
                            Console.WriteLine($"Player : {attacker.Character.Name} SkillVNum : {ski.Skill.SkillVNum} CastId: {skill.CastId}");
                        }
                    });
                }
                return 0;
            }

            #endregion

            #region Too Near Range Attack Penalty (boostCategory2)

            if (!attacker.HasBuff(CardType.GuarantedDodgeRangedAttack, (byte)AdditionalTypes.GuarantedDodgeRangedAttack.NoPenatly))
            {
                if (attacker.AttackType == AttackType.Range && Map.GetDistance(
                        new MapCell { X = attacker.PositionX, Y = attacker.PositionY },
                        new MapCell { X = defender.PositionX, Y = defender.PositionY }) <= 5)
                {
                    boostCategory2 -= 0.3;
                }
            }

            if (attacker.AttackType == AttackType.Range && attacker.HasBuff(CardType.GuarantedDodgeRangedAttack, (byte)AdditionalTypes.GuarantedDodgeRangedAttack.DistanceDamageIncreasing))
            {
                double distance = Map.GetDistance(
                        new MapCell { X = attacker.PositionX, Y = attacker.PositionY },
                        new MapCell { X = defender.PositionX, Y = defender.PositionY });

                boostCategory2 += distance * 0.015;
            }

            #endregion

            #region Morale and Dodge

            var multiplicator = defender.GetBuff(CardType.IncreaseRes, (byte)AdditionalTypes.IncreaseRes.IncreaseDodge)[0] / 100D;
            defender.Dodge += (int)(GetDefenderBenefitingBuffs(CardType.DodgeAndDefencePercent, (byte)AdditionalTypes.DodgeAndDefencePercent.DodgeIncreased)[0]
                - (GetDefenderBenefitingBuffs(CardType.DodgeAndDefencePercent, (byte)AdditionalTypes.DodgeAndDefencePercent.DodgeDecreased)[0] * -1) * multiplicator);

            double chance = 0;
            if (attacker.AttackType != AttackType.Magical)
            {
                if (GetAttackerBenefitingBuffs(CardType.Target, (byte)AdditionalTypes.Target.AllHitRateIncreased)[0] is int AllHitRateIncreased)
                {
                    if (AllHitRateIncreased != 0)
                    {
                        hitrate += AllHitRateIncreased;
                    }
                }

                if (GetAttackerBenefitingBuffs(CardType.IncreaseRes, (byte)AdditionalTypes.IncreaseRes.IncreaseHitRate)[0] is int AllHitRateIncreased2)
                {
                    if (AllHitRateIncreased2 != 0)
                    {
                        hitrate += AllHitRateIncreased2;
                    }
                }

                double multiplier = defender.Dodge / (hitrate > 1 ? hitrate : 1);

                if (multiplier > 5)
                {
                    multiplier = 5;
                }

                chance = (-0.25 * Math.Pow(multiplier, 3)) - (0.57 * Math.Pow(multiplier, 2)) + (25.3 * multiplier)
                         - 1.41;
                if (chance <= 1)
                {
                    chance = 1;
                }

                if (GetAttackerBenefitingBuffs(CardType.GuarantedDodgeRangedAttack, (byte)AdditionalTypes.GuarantedDodgeRangedAttack.AttackHitChance)[0] is int AttackHitChance)
                {
                    if (AttackHitChance != 0 && chance > 100 - AttackHitChance)
                    {
                        chance = 100 - AttackHitChance;
                    }
                }

                if (GetDefenderBenefitingBuffs(CardType.GuarantedDodgeRangedAttack, (byte)AdditionalTypes.GuarantedDodgeRangedAttack.AttackHitChance)[0] is int AttackHitChanceNegated)
                {
                    if (AttackHitChanceNegated != 0 && chance < 100 - AttackHitChanceNegated)
                    {
                        chance = 100 - AttackHitChanceNegated;
                    }
                }

                if (attacker.HasBuff(CardType.GuarantedDodgeRangedAttack, (byte)AdditionalTypes.GuarantedDodgeRangedAttack.AttackHitChance100))
                {
                    chance = 0;
                }
            }

            int bonus = 0;
            if ((attacker.EntityType == EntityType.Player || attacker.EntityType == EntityType.Mate)
                && (defender.EntityType == EntityType.Player || defender.EntityType == EntityType.Mate))
            {
                switch (attacker.AttackType)
                {
                    case AttackType.Melee:
                        bonus += GetShellArmorEffectValue(ShellArmorEffectType.CloseDefenceDodgeInPVP);
                        break;

                    case AttackType.Range:
                        bonus += GetShellArmorEffectValue(ShellArmorEffectType.DistanceDefenceDodgeInPVP);
                        break;

                    case AttackType.Magical:
                        bonus += GetShellArmorEffectValue(ShellArmorEffectType.IgnoreMagicDamage);
                        break;
                }

                bonus += GetShellArmorEffectValue(ShellArmorEffectType.DodgeAllAttacksInPVP);
            }

            if
                        (
                            !(
                                defender.HasBuff
                                (
                                    BCardType.CardType.SpecialDefence,
                                    (byte)AdditionalTypes.SpecialDefence.NoDefence
                                )
                                ||
                                defender.HasBuff
                                (
                                    BCardType.CardType.SpecialDefence,
                                    (byte)AdditionalTypes.SpecialDefence.AllDefenceNullified
                                )
                                ||
                                (
                                    attacker.AttackType switch
                                    {
                                        AttackType.Melee =>
                                            defender.HasBuff
                                            (
                                                BCardType.CardType.SpecialDefence,
                                                (byte)AdditionalTypes.SpecialDefence.MeleeDefenceNullified
                                            ),
                                        AttackType.Range =>
                                            defender.HasBuff
                                            (
                                                BCardType.CardType.SpecialDefence,
                                                (byte)AdditionalTypes.SpecialDefence.RangedDefenceNullified
                                            ),
                                        AttackType.Magical =>
                                            defender.HasBuff
                                            (
                                                BCardType.CardType.SpecialDefence,
                                                (byte)AdditionalTypes.SpecialDefence.MagicDefenceNullified
                                            ),
                                        _ => false
                                    }
                                )
                            )
                            &&
                            !defender.Invincible && ServerManager.RandomNumber() - bonus < chance
                        )
            {
                if (attacker.Character != null)
                {
                    if (attacker.Character.SkillComboCount > 0 && totalDamage != 0)
                    {
                        attacker.Character.SkillComboCount = 0;
                        attacker.Character.Session.SendPacket("ms_c 1");
                    }
                }

                hitMode = 4;
                return 0;
            }

            #endregion

            #region Base Damage

            int baseDamage = ServerManager.RandomNumber(attacker.DamageMinimum < attacker.DamageMaximum ? attacker.DamageMinimum : attacker.DamageMaximum, attacker.DamageMaximum + 1);
            int weaponDamage =
                ServerManager.RandomNumber(attacker.WeaponDamageMinimum < attacker.WeaponDamageMaximum ? attacker.WeaponDamageMinimum : attacker.WeaponDamageMaximum, attacker.WeaponDamageMaximum + 1);

            // Adventurer Boost
            if (attacker.Character?.Class == ClassType.Adventurer && attacker.Character?.Level <= 20)
            {
                baseDamage *= 300 / 100;
            }

            #region Attack Level Calculation

            int[] atklvlfix = GetDefenderBenefitingBuffs(CardType.CalculatingLevel,
                (byte)AdditionalTypes.CalculatingLevel.CalculatedAttackLevel);
            int[] deflvlfix = GetAttackerBenefitingBuffs(CardType.CalculatingLevel,
                (byte)AdditionalTypes.CalculatingLevel.CalculatedDefenceLevel);

            if (atklvlfix[3] != 0)
            {
                attacker.AttackUpgrade = (short)atklvlfix[0];
            }

            if (deflvlfix[3] != 0)
            {
                attacker.DefenseUpgrade = (short)deflvlfix[0];
            }

            attacker.AttackUpgrade -= defender.DefenseUpgrade;

            if (attacker.AttackUpgrade < -10)
            {
                attacker.AttackUpgrade = -10;
            }
            else if (attacker.AttackUpgrade > ServerManager.Instance.Configuration.MaxUpgrade)
            {
                attacker.AttackUpgrade = ServerManager.Instance.Configuration.MaxUpgrade;
            }

            if (attacker.Mate?.MateType == MateType.Pet)
            {
                switch (attacker.AttackUpgrade)
                {
                    case 0:
                        baseDamage += 0;
                        break;

                    case 1:
                        baseDamage += (int)(baseDamage * 0.1);
                        break;

                    case 2:
                        baseDamage += (int)(baseDamage * 0.15);
                        break;

                    case 3:
                        baseDamage += (int)(baseDamage * 0.22);
                        break;

                    case 4:
                        baseDamage += (int)(baseDamage * 0.32);
                        break;

                    case 5:
                        baseDamage += (int)(baseDamage * 0.43);
                        break;

                    case 6:
                        baseDamage += (int)(baseDamage * 0.54);
                        break;

                    case 7:
                        baseDamage += (int)(baseDamage * 0.65);
                        break;

                    case 8:
                        baseDamage += (int)(baseDamage * 0.9);
                        break;

                    case 9:
                        baseDamage += (int)(baseDamage * 1.2);
                        break;

                    case 10:
                        baseDamage += baseDamage * 2;
                        break;

                        //default:
                        //    if (attacker.AttackUpgrade > 0)
                        //    {
                        //        weaponDamage *= attacker.AttackUpgrade / 5;
                        //    }

                        // break;
                }
            }
            else
            {
                switch (attacker.AttackUpgrade)
                {
                    case 0:
                        weaponDamage += 0;
                        break;

                    case 1:
                        weaponDamage += (int)(weaponDamage * 0.1);
                        break;

                    case 2:
                        weaponDamage += (int)(weaponDamage * 0.15);
                        break;

                    case 3:
                        weaponDamage += (int)(weaponDamage * 0.22);
                        break;

                    case 4:
                        weaponDamage += (int)(weaponDamage * 0.32);
                        break;

                    case 5:
                        weaponDamage += (int)(weaponDamage * 0.43);
                        break;

                    case 6:
                        weaponDamage += (int)(weaponDamage * 0.54);
                        break;

                    case 7:
                        weaponDamage += (int)(weaponDamage * 0.65);
                        break;

                    case 8:
                        weaponDamage += (int)(weaponDamage * 0.9);
                        break;

                    case 9:
                        weaponDamage += (int)(weaponDamage * 1.2);
                        break;

                    case 10:
                        weaponDamage += weaponDamage * 2;
                        break;

                        //default:
                        //    if (attacker.AttackUpgrade > 0)
                        //    {
                        //        weaponDamage *= attacker.AttackUpgrade / 5;
                        //    }

                        // break;
                }
            }

            #endregion

            baseDamage = (int)((int)((baseDamage + staticBoostCategory3 + weaponDamage + 15) * boostCategory3)
                                * shellBoostCategory3);

            if (attacker.Character?.ChargeValue > 0)
            {
                baseDamage += attacker.Character.ChargeValue;
                attacker.Character.ChargeValue = 0;
                attacker.RemoveBuff(0);
            }

            #endregion

            #region Defense

            if(defender != null && defender.HasBuff(43))
            {
                defender.ArmorDefense = 0;
            }

            switch (attacker.AttackUpgrade)
            {
                //default:
                //    if (attacker.AttackUpgrade < 0)
                //    {
                //        defender.ArmorDefense += defender.ArmorDefense / 5;
                //    }

                //break;

                case -10:
                    defender.ArmorDefense += defender.ArmorDefense * 2;
                    break;

                case -9:
                    defender.ArmorDefense += (int)(defender.ArmorDefense * 1.2);
                    break;

                case -8:
                    defender.ArmorDefense += (int)(defender.ArmorDefense * 0.9);
                    break;

                case -7:
                    defender.ArmorDefense += (int)(defender.ArmorDefense * 0.65);
                    break;

                case -6:
                    defender.ArmorDefense += (int)(defender.ArmorDefense * 0.54);
                    break;

                case -5:
                    defender.ArmorDefense += (int)(defender.ArmorDefense * 0.43);
                    break;

                case -4:
                    defender.ArmorDefense += (int)(defender.ArmorDefense * 0.32);
                    break;

                case -3:
                    defender.ArmorDefense += (int)(defender.ArmorDefense * 0.22);
                    break;

                case -2:
                    defender.ArmorDefense += (int)(defender.ArmorDefense * 0.15);
                    break;

                case -1:
                    defender.ArmorDefense += (int)(defender.ArmorDefense * 0.1);
                    break;

                case 0:
                    defender.ArmorDefense += 0;
                    break;
            }

            int defense = (int)((int)((defender.Defense + defender.ArmorDefense + staticBoostCategory4) * boostCategory4) * shellBoostCategory4);

            if (GetAttackerBenefitingBuffs(CardType.StealBuff, (byte)AdditionalTypes.StealBuff.IgnoreDefenceChance) is int[] IgnoreDefenceChance)
            {
                if (ServerManager.RandomNumber() < IgnoreDefenceChance[0])
                {
                    defense -= (int)(defense * IgnoreDefenceChance[1] / 100D);
                    attacker.MapInstance?.Broadcast(StaticPacketHelper.GenerateEff(realAttacker.UserType, realAttacker.MapEntityId, 30));
                }
            }

            #endregion

            #region Normal Damage

            int normalDamage = (int)((int)((baseDamage + staticBoostCategory2 - defense) * boostCategory2)
                                      * shellBoostCategory2);

            if (normalDamage < 0)
            {
                normalDamage = 0;
            }

            #endregion

            #region Crit Damage

            attacker.CritChance += GetShellWeaponEffectValue(ShellWeaponEffectType.CriticalChance);
            attacker.CritChance -= GetShellArmorEffectValue(ShellArmorEffectType.ReducedCritChanceRecive);
            attacker.CritChance += GetAttackerBenefitingBuffs(CardType.Critical, (byte)AdditionalTypes.Critical.InflictingIncreased)[0];
            attacker.CritChance += GetDefenderBenefitingBuffs(CardType.Critical, (byte)AdditionalTypes.Critical.ReceivingIncreased)[0];
            attacker.CritChance -= GetDefenderBenefitingBuffs(CardType.Critical, (byte)AdditionalTypes.Critical.ReceivingDecreased)[0] * -1;

            attacker.CritRate += GetShellWeaponEffectValue(ShellWeaponEffectType.CriticalDamage);
            attacker.CritRate += GetAttackerBenefitingBuffs(CardType.Critical, (byte)AdditionalTypes.Critical.DamageIncreased)[0];
            attacker.CritRate += GetDefenderBenefitingBuffs(CardType.Critical, (byte)AdditionalTypes.Critical.DamageFromCriticalIncreased)[0];
            attacker.CritRate -= GetDefenderBenefitingBuffs(CardType.Critical, (byte)AdditionalTypes.Critical.DamageFromCriticalDecreased)[0] * -1;
            attacker.CritRate -= GetDefenderBenefitingBuffs(CardType.Count, (byte)AdditionalTypes.Count.DecreaseCriticalDmg)[0] * -1;

            if (defender.CellonOptions != null)
            {
                attacker.CritRate -= defender.CellonOptions.Where(s => s.Type == CellonOptionType.CritReduce)
                    .Sum(s => s.Value);
            }

            if (GetDefenderBenefitingBuffs(CardType.StealBuff, (byte)AdditionalTypes.StealBuff.ReduceCriticalReceivedChance) is int[] ReduceCriticalReceivedChance)
            {
                if (ServerManager.RandomNumber() < ReduceCriticalReceivedChance[0])
                {
                    attacker.CritRate -= (int)(attacker.CritRate * ReduceCriticalReceivedChance[1] / 100D);
                }
            }

            if (defender.GetBuff(CardType.SpecialCritical, (byte)AdditionalTypes.SpecialCritical.ReceivingChancePercent)[0] is int Rate)
            {
                if (Rate < 0) // If > 0 is benefit defender buff
                {
                    if (attacker.CritChance < -Rate)
                    {
                        attacker.CritChance = -Rate * -1;
                    }
                }
            }

            if (defender.HasBuff(CardType.SpecialCritical, (byte)AdditionalTypes.SpecialCritical.AlwaysReceives))
            {
                attacker.CritChance = 100;
            }

            if (defender.HasBuff(CardType.SpecialCritical, (byte)AdditionalTypes.SpecialCritical.NeverReceives))
            {
                attacker.CritChance = 0;
            }

            if (skill?.SkillVNum == 1124 && GetAttackerBenefitingBuffs(CardType.SniperAttack, (byte)AdditionalTypes.SniperAttack.ReceiveCriticalFromSniper)[0] is int ReceiveCriticalFromSniper)
            {
                if (ReceiveCriticalFromSniper > 0)
                {
                    attacker.CritChance = ReceiveCriticalFromSniper;
                }
            }

            if (skill?.SkillVNum == 1248
                || (ServerManager.RandomNumber() < attacker.CritChance && attacker.AttackType != AttackType.Magical && !attacker.HasBuff(CardType.SpecialCritical, (byte)AdditionalTypes.SpecialCritical.NeverInflict)))
            {
                double multiplier = (double)attacker.CritRate / 100D;

                if (multiplier > 3)
                {
#warning Disabled Critical Rate limit

                    // multiplier = 3;
                }

                normalDamage += (int)((double)normalDamage * multiplier);

                if (GetDefenderBenefitingBuffs(CardType.VulcanoElementBuff, (byte)AdditionalTypes.VulcanoElementBuff.CriticalDefence)[0] is int CriticalDefence)
                {
                    if (CriticalDefence > 0 && normalDamage > CriticalDefence)
                    {
                        normalDamage = CriticalDefence;
                    }
                }

                hitMode = 3;
            }

            #endregion

            #region Fairy Damage

            int fairyDamage = (int)((baseDamage + 100) * realAttacker.ElementRate / 100D);

            #endregion

            #region Elemental Damage Advantage

            double elementalBoost = 0;

            switch (realAttacker.Element)
            {
                case 0:
                    break;

                case 1:
                    defender.Resistance = defender.FireResistance;
                    switch (defender.Element)
                    {
                        case 0:
                            elementalBoost = 1.3; // Damage vs no element
                            break;

                        case 1:
                            elementalBoost = 1; // Damage vs fire
                            break;

                        case 2:
                            elementalBoost = 2; // Damage vs water
                            break;

                        case 3:
                            elementalBoost = 1; // Damage vs light
                            break;

                        case 4:
                            elementalBoost = 1.5; // Damage vs darkness
                            break;
                    }

                    break;

                case 2:
                    defender.Resistance = defender.WaterResistance;
                    switch (defender.Element)
                    {
                        case 0:
                            elementalBoost = 1.3;
                            break;

                        case 1:
                            elementalBoost = 2;
                            break;

                        case 2:
                            elementalBoost = 1;
                            break;

                        case 3:
                            elementalBoost = 1.5;
                            break;

                        case 4:
                            elementalBoost = 1;
                            break;
                    }

                    break;

                case 3:
                    defender.Resistance = defender.LightResistance;
                    switch (defender.Element)
                    {
                        case 0:
                            elementalBoost = 1.3;
                            break;

                        case 1:
                            elementalBoost = 1.5;
                            break;

                        case 2:
                        case 3:
                            elementalBoost = 1;
                            break;

                        case 4:
                            elementalBoost = 3;
                            break;
                    }

                    break;

                case 4:
                    defender.Resistance = defender.ShadowResistance;
                    switch (defender.Element)
                    {
                        case 0:
                            elementalBoost = 1.3;
                            break;

                        case 1:
                            elementalBoost = 1;
                            break;

                        case 2:
                            elementalBoost = 1.5;
                            break;

                        case 3:
                            elementalBoost = 3;
                            break;

                        case 4:
                            elementalBoost = 1;
                            break;
                    }

                    break;
            }

            if (/*skill?.Element == 0 || */(skill?.Element != 0 && skill?.Element != realAttacker.Element && realAttacker.EntityType == EntityType.Player))
            {
                //elementalBoost = 0;
            }

            #endregion

            #region Elemental Damage

            int elementalDamage =
                (int)((int)((int)((int)((staticBoostCategory5 + fairyDamage) * elementalBoost)
                                     * (1 - (defender.Resistance / 100D))) * boostCategory5) * shellBoostCategory5);

            if (elementalDamage < 0)
            {
                elementalDamage = 0;
            }

            #endregion

            #region Total Damage

            if (!percentDamage)
            {
                totalDamage =
                    (int)((int)((normalDamage + elementalDamage + attacker.Morale + staticBoostCategory1)
                                  * boostCategory1) * shellBoostCategory1);


                var newBoost = boostCategory2 < 1 ? 1 : boostCategory2;

                totalDamage = (int)(totalDamage * newBoost);

                if ((attacker.EntityType == EntityType.Player || attacker.EntityType == EntityType.Mate)
                    && (defender.EntityType == EntityType.Player || defender.EntityType == EntityType.Mate))
                {
                    totalDamage /= 2;
                }

                #region HP %

                if (attacker != null && defender != null)
                {
                    if (defender.HasBuff(768) && defender.EntityType == EntityType.Monster)
                    {
                        defender?.MapInstance?.Broadcast(StaticPacketHelper.GenerateEff(defender.UserType, defender.MapEntityId, 6008));
                        attacker?.Character?.Session?.CurrentMapInstance?.Broadcast(attacker?.GenerateDm(attacker.HpMax * 25 / 100));
                        if (attacker.Hp > attacker.HpMax * 25 / 100 + 1)
                        {
                            realAttacker?.MapInstance?.Broadcast(StaticPacketHelper.GenerateEff(realAttacker.UserType, realAttacker.MapEntityId, 4072));
                            attacker.Hp -= attacker.HpMax * 25 / 100;
                        }
                        else
                        {
                            attacker.Hp = 1;
                        }
                    }
                }

                #endregion

                #region Fairy Faction Damage

                if (attacker.Character != null && defender.Character != null)
                {
                    var angelDamageBcards = attacker.BCards.Where(s => s.Type == (byte)CardType.PvPFairyEffects && s.SubType == (byte)AdditionalTypes.PvpFairyEffect.IncreaseDamageVsAngels);
                    var demonDamageBCards = attacker.BCards.Where(s => s.Type == (byte)CardType.PvPFairyEffects && s.SubType == (byte)AdditionalTypes.PvpFairyEffect.IncreaseDamageVsDemons);

                    if (angelDamageBcards != null && defender.EntityType == EntityType.Player && defender.Character.Faction == FactionType.Angel)
                    {
                        totalDamage = angelDamageBcards.Aggregate(totalDamage, (current, angelDamage) => current + angelDamage.FirstData * current / 100);
                        attacker.MapInstance?.Broadcast(StaticPacketHelper.GenerateEff(realAttacker.UserType, realAttacker.MapEntityId, 195));
                    }

                    if (demonDamageBCards != null && defender.EntityType == EntityType.Player && defender.Character.Faction == FactionType.Demon)
                    {
                        totalDamage = demonDamageBCards.Aggregate(totalDamage, (current, demonDamage) => current + demonDamage.FirstData * current / 100);
                        attacker.MapInstance?.Broadcast(StaticPacketHelper.GenerateEff(realAttacker.UserType, realAttacker.MapEntityId, 195));
                    }
                }

                #endregion



                #region Fairy Faction Damage

                if (attacker.Character != null && defender.Character != null)
                {
                    var angelDamageBcards = attacker.BCards.Where(s => s.Type == (byte)CardType.PvPFairyEffects && s.SubType == (byte)AdditionalTypes.PvpFairyEffect.IncreaseDamageVsAngels);
                    var demonDamageBCards = attacker.BCards.Where(s => s.Type == (byte)CardType.PvPFairyEffects && s.SubType == (byte)AdditionalTypes.PvpFairyEffect.IncreaseDamageVsDemons);

                    if (angelDamageBcards != null && defender.EntityType == EntityType.Player && defender.Character.Faction == FactionType.Angel)
                    {
                        totalDamage = angelDamageBcards.Aggregate(totalDamage, (current, angelDamage) => current + angelDamage.FirstData * current / 100);
                        attacker.MapInstance?.Broadcast(StaticPacketHelper.GenerateEff(realAttacker.UserType, realAttacker.MapEntityId, 195));
                    }

                    if (demonDamageBCards != null && defender.EntityType == EntityType.Player && defender.Character.Faction == FactionType.Demon)
                    {
                        totalDamage = demonDamageBCards.Aggregate(totalDamage, (current, demonDamage) => current + demonDamage.FirstData * current / 100);
                        attacker.MapInstance?.Broadcast(StaticPacketHelper.GenerateEff(realAttacker.UserType, realAttacker.MapEntityId, 195));
                    }
                }


                #endregion

                #region Mega Titan Hardcode(custom)

                if (attacker != null && defender != null)
                {
                    if (realAttacker == attacker && attacker.Buffs.ContainsKey(755) && ServerManager.RandomNumber(0, 100) < 3)
                    {
                        attacker.AddBuff(new Buff(7000, attacker.Level), attacker);
                    }
                }

                if (attacker != null && defender != null)
                {
                    if (realAttacker == attacker && attacker.Buffs.ContainsKey(663) && ServerManager.RandomNumber(0, 100) < 20)
                    {
                        attacker.AddBuff(new Buff(664, attacker.Level), attacker);
                    }
                }

                #endregion

                #region Pen Defence

                if (attacker != null && defender != null)
                {
                    if (attacker.Character != null)
                    {
                        if (realAttacker == attacker &&
                            attacker.Character.Inventory.Any(i => i.Value.Type == InventoryType.Wear && (i.Value.ItemVNum == 30004 || i.Value.ItemVNum == 30005 || i.Value.ItemVNum == 30006)) &&
                            ServerManager.RandomNumber(0, 100) < 8)
                        {
                            defender.AddBuff(new Buff(5000, defender.Level), defender);
                        }
                    }
                }

                #endregion

                #region Drain Power on Item

                if (attacker != null && defender != null)
                {
                    if (attacker.Character != null)
                    {
                        if (realAttacker == attacker &&
                            attacker.Character.Inventory.Any(i => i.Value.Type == InventoryType.Wear && (i.Value.ItemVNum == 30071 || i.Value.ItemVNum == 30072 || i.Value.ItemVNum == 30073)) &&
                            ServerManager.RandomNumber(0, 100) < 5)
                        {
                            attacker.AddBuff(new Buff(79, attacker.Level), attacker);
                        }
                    }
                }

                #endregion

                #region Enraged

                if (attacker != null && defender != null)
                {
                    if (attacker.Character != null)
                    {
                        if (realAttacker == attacker &&
                            attacker.Character.Inventory.Any(i => i.Value.Type == InventoryType.Wear && (i.Value.ItemVNum == 30046 || i.Value.ItemVNum == 30047 || i.Value.ItemVNum == 30048 || i.Value.ItemVNum == 30049 || i.Value.ItemVNum == 4321 || i.Value.ItemVNum == 4323)) &&
                            ServerManager.RandomNumber(0, 100) < 5)
                        {
                            attacker.AddBuff(new Buff(8008, defender.Level), attacker);
                        }
                    }
                }

                #endregion

                #region Rainbow Blessing

                if (attacker != null && defender != null)
                {
                    if (attacker.Character != null)
                    {
                        if (realAttacker == attacker &&
                            attacker.Character.Inventory.Any(i => i.Value.Type == InventoryType.Wear && (i.Value.ItemVNum == 30013 || i.Value.ItemVNum == 30014)) &&
                            ServerManager.RandomNumber(0, 100) < 5)
                        {
                            attacker.AddBuff(new Buff(5003, attacker.Level), attacker);
                        }
                    }
                }

                #endregion


                #region Damage to Heal

                if (attacker != null && defender != null)
                {
                    if (defender.HasBuff(8000))
                    {
                        if (defender.Hp <= defender.HpMax - totalDamage * 2)
                        {
                            //rc 3 15409 198 0
                            defender?.MapInstance?.Broadcast(StaticPacketHelper.GenerateEff(defender.UserType, defender.MapEntityId, 6008));
                            defender?.MapInstance.Broadcast($"rc 3 {defender.MapMonster?.MapMonsterId} {totalDamage * 2} 0");
                            defender.Hp += totalDamage * 2;
                        }
                        else
                        {
                            defender.Hp = defender.HpMax;
                        }
                    }
                }

                #endregion

                #region dodge on buff

                int[] dodgeChance = defender.GetBuff(CardType.GuarantedDodgeRangedAttack, (byte)AdditionalTypes.GuarantedDodgeRangedAttack.AlwaysDodgePropability);

                if (ServerManager.RandomNumber() < dodgeChance[0])
                {
                    hitMode = 4;
                    totalDamage = 0;
                }

                #endregion

                #region lifesteal

                int[] lifesteal = attacker.GetBuff(CardType.AbsorptionAndPowerSkill, (byte)AdditionalTypes.AbsorptionAndPowerSkill.StealPercentageHpFromEnemy);

                var lifestealAmount = (int)((totalDamage * lifesteal[0]) / 100D);

                if (!defender.HasBuff(72) || hitMode != 4)
                {
                    if (attacker.Hp < attacker.HpMax && defender.Hp > lifestealAmount)
                    {
                        attacker?.Character?.Session?.CurrentMapInstance?.Broadcast(attacker?.GenerateRc(lifestealAmount));
                        defender?.Character?.Session?.CurrentMapInstance?.Broadcast(defender?.GenerateDm(lifestealAmount));
                        attacker?.MapMonster?.MapInstance?.Broadcast(attacker?.GenerateRc(lifestealAmount));
                        defender?.MapMonster?.MapInstance?.Broadcast(defender?.GenerateDm(lifestealAmount));
                        attacker.Hp += lifestealAmount;
                        defender.Hp -= lifestealAmount;
                    }
                }


                #endregion

                if (defender.EntityType == EntityType.Monster || defender.EntityType == EntityType.Npc)
                {
                    //totalDamage -= GetMonsterDamageBonus(defender.Level);
                }

                if (totalDamage < 5 && boostCategory1 > 0 && shellBoostCategory1 > 0)
                {
                    totalDamage = ServerManager.RandomNumber(1, 6);
                }

                if (attacker.EntityType == EntityType.Monster || attacker.EntityType == EntityType.Npc)
                {
                    if (totalDamage < GetMonsterDamageBonus(attacker.Level) && boostCategory1 > 0 && shellBoostCategory1 > 0)
                    {
                        totalDamage = GetMonsterDamageBonus(attacker.Level);
                    }
                }

                if (realAttacker != attacker)
                {
                    totalDamage /= 2;
                }
            }
            if (totalDamage <= 0)
            {
                totalDamage = 1;
            }

            #endregion

            #region Onyx Wings

            int[] onyxBuff = GetAttackerBenefitingBuffs(CardType.StealBuff,
                (byte)AdditionalTypes.StealBuff.ChanceSummonOnyxDragon);
            if (onyxBuff[0] > ServerManager.RandomNumber() && (skill.CastId != 0))
            {
                onyxWings = true;
            }

            #endregion

            #region Zephyr Wings

            int[] zephyrBuff = GetAttackerBenefitingBuffs(CardType.DragonSkills, (byte)AdditionalTypes.DragonSkills.Wings);

            if ((attacker.AttackType == AttackType.Magical || attacker.AttackType == AttackType.Range) && zephyrBuff[0] > ServerManager.RandomNumber())
            {
                zephyrWings = true;
            }

            #endregion

            if (defender.Character != null && defender.HasBuff(CardType.NoDefeatAndNoDamage, (byte)AdditionalTypes.NoDefeatAndNoDamage.TransferAttackPower))
            {
                if (!percentDamage)
                {
                    defender.Character.ChargeValue = totalDamage;
                    if (defender.Character.ChargeValue > 7000) defender.Character.ChargeValue = 7000;
                    defender.AddBuff(new Buff(0, defender.Level), defender);
                }
                hitMode = 4;
                return 0;
            }

            #region AbsorptionAndPowerSkill

            int[] addDamageToHp = defender.GetBuff(CardType.AbsorptionAndPowerSkill, (byte)AdditionalTypes.AbsorptionAndPowerSkill.AddDamageToHP);

            if (addDamageToHp[0] > 0)
            {
                int damageToHp = (int)(totalDamage / 100D * addDamageToHp[0]);

                if (defender.Hp + damageToHp > defender.HpMax)
                {
                    damageToHp = defender.HpMax - defender.Hp;
                }

                if (damageToHp > 0)
                {
                    defender.MapInstance?.Broadcast(defender.GenerateRc(damageToHp));

                    defender.Hp = Math.Min(defender.Hp + damageToHp, defender.HpMax);

                    if (defender.Character != null)
                    {
                        defender.Character.Session?.SendPacket(defender.Character.GenerateStat());
                    }
                }

                hitMode = 0;
                return 0;
            }

            #endregion

            if (defender.GetBuff(CardType.SecondSPCard, (byte)AdditionalTypes.SecondSPCard.HitAttacker) is int[] CounterDebuff)
            {
                if (ServerManager.RandomNumber() < CounterDebuff[0])
                {
                    realAttacker.AddBuff(new Buff((short)CounterDebuff[1], defender.Level), defender);
                }
            }

            #region ReflectMaximumDamageFrom

            if (defender.GetBuff(CardType.TauntSkill, (byte)AdditionalTypes.TauntSkill.ReflectMaximumDamageFrom) is int[] ReflectsMaximumDamageFrom)
            {
                if (ReflectsMaximumDamageFrom[0] < 0)
                {
                    int maxReflectDamage = -ReflectsMaximumDamageFrom[0];

                    int reflectedDamage = Math.Min(totalDamage, maxReflectDamage);
                    totalDamage = 0;
                    hitMode = 4;

                    if (!percentDamage)
                    {
                        reflectedDamage = realAttacker.GetDamage(reflectedDamage, defender, true);

                        defender.MapInstance.Broadcast(StaticPacketHelper.SkillUsed(realAttacker.UserType, realAttacker.MapEntityId, (byte)realAttacker.UserType, realAttacker.MapEntityId,
                            -1, 0, 0, 0, 0, 0, realAttacker.Hp > 0, (int)(realAttacker.Hp / realAttacker.HPLoad() * 100), reflectedDamage, 0, 1));

                        defender.Character?.Session?.SendPacket(defender.Character.GenerateStat());
                    }
                }
            }

            if (defender.GetBuff(CardType.TauntSkill, (byte)AdditionalTypes.TauntSkill.ReflectsMaximumDamageFromNegated) is int [] ReflectsMaximumDamageFromNegated)
                if (ReflectsMaximumDamageFromNegated[0] > 0)
                {
                    int maxReflectDamage = ReflectsMaximumDamageFromNegated[0];

                    int reflectedDamage = Math.Min(totalDamage, maxReflectDamage);
                    totalDamage -= reflectedDamage;

                    if (!percentDamage)
                    {
                        reflectedDamage = realAttacker.GetDamage(reflectedDamage, defender, true);

                        defender.MapInstance.Broadcast(StaticPacketHelper.SkillUsed(realAttacker.UserType,
                            realAttacker.MapEntityId, (byte)realAttacker.UserType, realAttacker.MapEntityId,
                            -1, 0, 0, 0, 0, 0, realAttacker.Hp > 0,
                            (int)(realAttacker.Hp / realAttacker.HPLoad() * 100), reflectedDamage, 0, 1));

                        defender.Character?.Session?.SendPacket(defender.Character.GenerateStat());
                    }
                }

            #endregion

            #region ReflectMaximumReceivedDamage

            if (defender.GetBuff(CardType.DamageConvertingSkill, (byte)AdditionalTypes.DamageConvertingSkill.ReflectMaximumReceivedDamage) is int[] ReflectMaximumReceivedDamage)
            {
                if (ReflectMaximumReceivedDamage[0] > 0)
                {
                    int maxReflectDamage = ReflectMaximumReceivedDamage[0];

                    int reflectedDamage = Math.Min(totalDamage, maxReflectDamage);
                    totalDamage = 0;
                    hitMode = 4;

                    if (!percentDamage)
                    {
                        reflectedDamage = realAttacker.GetDamage(reflectedDamage, defender, true);

                        defender.MapInstance.Broadcast(StaticPacketHelper.SkillUsed(realAttacker.UserType, realAttacker.MapEntityId, (byte)realAttacker.UserType, realAttacker.MapEntityId,
                            -1, 0, 0, 0, 0, 0, realAttacker.Hp > 0, (int)(realAttacker.Hp / realAttacker.HPLoad() * 100), reflectedDamage, 0, 1));

                        defender.Character?.Session?.SendPacket(defender.Character.GenerateStat());
                    }
                }
            }

            #endregion

            if (defender.Buffs.FirstOrDefault(s => s.Card.BCards.Any(b => b.Type.Equals((byte)CardType.DamageConvertingSkill) && b.SubType.Equals((byte)AdditionalTypes.DamageConvertingSkill.TransferInflictedDamage)))?.Sender is BattleEntity TransferInflictedDamageSender)
            {
                if (defender.GetBuff(CardType.DamageConvertingSkill, (byte)AdditionalTypes.DamageConvertingSkill.TransferInflictedDamage) is int[] TransferInflictedDamage)
                {
                    if (TransferInflictedDamage[0] > 0)
                    {
                        int transferedDamage = (int)(totalDamage * TransferInflictedDamage[0] / 100d);
                        totalDamage -= transferedDamage;
                        TransferInflictedDamageSender.GetDamage(transferedDamage, defender, true);
                        if (TransferInflictedDamageSender.Hp - transferedDamage <= 0)
                        {
                            transferedDamage = TransferInflictedDamageSender.Hp - 1;
                        }
                        defender.MapInstance.Broadcast(StaticPacketHelper.SkillUsed(realAttacker.UserType, realAttacker.MapEntityId, (byte)TransferInflictedDamageSender.UserType, TransferInflictedDamageSender.MapEntityId,
                                        skill?.SkillVNum ?? 0, skill?.Cooldown ?? 0,
                                        0, skill?.Effect ?? attacker.Mate?.Monster.BasicSkill ?? attacker.MapMonster?.Monster.BasicSkill ?? attacker.MapNpc?.Npc.BasicSkill ?? 0, defender.PositionX, defender.PositionY,
                                        TransferInflictedDamageSender.Hp > 0,
                                        (int)(TransferInflictedDamageSender.Hp / TransferInflictedDamageSender.HPLoad() * 100), transferedDamage,
                                        0, 1));
                        if (TransferInflictedDamageSender.Character != null)
                        {
                            TransferInflictedDamageSender.Character.Session.SendPacket(TransferInflictedDamageSender.Character.GenerateStat());
                        }
                    }
                }
            }

            totalDamage = Math.Max(0, totalDamage);

            // TODO: Find a better way because hardcoded There is no clue about this in DB?

            // Convert && Corruption

            if (skill?.SkillVNum == 1348 && defender.HasBuff(628))
            {
                int bonusDamage = totalDamage / 2;

                if (defender.Character != null)
                {
                    bonusDamage /= 2;

                    defender.GetDamage(bonusDamage, realAttacker, true);

                    defender.MapInstance.Broadcast(StaticPacketHelper.SkillUsed(realAttacker.UserType, realAttacker.MapEntityId, (byte)defender.UserType, defender.MapEntityId,
                        skill.SkillVNum, skill.Cooldown, 0, skill.Effect, defender.PositionX, defender.PositionY, defender.Hp > 0, (int)(defender.Hp / defender.HPLoad() * 100), bonusDamage, 0, 1));

                    defender.Character.Session.SendPacket(defender.Character.GenerateStat());
                }
                else
                {
                    defender.GetDamage(bonusDamage, realAttacker, true);

                    defender.MapInstance.Broadcast(StaticPacketHelper.SkillUsed(realAttacker.UserType, realAttacker.MapEntityId, (byte)defender.UserType, defender.MapEntityId,
                        skill.SkillVNum, skill.Cooldown, 0, skill.Effect, defender.PositionX, defender.PositionY, defender.Hp > 0, (int)(defender.Hp / defender.HPLoad() * 100), bonusDamage, 0, 1));
                }

                defender.RemoveBuff(628);
            }

            // Spirit Splitter && Mark of Death
            else if (skill?.SkillVNum == 1178 && defender.HasBuff(597))
            {
                totalDamage *= 2;

                defender.RemoveBuff(597);
            }

            // Holy Explosion && Illuminating Powder
            else if (skill?.SkillVNum == 1326 && defender.HasBuff(619))
            {
                defender.GetDamage(totalDamage, realAttacker, true);

                defender.MapInstance.Broadcast(StaticPacketHelper.SkillUsed(realAttacker.UserType, realAttacker.MapEntityId, (byte)defender.UserType, defender.MapEntityId,
                        skill.SkillVNum, skill.Cooldown, 0, skill.Effect, defender.PositionX, defender.PositionY, defender.Hp > 0, (int)(defender.Hp / defender.HPLoad() * 100), totalDamage, 0, 1));

                defender.RemoveBuff(619);
            }

            if (attacker.Character != null)
            {
                var damageIncrease = 0;

                if (defender.MapMonster != null)
                {
                    short[] vnums =
                    {
                        1161, 2282, 1030, 1244, 1218, 5369, 1012, 1363, 1364, 2160, 2173, 5959, 5983, 2514,
                        2515, 2516, 2517, 2518, 2519, 2520, 2521, 1685, 1686, 5087, 5203, 2418, 2310, 2303,
                        2169, 2280, 5892, 5893, 5894, 5895, 5896, 5897, 5898, 5899, 5332, 5105, 2161, 2162,
                        5560, 5591, 4099, 907, 1160, 4705, 4706, 4707, 4708, 4709, 4710, 4711, 4712, 4713,
                        4714, 4715, 4716, 361, 362, 363, 366, 367, 368, 371, 372, 373, 1386, 1387, 1388,
                        1389, 1390, 1391, 1392, 1393, 1394, 1395, 1396, 1397, 1398, 1399, 1400, 1401, 1402,
                        1403, 1404, 1405
                    };

                    // Damage Vs Sealed Monster
                    if (attacker.HasBuff(CardType.IncreaseDamageVsMonsterInMap, (byte)AdditionalTypes.IncreaseDamageVsMonsterInMap.IncreaseDamageVsSealledMonster) &&
                        vnums.Contains(defender.MapMonster.MonsterVNum))
                    {
                        damageIncrease = (int)(totalDamage * (attacker.GetBuff(CardType.IncreaseDamageVsMonsterInMap,
                            (byte)AdditionalTypes.IncreaseDamageVsMonsterInMap.IncreaseDamageVsSealledMonster)[0] * 0.01));
                    }

                    // Kovolts
                    if (attacker.HasBuff(CardType.IncreaseDamageVsEntity, (byte)AdditionalTypes.IncreaseDamageVsEntity.IncreaseDamageVsKovolts) &&
                        defender.MapMonster.Monster.Race == 2 && defender.MapMonster.Monster.RaceType == 0)
                    {
                        damageIncrease = (int)(totalDamage * (attacker.GetBuff(CardType.IncreaseDamageVsEntity, (byte)AdditionalTypes.IncreaseDamageVsEntity.IncreaseDamageVsKovolts)[0] * 0.01));
                    }

                    // Catsys
                    if (attacker.HasBuff(CardType.IncreaseDamageVsEntity, (byte)AdditionalTypes.IncreaseDamageVsEntity.IncreaseDamageVsCatsys) &&
                        defender.MapMonster.Monster.Race == 2 && defender.MapMonster.Monster.RaceType == 0)
                    {
                        damageIncrease = (int)(totalDamage * (attacker.GetBuff(CardType.IncreaseDamageVsEntity, (byte)AdditionalTypes.IncreaseDamageVsEntity.IncreaseDamageVsCatsys)[0] * 0.01));
                    }

                    // Plants
                    if (attacker.HasBuff(CardType.IncreaseDamageVsEntity, (byte)AdditionalTypes.IncreaseDamageVsEntity.IncreaseDamageVsPlants) &&
                        defender.MapMonster.Monster.Race == 0 && defender.MapMonster.Monster.RaceType == 0)
                    {
                        damageIncrease = (int)(totalDamage * (attacker.GetBuff(CardType.IncreaseDamageVsEntity, (byte)AdditionalTypes.IncreaseDamageVsEntity.IncreaseDamageVsPlants)[0] * 0.01));
                    }

                    // Animals
                    if (attacker.HasBuff(CardType.IncreaseDamageVsEntity, (byte)AdditionalTypes.IncreaseDamageVsEntity.IncreaseDamageVsAnimals) &&
                        defender.MapMonster.Monster.Race == 0 && defender.MapMonster.Monster.RaceType == 1)
                    {
                        damageIncrease = (int)(totalDamage * (attacker.GetBuff(CardType.IncreaseDamageVsEntity, (byte)AdditionalTypes.IncreaseDamageVsEntity.IncreaseDamageVsAnimals)[0] * 0.01));
                    }

                    // Monster
                    if (attacker.HasBuff(CardType.IncreaseDamageVsEntity, (byte)AdditionalTypes.IncreaseDamageVsEntity.IncreaseDamageVsMonster) &&
                        defender.MapMonster.Monster.Race == 0 && defender.MapMonster.Monster.RaceType == 2)
                    {
                        damageIncrease = (int)(totalDamage * (attacker.GetBuff(CardType.IncreaseDamageVsEntity, (byte)AdditionalTypes.IncreaseDamageVsEntity.IncreaseDamageVsMonster)[0] * 0.01));
                    }

                    if (attacker?.Character?.Session?.CurrentMapInstance?.MapInstanceType == MapInstanceType.LodInstance)
                    {
                        // Increase Damage in Lod
                        if (attacker.HasBuff(CardType.IncreaseDamageVsMonsterInMap, (byte)AdditionalTypes.IncreaseDamageVsMonsterInMap.IncreaseDamageInLod))
                        {
                            damageIncrease = (int)(totalDamage * (attacker.GetBuff(CardType.IncreaseDamageVsMonsterInMap, (byte)AdditionalTypes.IncreaseDamageVsMonsterInMap.IncreaseDamageInLod)[0] * 0.01));
                        }
                    }
                }

                if (defender.Character != null)
                {
                    // No Faction ?? probably idk
                    if (attacker.HasBuff(CardType.IncreaseDamageVsFaction, (byte)AdditionalTypes.IncreaseDamageVsFaction.IncreaseDamageVsSouls) && defender.Character.Faction == FactionType.None)
                    {
                        damageIncrease = (int)(totalDamage * (attacker.GetBuff(CardType.IncreaseDamageVsFaction, (byte)AdditionalTypes.IncreaseDamageVsFaction.IncreaseDamageVsSouls)[0] * 0.01));
                    }

                    // Increase Damage Vs Angel
                    if (attacker.HasBuff(CardType.IncreaseDamageVsFaction, (byte)AdditionalTypes.IncreaseDamageVsFaction.IncreaseDamageVsAngel) && defender.Character.Faction == FactionType.Angel)
                    {
                        damageIncrease = (int)(totalDamage * (attacker.GetBuff(CardType.IncreaseDamageVsFaction, (byte)AdditionalTypes.IncreaseDamageVsFaction.IncreaseDamageVsAngel)[0] * 0.01));
                    }

                    // Increase Damage Vs Devil
                    if (attacker.HasBuff(CardType.IncreaseDamageVsFaction, (byte)AdditionalTypes.IncreaseDamageVsFaction.IncreaseDamageVsDevil) && defender.Character.Faction == FactionType.Demon)
                    {
                        damageIncrease = (int)(totalDamage * (attacker.GetBuff(CardType.IncreaseDamageVsFaction, (byte)AdditionalTypes.IncreaseDamageVsFaction.IncreaseDamageVsDevil)[0] * 0.01));
                    }
                }

                totalDamage = (totalDamage + damageIncrease);
            }

            var chanceCriticalDefence = GetDefenderBenefitingBuffs(BCardType.CardType.VulcanoElementBuff, (byte)AdditionalTypes.VulcanoElementBuff.CriticalDefence);

            if (chanceCriticalDefence[0] > 0 && totalDamage > chanceCriticalDefence[0] && hitMode == 3)
            {
                totalDamage = chanceCriticalDefence[0];
            }

            // recovery hp in def
            if (defender.Character != null)
            {
                int bo = defender.Character.GetShellArmor(ShellArmorEffectType.RecoveryHPInDefence);
                if (bo > 0)
                {
                    int heal = (int)(totalDamage / 10 * (bo * 0.01));
                    if (heal > 0)
                    {
                        if (defender.Character.Hp + heal < defender.Character.HPLoad())
                        {
                            defender.Character.Hp += heal;
                            defender.Character.Session?.CurrentMapInstance?.Broadcast(defender.Character.GenerateRc(heal));
                        }
                        else
                        {
                            if (defender.Character.Hp != (int)defender.Character.HPLoad())
                            {
                                defender.Character.Session?.CurrentMapInstance?.Broadcast(defender.Character.GenerateRc((int)(defender.Character.HPLoad() - defender.Character.Hp)));
                            }
                            defender.Character.Hp = (int)defender.Character.HPLoad();
                        }
                        defender.Character.Session?.CurrentMapInstance?.Broadcast($"eff 1 {defender.Character.Session.Character.CharacterId} 3019");
                    }
                }
            }

            if (defender.HasBuff(475) || defender.HasBuff(767))
            {
                return 0;
            }

            if (defender.HasBuff(CardType.IncreaseRes, (byte)AdditionalTypes.IncreaseRes.DecreaseTotalDamageBy))
            {
                var data = attacker.GetBuff(CardType.IncreaseRes,
                        (byte)AdditionalTypes.IncreaseRes.DecreaseTotalDamageBy)[0];
                totalDamage -= data;
            }

            if (attacker.HasBuff(CardType.DealMoreDamage, (byte)AdditionalTypes.DealMoreDamage.IncreaseDamagePercent))
            {
                var data = attacker.GetBuff(CardType.DealMoreDamage,
                        (byte)AdditionalTypes.DealMoreDamage.IncreaseDamagePercent)[0];
                var data2 = attacker.GetBuff(CardType.DealMoreDamage,
                        (byte)AdditionalTypes.DealMoreDamage.IncreaseDamagePercent)[1];
                if (data < ServerManager.RandomNumber())
                {
                    totalDamage = totalDamage + (totalDamage * data2 / 100);
                }
            }

            if (attacker.HasBuff(CardType.DealMoreDamage, (byte)AdditionalTypes.DealMoreDamage.IncreaseDamage))
            {
                var data = attacker.GetBuff(CardType.DealMoreDamage,
                        (byte)AdditionalTypes.DealMoreDamage.IncreaseDamage)[0];
                var data2 = attacker.GetBuff(CardType.DealMoreDamage,
                        (byte)AdditionalTypes.DealMoreDamage.IncreaseDamage)[1];
                if (data < ServerManager.RandomNumber())
                {
                    totalDamage = totalDamage + (totalDamage * data2 / 100);
                }
            }

            if (attacker.HasBuff(CardType.DealMoreDamage, (byte)AdditionalTypes.DealMoreDamage.IncreaseDamageLoa))
            {
                var data = attacker.GetBuff(CardType.DealMoreDamage,
                        (byte)AdditionalTypes.DealMoreDamage.IncreaseDamageLoa)[0];
                var data2 = attacker.GetBuff(CardType.DealMoreDamage,
                        (byte)AdditionalTypes.DealMoreDamage.IncreaseDamageLoa)[1];
                if (data < ServerManager.RandomNumber())
                {
                    totalDamage = totalDamage + (totalDamage * data2 / 100);
                }
            }

            if (attacker.HasBuff(CardType.DealMoreDamage, (byte)AdditionalTypes.DealMoreDamage.IncreaseDamageSnake))
            {
                var data = attacker.GetBuff(CardType.DealMoreDamage,
                        (byte)AdditionalTypes.DealMoreDamage.IncreaseDamageSnake)[0];
                var data2 = attacker.GetBuff(CardType.DealMoreDamage,
                        (byte)AdditionalTypes.DealMoreDamage.IncreaseDamageSnake)[1];
                if (data < ServerManager.RandomNumber())
                {
                    totalDamage = totalDamage + (totalDamage * data2 / 100);
                }
            }

            if (defender.HasBuff(CardType.SpawnEqMonster, (byte)AdditionalTypes.SpawnEqMonster.Deal50PercentDamage))
            {
                var data = attacker.GetBuff(CardType.IncreaseDamageVsEntity,
                        (byte)AdditionalTypes.IncreaseDamageVsEntity.IncreaseDamageVsPlants)[0];
                if (data < ServerManager.RandomNumber())
                {
                    realAttacker.GetDamage(totalDamage / 2, defender, false);
                    defender.MapInstance.Broadcast(
                            StaticPacketHelper.SkillUsed(realAttacker.UserType, realAttacker.MapEntityId,
                                    (byte)realAttacker.UserType, realAttacker.MapEntityId,
                                    -1, 0, 0, 0, 0, 0,
                                    realAttacker.Hp > 0, (int)(realAttacker.Hp / realAttacker.HPLoad() * 100),
                                    totalDamage / 2, 0, 1));
                    defender.Character?.Session?.SendPacket(defender.Character.GenerateStat());
                }
            }

            if (attacker.HasBuff(CardType.GiveBuff, (byte)AdditionalTypes.GiveBuff.GiveBuffToEntity))
            {
                var data = attacker.GetBuff(CardType.GiveBuff,
                        (byte)AdditionalTypes.GiveBuff.GiveBuffToEntity)[0];
                if (data < ServerManager.RandomNumber())
                {
                    var buff = attacker.GetBuff(CardType.GiveBuff, (byte)AdditionalTypes.GiveBuff.GiveBuffToEntity)[1];
                    defender.AddBuff(new Buff((short)buff, attacker.Level), attacker);
                }
            }

            if (hitMode == 3)
            {

                if (defender.HasBuff(CardType.ReverteDamage, (byte)AdditionalTypes.ReverteDamage.ByPercent))
                {
                    var data = attacker.GetBuff(CardType.ReverteDamage,
                            (byte)AdditionalTypes.ReverteDamage.ByPercent)[0];
                    if (data < ServerManager.RandomNumber())
                    {
                        var damage = totalDamage - (totalDamage * attacker.GetBuff(CardType.ReverteDamage,
                                (byte)AdditionalTypes.ReverteDamage.ByPercent)[1] / 100);
                        realAttacker.GetDamage(damage, defender, false);
                        defender.MapInstance.Broadcast(
                                StaticPacketHelper.SkillUsed(realAttacker.UserType, realAttacker.MapEntityId,
                                        (byte)realAttacker.UserType, realAttacker.MapEntityId,
                                        -1, 0, 0, 0, 0, 0,
                                        realAttacker.Hp > 0,
                                        (int)(realAttacker.Hp / realAttacker.HPLoad() * 100),
                                        damage, 0, 1));
                        defender.Character?.Session?.SendPacket(defender.Character.GenerateStat());
                    }
                }


                #region CritDamage to Heal

                int[] addCritDamageToHp = defender.GetBuff(CardType.AbsorptionAndPowerSkill, (byte)AdditionalTypes.AbsorptionAndPowerSkill.CritDamagetoHeal);

                if (addCritDamageToHp[0] > 0)
                {
                    var divider = defender?.MapInstance != null && defender.MapInstance.IsPVP ? 2.0D : 1.0D;
                    var healAmount = (int)((totalDamage / divider) * addCritDamageToHp[0] / 100D);
                    defender.Character?.MapInstance?.Broadcast(defender?.Character?.GenerateRc(
                    defender?.MapInstance != null && defender.MapInstance.IsPVP ? healAmount * 2 : healAmount));
                    defender.Hp += healAmount;
                }
            }

            #endregion

            if (hitMode == 4)
            {
                if (defender.HasBuff(CardType.GetInDodge, (byte)AdditionalTypes.GetInDodge.RecoveryHp))
                {
                    var data = attacker.GetBuff(CardType.GetInDodge, (byte)AdditionalTypes.GetInDodge.RecoveryHp)[0];
                    defender.Hp += data;
                    defender?.MapInstance?.Broadcast(StaticPacketHelper.GenerateEff(defender.UserType, defender.MapEntityId, 5));
                    defender?.Character?.Session?.CurrentMapInstance?.Broadcast(defender?.Character?.GenerateRc(150));
                }
            }

            return totalDamage;
        }

        private static int[] GetBuff(byte level, List<Buff> buffs, List<BCard> bcards, CardType type,
            byte subtype, BuffType btype, ref int count, bool castTypeNotZero = false)
        {
            int value1 = 0;
            int value2 = 0;
            int value3 = 0;

            IEnumerable<BCard> cards;

            if (bcards != null && btype.Equals(BuffType.Good))
            {
                cards = subtype % 10 == 1
                ? bcards.ToList().Where(s =>
                    (!castTypeNotZero || s.CastType != 0) && s.Type.Equals((byte)type) && s.SubType.Equals((byte)(subtype)) && s.FirstData >= 0)
                : bcards.ToList().Where(s =>
                    (!castTypeNotZero || s.CastType != 0) && s.Type.Equals((byte)type) && s.SubType.Equals((byte)(subtype)));

                foreach (BCard entry in cards.ToList())
                {
                    if (entry.IsLevelScaled)
                    {
                        if (entry.IsLevelDivided)
                        {
                            value1 += level / entry.FirstData;
                        }
                        else
                        {
                            value1 += entry.FirstData * level;
                        }
                    }
                    else
                    {
                        value1 += entry.FirstData;
                    }

                    value2 += entry.SecondData;
                    value3 += entry.ThirdData;
                    count++;
                }
            }

            if (buffs != null)
            {
                foreach (Buff buff in buffs.ToList().Where(b => b.Card.BuffType.Equals(btype)))
                {
                    cards = subtype % 10 == 1
                        ? buff.Card.BCards.Where(s =>
                            (!castTypeNotZero || s.CastType != 0) && s.Type.Equals((byte)type) && s.SubType.Equals((byte)(subtype))
                            && (s.CastType != 1 || (s.CastType == 1
                                                 && buff.Start.AddMilliseconds(buff.Card.Delay * 100) < DateTime.Now))
                            && s.FirstData >= 0).ToList()
                        : buff.Card.BCards.Where(s =>
                            (!castTypeNotZero || s.CastType != 0) && s.Type.Equals((byte)type) && s.SubType.Equals((byte)(subtype))
                            && (s.CastType != 1 || (s.CastType == 1
                                                 && buff.Start.AddMilliseconds(buff.Card.Delay * 100) < DateTime.Now))
                            && s.FirstData <= 0).ToList();

                    foreach (BCard entry in cards)
                    {
                        if (entry.IsLevelScaled)
                        {
                            if (entry.IsLevelDivided)
                            {
                                value1 += buff.Level / entry.FirstData;
                            }
                            else
                            {
                                value1 += entry.FirstData * buff.Level;
                            }
                        }
                        else
                        {
                            value1 += entry.FirstData;
                        }

                        value2 += entry.SecondData;
                        value3 += entry.ThirdData;
                        count++;
                    }
                }
            }

            return new[] { value1, value2, value3 };
        }

        private static int GetMonsterDamageBonus(byte level)
        {
            if (level < 45)
            {
                return 0;
            }
            else if (level < 55)
            {
                return level;
            }
            else if (level < 60)
            {
                return level * 2;
            }
            else if (level < 65)
            {
                return level * 3;
            }
            else if (level < 70)
            {
                return level * 4;
            }
            else
            {
                return level * 5;
            }
        }

        #endregion
        #endregion
    }
}