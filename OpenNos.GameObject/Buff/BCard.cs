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
using OpenNos.Data;
using OpenNos.Domain;
using OpenNos.GameObject.Extension.CharacterExtensions;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;
using OpenNos.GameObject.Npc;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Media.Media3D;
using static OpenNos.Domain.BCardType;

namespace OpenNos.GameObject
{
    public class BCard : BCardDTO
    {
        #region Instantiation

        public BCard()
        {
        }

        public BCard(BCardDTO input) : this()
        {
            BCardId = input.BCardId;
            CardId = input.CardId;
            CastType = input.CastType;
            FirstData = input.FirstData;
            IsLevelDivided = input.IsLevelDivided;
            IsLevelScaled = input.IsLevelScaled;
            ItemVNum = input.ItemVNum;
            NpcMonsterVNum = input.NpcMonsterVNum;
            SecondData = input.SecondData;
            SkillVNum = input.SkillVNum;
            SubType = input.SubType;
            ThirdData = input.ThirdData;
            Type = input.Type;
            MinLevel = input.MinLevel;
            MaxLevel = input.MaxLevel;
        }

        #endregion

        #region Properties

        public int ForceDelay { get; set; }

        public bool IsPartnerSkillBCard { get; set; }

        #endregion

        #region Methods

        public void ApplyBCards(BattleEntity session, BattleEntity sender, short x = 0, short y = 0,
            short partnerBuffLevel = 0)
        {
            //session.Event.EmitEvent(new BCardEvent
            //{
            //    Target = session,
            //    Sender = sender,
            //    Card = this
            //});
            //return;
            /*if (Type != (byte)CardType.Buff && (CardId != null || SkillVNum != null))
            {
                Console.WriteLine($"BCardId: {BCardId} Type: {(CardType)Type} SubType: {SubType} CardId: {CardId?.ToString() ?? "null"} ItemVNum: {ItemVNum?.ToString() ?? "null"} SkillVNum: {SkillVNum?.ToString() ?? "null"} SessionType: {session?.EntityType.ToString() ?? "null"} SenderType: {sender?.EntityType.ToString() ?? "null"}");
            }*/

            var firstData = FirstData;
            int senderLevel = sender.MapMonster?.Owner?.Level ?? sender.Level;

            Card card = null;
            Skill skill = null;
            var delayTime = 0;
            var duration = 0;

            if (CardId is short cardId2 && ServerManager.Instance.GetCardByCardId(cardId2) is Card BuffCard)
            {
                card = BuffCard;

                if (CastType == 1)
                {
                    delayTime = card.Delay * 100;
                }

                duration = card.Duration * 100 - delayTime;
            }

            if (SkillVNum is short skillVNum && ServerManager.GetSkill(skillVNum) is Skill Skill)
            {
                skill = Skill;
                if (sender.Character != null)
                {
                    var skills = sender.Character.GetSkills();

                    if (skills != null)
                    {
                        firstData = skills.Find(s => s.SkillVNum == skill.SkillVNum)?.GetSkillBCards()
                                        .OrderByDescending(s => s.SkillVNum)
                                        .FirstOrDefault(b => b.Type == Type && b.SubType == SubType).FirstData ??
                                    FirstData;

                        //firstData = skills.Where(s => s.SkillVNum == skill.SkillVNum).Sum(s => s.GetSkillBCards().Where(b => b.Type == Type && b.SubType == SubType).Sum(b => b.FirstData));
                        if (firstData == 0)
                        {
                            firstData = FirstData;
                        }
                    }
                }
            }

            if (ForceDelay > 0)
            {
                delayTime = ForceDelay * 100;
            }

            if (BCardId > 0)
            {
                session.BCardDisposables[skill?.SkillVNum == 1098 ? skill.SkillVNum * 1000 : BCardId]?.Dispose();
            }

            session.BCardDisposables[skill?.SkillVNum == 1098 ? skill.SkillVNum * 1000 : BCardId] = Observable
                .Timer(TimeSpan.FromMilliseconds(delayTime)).Subscribe(o =>
                {
                    switch ((CardType)Type)
                    {
                        case CardType.Buff:
                            {
                                var cardId = (short)(SecondData + partnerBuffLevel);

                                // Memorial should only be applied on 1st Mass Teleport activation

                                if (cardId == 620 && sender?.Character?.SavedLocation != null)
                                {
                                    return;
                                }

                                var buff = new Buff(cardId, senderLevel)
                                {
                                    SkillVNum = SkillVNum
                                };

                                var Chance = firstData == 0 ? ThirdData : firstData;
                                var CardsToProtect = new List<short>();
                                if (buff.Card.BuffType == BuffType.Bad &&
                                    session.GetBuff(CardType.DebuffResistance,
                                            (byte)AdditionalTypes.DebuffResistance.NeverBadEffectChance) is int[]
                                        NeverBadEffectChance)
                                {
                                    // I divide in NeverBadEffectChance[3] since we have to avoid the Level debuffs being added
                                    if (ServerManager.RandomNumber() < NeverBadEffectChance[1]
                                        && buff.Card.Level <= (NeverBadEffectChance[0]))
                                    {
                                        return;
                                    }
                                }

                                if (session.GetBuff(CardType.DebuffResistance,
                                        (byte)AdditionalTypes.DebuffResistance.NeverBadGeneralEffectChance) is int[]
                                    NeverBadGeneralEffectChance)
                                {
                                    if (ServerManager.RandomNumber() < NeverBadGeneralEffectChance[1]
                                        && buff.Card.Level <= NeverBadGeneralEffectChance[0]
                                        && buff.Card.BuffType == BuffType.Bad)
                                    {
                                        return;
                                    }
                                }

                                if (session.GetBuff(CardType.Buff,
                                        (byte)AdditionalTypes.Buff.PreventingBadEffect) is int[] PreventingBadEffect &&
                                    (PreventingBadEffect[1] > 0 || PreventingBadEffect[2] > 0))
                                {
                                    var Prob = 100 - PreventingBadEffect[1] * 10;
                                    var ProtectType = PreventingBadEffect[0];

                                    if (PreventingBadEffect[2] > 0)
                                    {
                                        Prob = PreventingBadEffect[2];
                                        ProtectType = PreventingBadEffect[1];
                                    }

                                    if (ServerManager.RandomNumber() < Prob && buff.Card.BuffType == BuffType.Bad)
                                    {
                                        switch (ProtectType)
                                        {
                                            case 0:

                                                //Bleedings
                                                CardsToProtect.Add(1);
                                                CardsToProtect.Add(21);
                                                CardsToProtect.Add(42);
                                                CardsToProtect.Add(82);
                                                CardsToProtect.Add(189);
                                                CardsToProtect.Add(190);
                                                CardsToProtect.Add(191);
                                                CardsToProtect.Add(192);
                                                break;

                                            case 4:

                                                //Blackouts
                                                CardsToProtect.Add(7);
                                                CardsToProtect.Add(66);
                                                CardsToProtect.Add(100);
                                                CardsToProtect.Add(195);
                                                CardsToProtect.Add(196);
                                                CardsToProtect.Add(197);
                                                CardsToProtect.Add(198);
                                                break;

                                            case 32:

                                                //Side-effects of resurrecting
                                                CardsToProtect.Add(44);
                                                break;

                                            case 85:
                                                CardsToProtect.Add(113);
                                                //Foggy Colossus' poison
                                                break;
                                        }
                                    }
                                }

                                if (buff.Card.BuffType == BuffType.Bad &&
                                    session.GetBuff(CardType.SpecialisationBuffResistance,
                                        (byte)AdditionalTypes.SpecialisationBuffResistance.ResistanceToEffect,
                                        buff.Card.CardId) is int[] ResistanceToEffect)
                                {
                                    if (ServerManager.RandomNumber() < ResistanceToEffect[0])
                                    {
                                        CardsToProtect.Add((short)ResistanceToEffect[1]);
                                    }
                                }

                                if (CardsToProtect.Contains(buff.Card.CardId))
                                {
                                    return;
                                }

                                if (SubType == (byte)AdditionalTypes.Buff.ChanceCausing)
                                {
                                    if (Chance > 0 && ServerManager.RandomNumber() < Chance)
                                    {
                                        if (SkillVNum != null && (buff.Card.CardId == 570 || buff.Card.CardId == 56))
                                        {
                                            sender.AddBuff(buff, sender, x: x, y: y, forced: true);
                                        }
                                        else if (buff.Card?.BuffType == BuffType.Bad
                                                 && session.HasBuff(CardType.TauntSkill,
                                                     (byte)AdditionalTypes.TauntSkill.ReflectBadEffect)
                                        //&& ServerManager.RandomNumber() < FirstData
                                        )
                                        {
                                            sender.AddBuff(buff, sender, x: x, y: y);
                                        }
                                        else
                                        {
                                            session.AddBuff(buff, sender, x: x, y: y);
                                        }
                                    }
                                    else if (Chance < 0 && ServerManager.RandomNumber() < -Chance)
                                    {
                                        session.RemoveBuff(cardId);
                                    }
                                }
                            }
                            break;

                        case CardType.Move:
                            ItemInstance hat = session?.Character?.Inventory?.LoadBySlotAndType((byte)EquipmentType.CostumeHat, InventoryType.Wear);
                            ItemInstance costume = session?.Character?.Inventory?.LoadBySlotAndType((byte)EquipmentType.CostumeSuit, InventoryType.Wear);
                            ItemInstance costumeWings = session?.Character?.Inventory?.LoadBySlotAndType((byte)EquipmentType.Wings, InventoryType.Wear);

                            if (session.Character != null)
                            {
                                session.Character.LastSpeedChange = DateTime.Now;
                                session.Character.LoadSpeed();
                                session.Character.Session?.SendPacket(session.Character.GenerateCond());
                            }

                            if (hat?.ItemVNum == 4411 && costume?.ItemVNum == 4409)
                            {
                                session.Character.Speed += 1;
                            }
                            break;

                        case CardType.OnAttackRestore:
                            if (ServerManager.RandomNumber() < firstData)
                            {
                                switch (SubType)
                                {
                                    case (byte)AdditionalTypes.OnAttackRestore.RestoreHP:
                                        {
                                            var amount = SecondData;
                                            sender.Hp += amount;

                                            if (sender.Hp > sender.Hp)
                                            {
                                                sender.Hp = sender.HpMax;
                                            }

                                            sender.Character.HPLoad();
                                            sender.Character?.Session?.SendPacket(sender.Character?.GenerateStat());
                                            sender.MapInstance?.Broadcast(session.GenerateRc(amount));
                                        }

                                        break;

                                    case (byte)AdditionalTypes.OnAttackRestore.RestoreMP:
                                        {
                                            var amount = SecondData;
                                            sender.Mp += amount;

                                            if (sender.Mp > sender.Mp)
                                            {
                                                sender.Mp = sender.Mp;
                                            }

                                            sender.Character.MPLoad();
                                            sender.Character?.Session?.SendPacket(sender.Character?.GenerateStat());
                                        }

                                        break;

                                    case (byte)AdditionalTypes.OnAttackRestore.RestoreHPPercentage:
                                        {
                                            var amount = (int)(session.Hp * SecondData / 100D);
                                            sender.Hp += amount;

                                            if (sender.Hp > sender.Hp)
                                            {
                                                sender.Hp = sender.HpMax;
                                            }

                                            sender.Character.HPLoad();
                                            sender.Character?.Session?.SendPacket(sender.Character?.GenerateStat());
                                            sender.MapInstance?.Broadcast(session.GenerateRc(amount));
                                        }
                                        
                                        break;

                                    case (byte)AdditionalTypes.OnAttackRestore.RestoreMPPercentage:
                                        {
                                            var amount = (int)(session.Mp * SecondData / 100D);
                                            sender.Mp += amount;

                                            if (sender.Mp > sender.Mp)
                                            {
                                                sender.Mp = sender.Mp;
                                            }

                                            sender.Character.MPLoad();
                                            sender.Character?.Session?.SendPacket(sender.Character?.GenerateStat());
                                        }
                                        
                                        break;
                                }
                            }
                            break;

                        case CardType.Summons:
                            if (sender.MapMonster?.MonsterVNum == 154)
                            {
                                return;
                            }

                            var move = SecondData != 1382;
                            var summonParameters = new List<MonsterToSummon>();
                            var amountToSpawn = firstData;
                            if (sender.Character != null) if (sender.Character.Morph == 31 && sender.Character.HasBuff(703)) amountToSpawn = firstData + 1;
                            var aliveTime = ServerManager.GetNpcMonster((short)SecondData).RespawnTime / (ServerManager.GetNpcMonster((short)SecondData).RespawnTime < 2400 ? ServerManager.GetNpcMonster((short)SecondData).RespawnTime < 150 ? 1 : 10 : 40) * (ServerManager.GetNpcMonster((short)SecondData).RespawnTime >= 150 ? 4 : 1);

                            for (var i = 0; i < amountToSpawn; i++)
                            {
                                x = (short)(ServerManager.RandomNumber(-1, 1) + sender.PositionX);
                                y = (short)(ServerManager.RandomNumber(-1, 1) + sender.PositionY);
                                if (skill != null && sender.Character == null)
                                {
                                    var randomCell =
                                        sender.MapInstance.Map.GetRandomPositionByDistance(sender.PositionX,
                                            sender.PositionY, skill.Range, true);
                                    if (randomCell != null)
                                    {
                                        x = randomCell.X;
                                        y = randomCell.Y;
                                    }
                                }

                                summonParameters.Add(new MonsterToSummon((short)SecondData, new MapCell { X = x, Y = y }, null, move, false, false, true, false, owner: sender, 10, 0, 15, 0, 0, 0));
                            }

                            if (ServerManager.RandomNumber() <= Math.Abs(ThirdData) || ThirdData == 0 || ThirdData < 0)
                            {
                                switch (SubType)
                                {
                                    case (byte)AdditionalTypes.Summons.Summons:
                                        if (session.MapInstance.MapInstanceType != MapInstanceType.ArenaInstance)
                                        {
                                            if (CardId == null && SkillVNum == null)
                                            {
                                                if (sender.MapMonster != null)
                                                {
                                                    sender.BCardDisposables[BCardId]?.Dispose();
                                                    IDisposable bcardDisposable = null;
                                                    bcardDisposable = Observable
                                                        .Interval(TimeSpan.FromSeconds(5))
                                                        .Subscribe(s =>
                                                        {
                                                            if (sender.BCardDisposables[BCardId] != bcardDisposable)
                                                            {
                                                                bcardDisposable.Dispose();
                                                                return;
                                                            }

                                                            summonParameters = new List<MonsterToSummon>();
                                                            for (var i = 0; i < amountToSpawn; i++)
                                                            {
                                                                x = (short)(ServerManager.RandomNumber(-1, 1) +
                                                                             sender.PositionX);
                                                                y = (short)(ServerManager.RandomNumber(-1, 1) +
                                                                             sender.PositionY);
                                                                summonParameters.Add(new MonsterToSummon((short)SecondData,
                                                                    new MapCell { X = x, Y = y }, null, move,
                                                                    aliveTime: aliveTime, owner: sender));
                                                            }

                                                            if (sender.MapMonster.Target != null && sender.MapInstance
                                                                .GetCharactersInRange(sender.PositionX,
                                                                    sender.PositionY, 5)
                                                                .Any(c => c.BattleEntity.MapEntityId ==
                                                                          sender.MapMonster.Target.MapEntityId))
                                                            {
                                                                EventHelper.Instance.RunEvent(
                                                                    new EventContainer(sender.MapInstance,
                                                                        EventActionType.SPAWNMONSTERS, summonParameters));
                                                            }
                                                        });
                                                    sender.BCardDisposables[BCardId] = bcardDisposable;
                                                }
                                            }
                                            else
                                            {
                                                EventHelper.Instance.RunEvent(new EventContainer(sender.MapInstance,
                                                    EventActionType.SPAWNMONSTERS, summonParameters));
                                            }
                                        }
                                        else
                                        {
                                            session.Character.Session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("CANT_USE_IN_ARENA"), 0));
                                            return;
                                        }

                                        break;

                                    case (byte)AdditionalTypes.Summons.SummonTrainingDummy:
                                        summonParameters = new List<MonsterToSummon>();
                                        for (var i = 0; i < amountToSpawn; i++)
                                        {
                                            x = (short)(ServerManager.RandomNumber(-1, 1) + sender.PositionX);
                                            y = (short)(ServerManager.RandomNumber(-1, 1) + sender.PositionY);
                                            summonParameters.Add(new MonsterToSummon((short)SecondData,
                                                new MapCell { X = x, Y = y }, null, move, aliveTimeMp: aliveTime,
                                                owner: sender));
                                        }

                                        EventHelper.Instance.RunEvent(new EventContainer(sender.MapInstance,
                                            EventActionType.SPAWNMONSTERS, summonParameters));
                                        break;

                                    default:
                                        if (!sender.OnDeathEvents.ToList().Any(s =>
#warning check this, s is null by some reason when loading worldserver, but just sometimes
                                            s != null &&
                                            s.EventActionType == EventActionType.SPAWNMONSTERS))
                                        {
                                            sender.OnDeathEvents.Add(new EventContainer(sender.MapInstance,
                                                EventActionType.SPAWNMONSTERS, summonParameters));
                                        }

                                        break;
                                }
                            }

                            break;

                        case CardType.SpecialAttack:
                            break;

                        case CardType.SpecialDefence:
                            break;

                        case CardType.AttackPower:
                            if (SubType == (byte)AdditionalTypes.AttackPower.AllAttacksIncreased)
                            {
                                if (session.Character != null && sender.Character != null &&
                                    session.Character == sender.Character)
                                {
                                    if (skill != null && skill.UpgradeSkill == 0)
                                    {
                                        var skills = session.Character.GetSkills();

                                        session.Character.ChargeValue = skills
                                            .Where(s => s.SkillVNum == skill.SkillVNum)
                                            .Sum(s => s.GetSkillBCards().Sum(b => b.FirstData));
                                        session.AddBuff(new Buff(0, session.Level), session);
                                    }
                                }
                            }

                            if (SubType == (byte)AdditionalTypes.AttackPower.MeleeAttacksIncreased)
                            {
                                if (session.Character != null && sender.Character != null &&
                                    session.Character == sender.Character)
                                {
                                    if (skill != null && skill.UpgradeSkill == 0)
                                    {
                                        var skills = session.Character.GetSkills();

                                        session.Character.ChargeValue = skills
                                            .Where(s => s.SkillVNum == skill.SkillVNum)
                                            .Sum(s => s.GetSkillBCards().Sum(b => b.FirstData));
                                        session.AddBuff(new Buff(0, session.Level), session);
                                    }
                                }
                            }

                            if (SubType == (byte)AdditionalTypes.AttackPower.RangedAttacksIncreased)
                            {
                                if (session.Character != null && sender.Character != null &&
                                    session.Character == sender.Character)
                                {
                                    if (skill != null && skill.UpgradeSkill == 0)
                                    {
                                        var skills = session.Character.GetSkills();

                                        session.Character.ChargeValue = skills
                                            .Where(s => s.SkillVNum == skill.SkillVNum)
                                            .Sum(s => s.GetSkillBCards().Sum(b => b.FirstData));
                                        session.AddBuff(new Buff(0, session.Level), session);
                                    }
                                }
                            }

                            if (SubType == (byte)AdditionalTypes.AttackPower.MagicalAttacksIncreased)
                            {
                                if (session.Character != null && sender.Character != null &&
                                    session.Character == sender.Character)
                                {
                                    if (skill != null && skill.UpgradeSkill == 0)
                                    {
                                        var skills = session.Character.GetSkills();

                                        session.Character.ChargeValue = skills
                                            .Where(s => s.SkillVNum == skill.SkillVNum)
                                            .Sum(s => s.GetSkillBCards().Sum(b => b.FirstData));
                                        session.AddBuff(new Buff(0, session.Level), session);
                                    }
                                }
                            }

                            break;

                        case CardType.Target:
                            break;

                        case CardType.Critical:
                            break;

                        case CardType.SpecialCritical:
                            break;

                        case CardType.Element:
                            break;

                        case CardType.IncreaseDamage:
                            break;

                        case CardType.Defence:
                            break;

                        case CardType.DodgeAndDefencePercent:
                            break;

                        case CardType.Block:
                            break;

                        case CardType.Absorption:
                            break;

                        case CardType.ElementResistance:
                            break;

                        case CardType.EnemyElementResistance:
                            break;

                        case CardType.EnemyElementResistancePercent:
                            break;

                        case CardType.Damage:
                            break;

                        case CardType.GuarantedDodgeRangedAttack:
                            break;

                        case CardType.Morale:
                            break;

                        case CardType.Casting:
                            break;

                        case CardType.Reflection:
                            if (SubType == (byte)AdditionalTypes.Reflection.EnemyMPDecreased)
                            {
                                if (ServerManager.RandomNumber() < firstData)
                                {
                                    session.DecreaseMp(session.Mp * SecondData / 100);
                                    if (session.Character != null)
                                    {
                                        session.Character.Session.SendPacket(session.Character.GenerateStat());
                                    }
                                }
                            }

                            break;

                        case CardType.DrainAndSteal:
                            if (SubType == (byte)AdditionalTypes.DrainAndSteal.ConvertEnemyHPToMP)
                            {
                                var bonus = 0;
                                if (IsLevelScaled)
                                {
                                    bonus = senderLevel * (firstData - 1);
                                }
                                else
                                {
                                    bonus = firstData;
                                }

                                bonus = session.GetDamage(bonus, sender, true, true);
                                if (bonus > 0)
                                {
                                    session.MapInstance?.Broadcast(session.GenerateDm(bonus));
                                    session.Character?.Session?.SendPacket(session.Character?.GenerateStat());
                                    if (sender.Mp + bonus > sender.MPLoad())
                                    {
                                        sender.Mp = (int)sender.MPLoad();
                                    }
                                    else
                                    {
                                        sender.Mp += bonus;
                                    }

                                    sender.Character?.Session?.SendPacket(sender.Character?.GenerateStat());
                                }
                            }
                            else if (SubType == (byte)AdditionalTypes.DrainAndSteal.LeechEnemyHP)
                            {
                                // FirstData = -1 SecondData = 0 SkillVNum = 400 (Tumble) IsLevelScaled
                                // = 1

                                if (SecondData == 0)
                                {
                                    break; // Wtf !? !? !!!
                                }

                                if (ServerManager.RandomNumber() < FirstData)
                                {
                                    if (session.Hp > 1
                                        && session.MapInstance != null)
                                    {
                                        var amount = SecondData;

                                        if (amount >= session.Hp)
                                        {
                                            amount = session.Hp - 1;
                                        }

                                        if (sender.Hp + amount > sender.HpMax)
                                        {
                                            amount = sender.HpMax - sender.Hp;
                                        }

                                        session.Hp -= amount;
                                        sender.Hp += amount;

                                        sender.MapInstance?.Broadcast(sender.GenerateRc(amount));
                                        sender.Character?.Session?.SendPacket(sender.Character?.GenerateStat());
                                        session.Character?.Session?.SendPacket(session.Character?.GenerateStat());
                                    }
                                }
                            }
                            else if (SubType == (byte)AdditionalTypes.DrainAndSteal.LeechEnemyMP)
                            {
                                // FirstData = -100 SecondData = 3 CardId = 228 (MAna Drain) ThirdData = 1

                                if (ThirdData != 0)
                                {
#warning TODO: Mana Drain
                                    break;
                                }

                                if (ServerManager.RandomNumber() < FirstData)
                                {
                                    if (session.Mp > 1
                                        && session.MapInstance != null)
                                    {
                                        var amount = SecondData;

                                        if (amount >= session.Mp)
                                        {
                                            amount = session.Mp - 1;
                                        }

                                        session.Mp -= amount;
                                        sender.Mp += amount;

                                        if (sender.Mp > sender.MpMax)
                                        {
                                            sender.Mp = sender.MpMax;
                                        }

                                        sender.Character?.Session?.SendPacket(sender.Character?.GenerateStat());
                                        session.Character?.Session?.SendPacket(session.Character?.GenerateStat());
                                    }
                                }
                            }
                            break;

                        case CardType.Recover:
                            if (SubType == (byte)AdditionalTypes.Recover.RecoverHP)
                            {
                                if (ServerManager.RandomNumber() < FirstData)
                                {
                                    var amount = SecondData;

                                    sender.Hp += amount;

                                    if (sender.Hp > sender.Hp)
                                    {
                                        sender.Hp = sender.HpMax;
                                    }

                                    sender.Character.HPLoad();
                                    sender.Character?.Session?.SendPacket(sender.Character?.GenerateStat());
                                    sender.MapInstance?.Broadcast(session.GenerateRc(amount));
                                }
                            }
                            else if (SubType == (byte)AdditionalTypes.Recover.RecoverMP)
                            {
                                if (ServerManager.RandomNumber() < FirstData)
                                {
                                    var amount = SecondData;

                                    sender.Mp += amount;

                                    if (sender.Mp > sender.MpMax)
                                    {
                                        sender.Mp = sender.MpMax;
                                    }

                                    sender.Character.MPLoad();
                                    sender.Character?.Session?.SendPacket(sender.Character?.GenerateStat());
                                }
                            }
                            else if (SubType == (byte)AdditionalTypes.Recover.RecoverHPPercent)
                            {
                                if (ServerManager.RandomNumber() < FirstData)
                                {
                                    var amount = SecondData;

                                    sender.Hp *= amount / 100;

                                    if (sender.Hp > sender.Hp)
                                    {
                                        sender.Hp = sender.HpMax;
                                    }

                                    sender.Character.HPLoad();
                                    sender.Character?.Session?.SendPacket(sender.Character?.GenerateStat());
                                    sender.MapInstance?.Broadcast(session.GenerateRc(amount));
                                }
                            }
                            else if (SubType == (byte)AdditionalTypes.Recover.RecoverMPPercent)
                            {
                                if (ServerManager.RandomNumber() < FirstData)
                                {
                                    var amount = SecondData;

                                    sender.Mp *= amount / 100;

                                    if (sender.Mp > sender.MpMax)
                                    {
                                        sender.Mp = sender.MpMax;
                                    }

                                    sender.Character.MPLoad();
                                    sender.Character?.Session?.SendPacket(sender.Character?.GenerateStat());
                                }
                            }
                            break;

                        case CardType.HealingBurningAndCasting:
                            {
                                var amount = 0;

                                void HealingBurningAndCastingAction()
                                {
                                    if (session.Hp < 1
                                        || session.MapInstance == null)
                                    {
                                        return;
                                    }

                                    if (IsLevelDivided)
                                    {
                                        amount = senderLevel / (firstData + 1);
                                    }
                                    else if (IsLevelDivided && IsLevelScaled)
                                    {
                                        amount = senderLevel / (firstData + 1);
                                    }
                                    else if (IsLevelScaled)
                                    {
                                        amount = senderLevel * (firstData + 1);
                                    }
                                    else
                                    {
                                        amount = firstData;
                                    }

                                    switch (SubType)
                                    {
                                        case (byte)AdditionalTypes.HealingBurningAndCasting.RestoreHP:

                                            if (session.Hp + amount > session.HpMax)
                                            {
                                                amount = session.HpMax - session.Hp;
                                            }

                                            if (amount > 0)
                                            {
                                                if (session.HasBuff(CardType.DarkCloneSummon, (byte)AdditionalTypes.DarkCloneSummon.ConvertRecoveryToDamage))
                                                {
                                                    amount = session.GetDamage(amount, sender, true, true);

                                                    session.MapInstance.Broadcast(session.GenerateDm(amount));
                                                }
                                                else
                                                {
                                                    session.Hp += amount;

                                                    session.MapInstance.Broadcast(session.GenerateRc(amount));
                                                }
                                                
                                            }
                                            else if (amount < 0)
                                            {
                                                amount = session.GetDamage(amount, sender, true, true);

                                                session.MapInstance.Broadcast(session.GenerateDm(amount));
                                            }

                                            break;

                                        case (byte)AdditionalTypes.HealingBurningAndCasting.RestoreMP:

                                            if (session.Mp + amount > session.MpMax)
                                            {
                                                amount = session.MpMax - session.Mp;
                                            }

                                            session.Mp += amount;

                                            break;

                                        case (byte)AdditionalTypes.HealingBurningAndCasting.DecreaseHP:

                                            if (IsLevelScaled)
                                            {
                                                amount = senderLevel * (firstData - 1);
                                            }
                                            else
                                            {
                                                amount = firstData;
                                            }

                                            amount *= -1;

                                            if (session.Hp - amount < 1)
                                            {
                                                amount = session.Hp - 1;
                                            }

                                            if (amount > 0)
                                            {
                                                amount = session.GetDamage(amount, sender, true, true);

                                                session.MapInstance.Broadcast(session.GenerateDm(amount));
                                            }

                                            break;

                                        case (byte)AdditionalTypes.HealingBurningAndCasting.DecreaseMP:

                                            session.Mp = session.Mp - (amount * -1) <= 0 ? 1 : session.Mp - (amount * -1);

                                            break;
                                    }

                                    session?.Character?.Session?.SendPacket(session.Character?.GenerateStat());
                                }

                                HealingBurningAndCastingAction();

                                var interval = ThirdData > 0 ? ThirdData * 2 : CastType * 2;

                                if (CardId != null && interval > 0)
                                {
                                    IDisposable bcardDisposable = null;
                                    bcardDisposable = Observable.Interval(TimeSpan.FromSeconds(interval))
                                        .Subscribe(s =>
                                        {
                                            if (session.BCardDisposables[BCardId] != bcardDisposable)
                                            {
                                                bcardDisposable.Dispose();
                                                return;
                                            }

                                            if (session != null)
                                            {
                                                HealingBurningAndCastingAction();
                                            }
                                        });
                                    session.BCardDisposables[BCardId] = bcardDisposable;
                                }
                            }
                            break;

                        case CardType.HPMP:
                            if (SubType == (byte)AdditionalTypes.HPMP.DecreaseRemainingMP)
                            {
                                var bonus = (int)(session.Mp * firstData / 100D);
                                var change = false;
                                if (session.Mp - bonus > 1)
                                {
                                    session.DecreaseMp(bonus);
                                    change = true;
                                }
                                else
                                {
                                    if (session.Mp != 1)
                                    {
                                        bonus = session.Mp - 1;
                                        session.Mp = 1;
                                        change = true;
                                    }
                                }

                                if (change)
                                {
                                    session.Character?.Session?.SendPacket(session.Character?.GenerateStat());
                                }
                            }
                            else
                            {
                                var bonus = 0;
                                var change = false;
                                if (IsLevelScaled)
                                {
                                    bonus = senderLevel * (firstData += 1);
                                }
                                else if (IsLevelDivided)
                                {
                                    bonus = senderLevel / (firstData += 1);
                                }
                                else
                                {
                                    bonus = firstData;
                                }

                                void HPMPAction()
                                {
                                    if (session.Hp > 0)
                                    {
                                        switch (SubType)
                                        {
                                            case (byte)AdditionalTypes.HPMP.HPRestored:

                                                if (session.Hp + bonus <= session.HPLoad())
                                                {
                                                    session.Hp += bonus;
                                                    change = true;
                                                }
                                                else
                                                {
                                                    bonus = (int)session.HPLoad() - session.Hp;
                                                    session.Hp = (int)session.HPLoad();
                                                    change = true;
                                                }

                                                if (change)
                                                {
                                                    session.MapInstance?.Broadcast(session.GenerateRc(bonus));
                                                    session.Character?.Session?.SendPacket(session.Character
                                                        ?.GenerateStat());
                                                }

                                                break;

                                            case (byte)AdditionalTypes.HPMP.HPReduced:

                                                if (session.Hp - bonus > 1)
                                                {
                                                    session.Hp -= bonus * -1;
                                                    change = true;
                                                }
                                                else
                                                {
                                                    if (session.Hp != 1)
                                                    {
                                                        bonus = session.Hp - 1;
                                                        session.Hp = 1;
                                                        change = true;
                                                    }
                                                }

                                                if (change)
                                                {
                                                    session.MapInstance?.Broadcast(session.GenerateDm(bonus));
                                                    session.Character?.Session?.SendPacket(session.Character
                                                        ?.GenerateStat());
                                                }

                                                break;

                                            case (byte)AdditionalTypes.HPMP.MPRestored:
                                                if (session.Mp + bonus <= session.MPLoad())
                                                {
                                                    session.Mp += bonus;
                                                    change = true;
                                                }
                                                else
                                                {
                                                    bonus = (int)session.MPLoad() - session.Mp;
                                                    session.Mp = (int)session.MPLoad();
                                                    change = true;
                                                }

                                                if (change)
                                                {
                                                    session.Character?.Session?.SendPacket(session.Character
                                                        ?.GenerateStat());
                                                }

                                                break;

                                            case (byte)AdditionalTypes.HPMP.MPReduced:
                                                if (session.Mp - bonus > 1)
                                                {
                                                    session.Mp -= bonus * -1;
                                                    change = true;
                                                }
                                                else
                                                {
                                                    if (session.Mp != 1)
                                                    {
                                                        bonus = session.Mp - 1;
                                                        session.Mp = 1;
                                                        change = true;
                                                    }
                                                }

                                                if (change)
                                                {
                                                    session.Character?.Session?.SendPacket(session.Character?.GenerateStat());
                                                }

                                                break;
                                        }
                                    }
                                }

                                HPMPAction();
                                if (ThirdData > 0)
                                {
                                    IDisposable bcardDisposable = null;
                                    bcardDisposable = Observable
                                        .Interval(TimeSpan.FromSeconds(ThirdData * 2))
                                        .Subscribe(s =>
                                        {
                                            if (session.BCardDisposables[BCardId] != bcardDisposable)
                                            {
                                                bcardDisposable.Dispose();
                                                return;
                                            }

                                            if (session != null)
                                            {
                                                HPMPAction();
                                            }
                                        });
                                    session.BCardDisposables[BCardId] = bcardDisposable;
                                }
                            }

                            break;

                        case CardType.SpecialisationBuffResistance:

                            if (session == null) return; // had to do this, still i don't know the cause

                            if (SubType.Equals((byte)AdditionalTypes.SpecialisationBuffResistance.RemoveBadEffects))
                            {
                                // bad
                                if (ServerManager.RandomNumber() <= FirstData /* && sender.BCardDisposables[BCardId] == null*/)
                                {
                                    session.DisableBuffs(BuffType.Bad, SecondData);
                                }

                                if (session == sender && skill?.SkillVNum == 1098)
                                {
                                    if (session.BCardDisposables[BCardId] != null)
                                    {
                                        session.BCardDisposables[BCardId].Dispose();
                                        session.BCardDisposables[BCardId] = null;
                                        return;
                                    }

                                    int count = 0, count2 = 0;
                                    IDisposable fiveSecs = null;
                                    fiveSecs = Observable.Interval(TimeSpan.FromSeconds(2.5)).Subscribe(s =>
                                    {
                                        count++;
                                        RemoveBadEffectsAction(true);
                                        if (count == 2)
                                        {
                                            fiveSecs.Dispose();
                                        }
                                    });
                                    IDisposable bcardDisposable = null;
                                    bcardDisposable = Observable.Interval(TimeSpan.FromSeconds(2.5)).Subscribe(s =>
                                    {
                                        if (session.BCardDisposables[BCardId] != bcardDisposable)
                                        {
                                            bcardDisposable.Dispose();
                                            return;
                                        }

                                        if (!session.Character.Session.HasCurrentMapInstance)
                                        {
                                            session.BCardDisposables[BCardId].Dispose();
                                            session.BCardDisposables[BCardId] = null;
                                            return;
                                        }

                                        count2++;
                                        if (count2 > 2)
                                        {
                                            var health = false;
                                            if (session.Mp >= 144)
                                            {
                                                health = true;
                                                session.DecreaseMp(144);
                                            }

                                            RemoveBadEffectsAction(health);
                                        }
                                    });
                                    session.BCardDisposables[BCardId] = bcardDisposable;

                                    void RemoveBadEffectsAction(bool health)
                                    {
                                        session.MapInstance
                                            .GetBattleEntitiesInRange(
                                                new MapCell { X = session.PositionX, Y = session.PositionY },
                                                skill.TargetRange)
                                            .Where(e => e.MapMonster == null && e.Hp > 0 && !session.CanAttackEntity(e))
                                            .ToList().ForEach(b =>
                                            {
                                                if (ServerManager.RandomNumber() < 25)
                                                {
                                                    b.DisableBuffs(BuffType.Bad, SecondData + 1);
                                                }

                                                if (health)
                                                {
                                                    var healthAmount = senderLevel * 2;
                                                    if (b.Hp + healthAmount > b.HpMax)
                                                    {
                                                        healthAmount = b.HpMax - b.Hp;
                                                    }

                                                    if (healthAmount > 0)
                                                    {
                                                        b.Hp += healthAmount;
                                                        b.MapInstance.Broadcast(b.GenerateRc(healthAmount));
                                                        b.Character?.Session.SendPacket(b.Character.GenerateStat());
                                                    }
                                                }

                                                b.MapInstance.Broadcast(
                                                    StaticPacketHelper.GenerateEff(b.UserType, b.MapEntityId, 4354));
                                            });
                                    }
                                }
                            }

                            if (SubType.Equals((byte)AdditionalTypes.SpecialisationBuffResistance.RemoveGoodEffects))
                            {
                                if (ServerManager.RandomNumber() < FirstData)
                                {
                                    session.DisableBuffs(BuffType.Good, SecondData);
                                }
                            }

                            break;

                        case CardType.SpecialEffects:
                            if (SubType.Equals((byte)AdditionalTypes.SpecialEffects.ShadowAppears))
                            {
                                session.MapInstance.Broadcast(
                                    $"guri 0 {(short)session.UserType} {session.MapEntityId} {firstData} {SecondData}");
                            }

                            break;

                        case CardType.Capture:
                            if (sender.Character?.Session is ClientSession senderSession)
                            {
                                if (session.MapMonster is MapMonster mapMonster)
                                {
                                    if (ServerManager.GetNpcMonster(mapMonster.MonsterVNum) is NpcMonster mateNpc)
                                    {
                                        if (mapMonster.Monster.Catch &&
                                            (senderSession.Character.MapInstance.MapInstanceType ==
                                             MapInstanceType.BaseMapInstance ||
                                             senderSession.Character.MapInstance.MapInstanceType ==
                                             MapInstanceType.TimeSpaceInstance))
                                        {
                                            if (mapMonster.Monster.Level < senderSession.Character.Level)
                                            {
                                                if (mapMonster.CurrentHp < mapMonster.Monster.MaxHP / 2)
                                                {
                                                    // Algo
                                                    var capturerate =
                                                        100 - (int)((mapMonster.CurrentHp / mapMonster.Monster.MaxHP +
                                                                      1) * 100 / 3);
                                                    if (ServerManager.RandomNumber() <= capturerate)
                                                    {
                                                        if (senderSession.Character.Quests.Any(q =>
                                                            q.Quest.QuestType == (int)QuestType.Capture1 &&
                                                            q.Quest.QuestObjectives.Any(d =>
                                                                d.Data == mapMonster.MonsterVNum &&
                                                                q.GetObjectives()[d.ObjectiveIndex - 1] <
                                                                q.GetObjectiveByIndex(d.ObjectiveIndex).Objective)))
                                                        {
                                                            senderSession.Character.IncrementQuests(QuestType.Capture1,
                                                                mapMonster.MonsterVNum);
                                                            mapMonster.SetDeathStatement();
                                                            senderSession.CurrentMapInstance?.Broadcast(
                                                                StaticPacketHelper.Out(UserType.Monster,
                                                                    mapMonster.MapMonsterId));
                                                        }
                                                        else
                                                        {
                                                            senderSession.Character.IncrementQuests(QuestType.Capture2,
                                                                mapMonster.MonsterVNum);
                                                            var mate = new Mate(senderSession.Character, mateNpc,
                                                                mapMonster.Monster.Level, MateType.Pet);
                                                            if (senderSession.Character.CanAddMate(mate))
                                                            {
                                                                mate.RefreshStats();
                                                                senderSession.Character.AddPetWithSkill(mate);
                                                                senderSession.SendPacket(
                                                                    UserInterfaceHelper.GenerateMsg(
                                                                        Language.Instance.GetMessageFromKey(
                                                                            "CATCH_SUCCESS"), 0));
                                                                senderSession.CurrentMapInstance?.Broadcast(
                                                                    StaticPacketHelper.GenerateEff(UserType.Player,
                                                                        senderSession.Character.CharacterId, 197));
                                                                senderSession.CurrentMapInstance?.Broadcast(
                                                                    StaticPacketHelper.SkillUsed(UserType.Player,
                                                                        senderSession.Character.CharacterId, 3,
                                                                        mapMonster.MapMonsterId, -1, 0, 15, -1, -1, -1,
                                                                        true,
                                                                        (int)((float)mapMonster.CurrentHp /
                                                                            (float)mapMonster.MaxHp * 100), 0, -1,
                                                                        0));
                                                                mapMonster.SetDeathStatement();
                                                                senderSession.CurrentMapInstance?.Broadcast(
                                                                    StaticPacketHelper.Out(UserType.Monster,
                                                                        mapMonster.MapMonsterId));
                                                            }
                                                            else
                                                            {
                                                                senderSession.SendPacket(
                                                                    senderSession.Character.GenerateSay(
                                                                        Language.Instance.GetMessageFromKey(
                                                                            "PET_SLOT_FULL"), 10));
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        senderSession.SendPacket(
                                                            UserInterfaceHelper.GenerateMsg(
                                                                Language.Instance.GetMessageFromKey("CATCH_FAIL"), 0));
                                                    }
                                                }
                                                else
                                                {
                                                    senderSession.SendPacket(
                                                        UserInterfaceHelper.GenerateMsg(
                                                            Language.Instance.GetMessageFromKey("CURRENT_HP_TOO_HIGH"),
                                                            0));
                                                }
                                            }
                                            else
                                            {
                                                senderSession.SendPacket(UserInterfaceHelper.GenerateMsg(
                                                    Language.Instance.GetMessageFromKey("LEVEL_LOWER_THAN_MONSTER"),
                                                    0));
                                            }
                                        }
                                        else
                                        {
                                            senderSession.SendPacket(UserInterfaceHelper.GenerateMsg(
                                                Language.Instance.GetMessageFromKey("MONSTER_CANT_BE_CAPTURED"), 0));
                                        }
                                    }
                                }

                                //senderSession.CurrentMapInstance?.Broadcast(StaticPacketHelper.SkillUsed(UserType.Player, senderSession.Character.CharacterId, 3, session.MapEntityId, -1, 0, 15, -1, -1, -1, true, (int)((float)session.Hp / (float)session.HpMax * 100), 0, -1, 0));
                                senderSession.SendPacket(StaticPacketHelper.Cancel(2, session.MapEntityId));
                            }

                            break;

                        case CardType.SpecialDamageAndExplosions:
                            break;

                        case CardType.SpecialEffects2:
                            if (SubType.Equals((byte)AdditionalTypes.SpecialEffects2.TeleportInRadius))
                            {
                                if (session.Character != null && session.MapEntityId == sender.MapEntityId)
                                {
                                    session.Character.TeleportToDir(session.Character.Direction, firstData);
                                }
                            }

                            break;

                        case CardType.CalculatingLevel:
                            break;

                        case CardType.Recovery:
                            break;

                        case CardType.MaxHPMP:
                            {
                                if (session.Character != null)
                                {
                                    if (SubType == (byte)AdditionalTypes.MaxHPMP.IncreasesMaximumHP)
                                    {
                                        session.Character.HPLoad();
                                        session.Character.Session?.SendPacket(session.Character.GenerateStat());
                                    }
                                    else if (SubType == (byte)AdditionalTypes.MaxHPMP.IncreasesMaximumMP)
                                    {
                                        session.Character.MPLoad();
                                        session.Character.Session?.SendPacket(session.Character.GenerateStat());
                                    }
                                }
                            }
                            break;

                        case CardType.MultAttack:
                            break;

                        case CardType.MultDefence:
                            break;

                        case CardType.TimeCircleSkills:
                            break;

                        case CardType.RecoveryAndDamagePercent:
                            {
                                /* if (session.HasBuff(CardType.RecoveryAndDamagePercent, 01))
                                 {
                                     return;
                                 }*/

                                var bonus = 0;
                                var change = false;

                                void RecoveryAndDamagePercentAction()
                                {
                                    if (session.Hp > 0)
                                    {
                                        if (IsLevelDivided)
                                        {
                                            bonus = (int)(senderLevel / firstData * (session.HPLoad() / 100));
                                        }
                                        else
                                        {
                                            bonus = (int)(firstData * (session.HPLoad() / 100));
                                        }

                                        switch (SubType)
                                        {
                                            case (byte)AdditionalTypes.RecoveryAndDamagePercent.HPRecovered:

                                                if (session.Hp >= session.HPLoad()) return;

                                                if (session.Hp + bonus < session.HPLoad())
                                                {
                                                    session.Hp += bonus;
                                                    change = true;
                                                }
                                                else if (session.Hp + bonus >= (int)session.HPLoad())
                                                {
                                                    bonus = (int)session.HPLoad() - session.Hp;
                                                    session.Hp = (int)session.HPLoad();
                                                    change = true;
                                                }

                                                if (change)
                                                {
                                                    session.MapInstance?.Broadcast(session.GenerateRc(bonus));
                                                    session.Character?.Session?.SendPacket(
                                                        session.Character?.GenerateStat());
                                                }

                                                break;

                                            case (byte)AdditionalTypes.RecoveryAndDamagePercent.HPReduced:
                                                bonus = session.GetDamage(bonus * -1, sender, true, true);
                                                if (bonus > 0)
                                                {
                                                    session.MapInstance?.Broadcast(session.GenerateDm(bonus));
                                                    session.Character?.Session?.SendPacket(session.Character?.GenerateStat());
                                                }

                                                break;

                                            case (byte)AdditionalTypes.RecoveryAndDamagePercent.MPRecovered:
                                                if (session.Mp + bonus < session.MPLoad())
                                                {
                                                    session.Mp += bonus;
                                                    change = true;
                                                }
                                                else
                                                {
                                                    if (session.Mp != (int)session.MPLoad())
                                                    {
                                                        bonus = (int)session.MPLoad() - session.Mp;
                                                        session.Mp = (int)session.MPLoad();
                                                        change = true;
                                                    }
                                                }

                                                if (change)
                                                {
                                                    session.Character?.Session?.SendPacket(
                                                        session.Character?.GenerateStat());
                                                }

                                                break;

                                            case (byte)AdditionalTypes.RecoveryAndDamagePercent.MPReduced:
                                                if (session.Mp - bonus > 1)
                                                {
                                                    session.DecreaseMp(bonus);
                                                    change = true;
                                                }
                                                else
                                                {
                                                    if (session.Mp != 1)
                                                    {
                                                        bonus = session.Mp - 1;
                                                        session.Mp = 1;
                                                        change = true;
                                                    }
                                                }

                                                if (change)
                                                {
                                                    session.Character?.Session?.SendPacket(
                                                        session.Character?.GenerateStat());
                                                }

                                                break;
                                        }
                                    }
                                }

                                if (ThirdData > 0 && CastType == 0)
                                {
                                    RecoveryAndDamagePercentAction();
                                    IDisposable bcardDisposable = null;
                                    bcardDisposable = Observable
                                        .Interval(TimeSpan.FromSeconds(ThirdData * 2))
                                        .Subscribe(s =>
                                        {
                                            if (session.BCardDisposables[BCardId] != bcardDisposable)
                                            {
                                                bcardDisposable.Dispose();
                                                return;
                                            }

                                            if (session != null &&
                                                ((session.Character != null && !session.Character.IsDisposed)
                                                 || (session.Mate != null)
                                                 || (session.MapMonster != null)
                                                 || (session.MapNpc != null)))
                                            {
                                                RecoveryAndDamagePercentAction();
                                            }
                                            else
                                            {
                                                bcardDisposable.Dispose();
                                            }
                                        });
                                    session.BCardDisposables[BCardId] = bcardDisposable;
                                }
                            }
                            break;

                        case CardType.Count:
                            break;

                        case CardType.NoDefeatAndNoDamage:
                            break;

                        case CardType.SpecialActions:
                            if (SubType.Equals((byte)AdditionalTypes.SpecialActions.PushBack))
                            {
                                if (!ServerManager.RandomProbabilityCheck(session.ResistForcedMovement))
                                {
                                    PushBackSession(firstData, session, sender);
                                }
                            }
                            else if (SubType.Equals((byte)AdditionalTypes.SpecialActions.Hide))
                            {
                                if (session.Character is Character charact)
                                {
                                    if (charact.MapInstance.MapInstanceType != MapInstanceType.NormalInstance ||
                                        charact.MapInstance.Map.MapId != 2004)
                                    {
                                        charact.Invisible = true;
                                        charact.Mates.Where(s => s.IsTeamMember).ToList().ForEach(s =>
                                            charact.Session.CurrentMapInstance?.Broadcast(s.GenerateOut()));
                                        charact.Session.CurrentMapInstance?.Broadcast(charact.GenerateInvisible());
                                    }
                                    else if (card != null)
                                    {
                                        charact.RemoveBuff(card.CardId);
                                    }
                                }
                            }
                            else if (SubType.Equals((byte)AdditionalTypes.SpecialActions.FocusEnemies))
                            {
                                if (!ServerManager.RandomProbabilityCheck(session.ResistForcedMovement))
                                {
                                    if ((session.MapMonster == null ||
                                         !session.MapMonster.IsBoss &&
                                         !ServerManager.Instance.BossVNums.Contains(session.MapMonster.MonsterVNum) ||
                                         session.MapMonster.Owner?.Character != null ||
                                         session.MapMonster.Owner?.Mate != null)
                                        && (session.MapNpc == null ||
                                            !ServerManager.Instance.BossVNums.Contains(session.MapNpc.NpcVNum)))
                                    {
                                        Observable.Timer(TimeSpan.FromMilliseconds(skill.CastTime * 100)).Subscribe(s =>
                                        {
                                            if (!session.MapInstance.Map.IsBlockedZone(session.PositionX,
                                                session.PositionY, sender.PositionX, sender.PositionY))
                                            {
                                                session.PositionX = sender.PositionX;
                                                session.PositionY = sender.PositionY;
                                                session.MapInstance.Broadcast(
                                                    $"guri 3 {(short)session.UserType} {session.MapEntityId} {session.PositionX} {session.PositionY} 3 {SecondData} 2 -1");
                                            }
                                        });
                                    }
                                }
                            }
                            else if (SubType.Equals((byte)AdditionalTypes.SpecialActions.RunAway))
                            {
                                if (session.MapMonster != null && session.MapMonster.IsMoving &&
                                    !session.MapMonster.IsBoss &&
                                    !ServerManager.Instance.BossVNums.Contains(session.MapMonster.MonsterVNum) &&
                                    session.MapMonster.Owner?.Character == null &&
                                    session.MapMonster.Owner?.Mate == null)
                                {
                                    if (session.MapMonster.Target != null)
                                    {
                                        (session.MapMonster.Path ?? (session.MapMonster.Path = new()))
                                            .Clear();
                                        session.MapMonster.Target = null;

                                        var RunToX = session.MapMonster.FirstX;
                                        var RunToY = session.MapMonster.FirstY;

                                        var RunToPos =
                                            session.MapInstance.Map.GetRandomPositionByDistance(session.PositionX,
                                                session.PositionY, 20);
                                        if (RunToPos != null)
                                        {
                                            RunToX = RunToPos.X;
                                            RunToY = RunToPos.Y;
                                        }

                                        /*session.MapMonster.Path = BestFirstSearch.FindPathJagged(new Node { X = session.MapMonster.MapX, Y = session.MapMonster.MapY }, new Node { X = RunToX, Y = RunToY },
                                            session.MapMonster.MapInstance.Map.JaggedGrid);*/
                                        session.MapMonster.RunToX = RunToX;
                                        session.MapMonster.RunToY = RunToY;
                                        Observable.Timer(
                                                TimeSpan.FromMilliseconds(card != null ? card.Duration * 100 : 10000))
                                            .Subscribe(s =>
                                            {
                                                session.MapMonster.RunToX = 0;
                                                session.MapMonster.RunToY = 0;
                                            });
                                    }
                                }
                            }

                            break;

                        case CardType.Mode:
                            break;

                        case CardType.NoCharacteristicValue:
                            break;

                        case CardType.LightAndShadow:
                            if (SubType == (byte)AdditionalTypes.LightAndShadow.RemoveBadEffects)
                            {
                                session.DisableBuffs(new List<BuffType> { BuffType.Bad }, firstData);
                            }

                            break;

                        case CardType.Item:
                            break;

                        case CardType.UpdateChampionExp:
                            break;

                        case CardType.DebuffResistance:
                            break;


                        case CardType.SpecialBehaviour:
                            if (SubType == (byte)AdditionalTypes.SpecialBehaviour.TeleportRandom)
                            {
                                if (sender.Character != null)
                                {
                                    var randomCell = session.MapInstance.Map.GetRandomPosition();
                                    session.PositionX = randomCell.X;
                                    session.PositionY = randomCell.Y;
                                    session.MapInstance.Broadcast(session.GenerateTp());
                                }
                                else
                                {
                                    MapCell randomCell = null;

                                    if (SecondData > 0)
                                    {
                                        randomCell =
                                            sender.MapInstance.Map.GetRandomPositionByDistance(sender.PositionX,
                                                sender.PositionY, (short)SecondData, true);
                                    }
                                    else
                                    {
                                        randomCell = sender.MapInstance.Map.GetRandomPosition();
                                    }

                                    sender.PositionX = randomCell.X;
                                    sender.PositionY = randomCell.Y;
                                    sender.MapInstance.Broadcast(sender.GenerateTp());
                                }
                            }
                            else if (SubType == (byte)AdditionalTypes.SpecialBehaviour.InflictOnTeam)
                            {
                                if (CardId != null && CardId.Value == SecondData) // Checked on official server
                                {
                                    return;
                                }

                                void InflictOnTeamAction()
                                {
                                    if (session.Hp < 1
                                        || session.MapInstance == null)
                                    {
                                        return;
                                    }

                                    session.MapInstance
                                        .GetBattleEntitiesInRange(
                                            new MapCell { X = session.PositionX, Y = session.PositionY },
                                            (byte)FirstData)
                                        .Where(s => s.EntityType != EntityType.Npc &&
                                                    (s.EntityType != session.EntityType ||
                                                     s.MapEntityId != session.MapEntityId) &&
                                                    !session.CanAttackEntity(s)).ToList()
                                        .ForEach(s => s.AddBuff(new Buff((short)SecondData, senderLevel), sender));
                                }

                                if (ThirdData > 0)
                                {
                                    IDisposable bcardDisposable = null;
                                    bcardDisposable = Observable.Interval(TimeSpan.FromSeconds(ThirdData * 2))
                                        .Subscribe(s =>
                                        {
                                            if (session.BCardDisposables[BCardId] != bcardDisposable)
                                            {
                                                bcardDisposable.Dispose();
                                                return;
                                            }

                                            InflictOnTeamAction();
                                        });
                                    session.BCardDisposables[BCardId] = bcardDisposable;
                                }
                            }

                            break;

                        case CardType.Quest:
                            break;

                        case CardType.SecondSPCard:
                            var summonParameters2 = new List<MonsterToSummon>();
                            if (session.Character != null)
                            {
                                aliveTime = ServerManager.GetNpcMonster((short)SecondData).RespawnTime /
                                    (ServerManager.GetNpcMonster((short)SecondData).RespawnTime < 2400
                                        ? ServerManager.GetNpcMonster((short)SecondData).RespawnTime < 150
                                            ? 1
                                            : 10
                                        : 40) * (ServerManager.GetNpcMonster((short)SecondData).RespawnTime >=
                                                 150
                                        ? 4
                                        : 1);
                                for (var i = 0; i < firstData; i++)
                                {
                                    x = SecondData == 945
                                        ? session.PositionX
                                        : (short)(ServerManager.RandomNumber(-1, 1) + session.PositionX);
                                    y = SecondData == 945
                                        ? session.PositionY
                                        : (short)(ServerManager.RandomNumber(-1, 1) + session.PositionY);
                                    if (session.MapInstance.Map.IsBlockedZone(x, y))
                                    {
                                        x = session.PositionX;
                                        y = session.PositionY;
                                    }

                                    summonParameters2.Add(new MonsterToSummon((short)SecondData,
                                        new MapCell { X = x, Y = y }, null, true, isHostile: SecondData != 945,
                                        owner: session, aliveTime: aliveTime));
                                }

                                if (ServerManager.RandomNumber() <= Math.Abs(ThirdData) || ThirdData == 0 ||
                                    ThirdData < 0)
                                {
                                    switch (SubType)
                                    {
                                        case (byte)AdditionalTypes.SecondSPCard.PlantBomb:
                                            {
                                                if (session.MapInstance.Monsters.Any(s =>
                                                    s.Owner != null && s.Owner.MapEntityId == session.MapEntityId &&
                                                    s.MonsterVNum == SecondData))
                                                {
                                                    //Attack
                                                    session.MapInstance.Monsters
                                                        .Where(s => s.Owner?.MapEntityId == session.MapEntityId &&
                                                                    s.MonsterVNum == SecondData).ToList().ForEach(s =>
                                                                    {
                                                                        s.Target = s.BattleEntity;
                                                                    });

                                                    short NewCooldown = 300;
                                                    Observable.Timer(TimeSpan.FromMilliseconds(skill.Cooldown * 100 + 50))
                                                        .Subscribe(s =>
                                                        {
                                                            session.Character.Session.CurrentMapInstance.Broadcast(
                                                                StaticPacketHelper.SkillUsed(UserType.Player,
                                                                    session.Character.CharacterId, 1,
                                                                    session.Character.CharacterId, skill.SkillVNum,
                                                                    NewCooldown, 2, skill.Effect,
                                                                    session.Character.PositionX,
                                                                    session.Character.PositionY, true,
                                                                    (int)(session.Character.Hp /
                                                                        session.Character.HPLoad() * 100), 0, -1,
                                                                    (byte)(skill.SkillType - 1)));
                                                        });
                                                    var EndCooldownTime = DateTime.Now.AddMilliseconds(NewCooldown * 100);
                                                    session.Character.SkillsSp.FirstOrDefault(s => s.SkillVNum == SkillVNum)
                                                        .LastUse = EndCooldownTime;
                                                    Observable.Timer(EndCooldownTime).Subscribe(s =>
                                                    {
                                                        if (session.Character.SkillsSp != null &&
                                                            session.Character.SkillsSp.FirstOrDefault(c =>
                                                                c.SkillVNum == SkillVNum) is CharacterSkill cSkill)
                                                        {
                                                            if (cSkill.LastUse <= EndCooldownTime)
                                                            {
                                                                session.Character.Session.SendPacket(
                                                                    StaticPacketHelper.SkillReset(skill.CastId));
                                                            }
                                                        }
                                                    });
                                                }
                                                else
                                                {
                                                    EventHelper.Instance.RunEvent(new EventContainer(session.MapInstance,
                                                        EventActionType.SPAWNMONSTERS, summonParameters2));
                                                }
                                            }
                                            break;

                                        case (byte)AdditionalTypes.SecondSPCard.PlantSelfDestructionBomb:
                                            {
                                                EventHelper.Instance.RunEvent(new EventContainer(session.MapInstance,
                                                    EventActionType.SPAWNMONSTERS, summonParameters2));
                                            }
                                            break;
                                    }
                                }

                                session.Character.Session.SendPacket(session.Character.GenerateStat());
                            }

                            break;

                        case CardType.SPCardUpgrade:
                            break;

                        case CardType.HugeSnowman:
                            if (SubType == (byte)AdditionalTypes.HugeSnowman.SnowStorm)
                            {
                                if (sender.CanAttackEntity(session))
                                {
                                    session.GetDamage((int)(session.HpMax * 0.5D), sender, false, true);
                                    session.Character?.Session.SendPacket(session.Character.GenerateStat());
                                    session.Mate?.Owner.Session.SendPacket(session.Mate.GenerateStatInfo());
                                    if (session.Hp <= 0)
                                    {
                                        session.MapInstance.Broadcast(StaticPacketHelper.Die(session.UserType,
                                            session.MapEntityId, session.UserType, session.MapEntityId));
                                        if (session.Character != null)
                                        {
                                            Observable.Timer(TimeSpan.FromMilliseconds(1000)).Subscribe(obs =>
                                            {
                                                ServerManager.Instance.AskRevive(session.Character.CharacterId);
                                            });
                                        }
                                    }
                                }
                            }

                            break;

                        case CardType.Drain:

                            /* if (session.HasBuff(CardType.RecoveryAndDamagePercent, 01))
                             {
                                 return;
                             }*/
                            void DrainAction()
                            {
                                if (session.Hp > 0 && sender.Hp > 0)
                                {
                                    if (SubType == (byte)AdditionalTypes.Drain.TransferEnemyHP)
                                    {
                                        var bonus = 0;
                                        var senderChange = false;
                                        if (IsLevelScaled)
                                        {
                                            bonus = senderLevel * (firstData += 1);
                                        }
                                        else
                                        {
                                            bonus = firstData;
                                        }

                                        bonus = session.GetDamage(bonus, sender, true, true);
                                        if (bonus > 0)
                                        {
                                            session.MapInstance?.Broadcast(session.GenerateDm(bonus));
                                            session.Character?.Session?.SendPacket(session.Character?.GenerateStat());

                                            if (sender.Hp + bonus <= sender.HPLoad())
                                            {
                                                sender.Hp += bonus;
                                                senderChange = true;
                                            }
                                            else
                                            {
                                                bonus = (int)sender.HPLoad() - sender.Hp;
                                                sender.Hp = (int)sender.HPLoad();
                                                senderChange = true;
                                            }

                                            if (senderChange)
                                            {
                                                sender.MapInstance?.Broadcast(sender.GenerateRc(bonus));
                                                sender.Character?.Session?.SendPacket(sender.Character?.GenerateStat());
                                            }
                                        }
                                    }
                                }
                            }

                            DrainAction();
                            if (ThirdData > 0)
                            {
                                IDisposable bcardDisposable = null;
                                bcardDisposable = Observable.Interval(TimeSpan.FromSeconds(ThirdData * 2)).Subscribe(s =>
                                    {
                                        if (session.BCardDisposables[BCardId] != bcardDisposable)
                                        {
                                            bcardDisposable.Dispose();
                                            return;
                                        }

                                        if (session != null)
                                        {
                                            DrainAction();
                                        }
                                    });
                                session.BCardDisposables[BCardId] = bcardDisposable;
                            }

                            break;

                        case CardType.BossMonstersSkill:
                            break;

                        case CardType.LordHatus:
                            break;

                        case CardType.LordCalvinas:
                            break;

                        case CardType.SESpecialist:
                            if (SubType.Equals((byte)AdditionalTypes.SESpecialist.LowerHPStrongerEffect))
                            {
                                var hpPercentage = session.Hp / session.HPLoad() * 100;
                                if (hpPercentage < 35)
                                {
                                    session.AddBuff(new Buff(274, senderLevel), sender);
                                }
                                else if (hpPercentage < 67)
                                {
                                    session.AddBuff(new Buff(273, senderLevel), sender);
                                }
                                else
                                {
                                    session.AddBuff(new Buff(272, senderLevel), sender);
                                }
                            }

                            break;

                        case CardType.FourthGlacernonFamilyRaid:
                            break;

                        case CardType.SummonedMonsterAttack:
                            break;

                        case CardType.BearSpirit:
                            break;

                        case CardType.SummonSkill:
                            if (SubType.Equals((byte)AdditionalTypes.SummonSkill.Summon12) ||
                                SubType.Equals((byte)AdditionalTypes.SummonSkill.Summon10))
                            {
                                var amount = 0;

                                if (SubType.Equals((byte)AdditionalTypes.SummonSkill.Summon12))
                                {
                                    amount = 12;
                                }

                                if (SubType.Equals((byte)AdditionalTypes.SummonSkill.Summon10))
                                {
                                    amount = 10;
                                }

                                if (ServerManager.RandomNumber() < firstData)
                                {
                                    aliveTime = ServerManager.GetNpcMonster((short)SecondData).RespawnTime /
                                                (ServerManager.GetNpcMonster((short)SecondData).RespawnTime < 2400
                                                    ? ServerManager.GetNpcMonster((short)SecondData).RespawnTime <
                                                      150 ? 1 : 10
                                                    : 40) *
                                                (ServerManager.GetNpcMonster((short)SecondData).RespawnTime >= 150
                                                    ? 4
                                                    : 1);
                                    var canMove = SecondData != 797 && SecondData != 798 && SecondData != 2013 &&
                                                  SecondData != 2016;
                                    var monstersToSummon = new List<MonsterToSummon>();
                                    for (var i = 0; i < (amount > 0 ? amount : 1); i++)
                                    {
                                        x = (short)(ServerManager.RandomNumber(-1, 1) + sender.PositionX);
                                        y = (short)(ServerManager.RandomNumber(-1, 1) + sender.PositionY);

                                        if (skill != null && sender.Character == null)
                                        {
                                            var randomCell =
                                                sender.MapInstance.Map.GetRandomPositionByDistance(sender.PositionX,
                                                    sender.PositionY, skill.Range, true);

                                            if (randomCell != null)
                                            {
                                                x = randomCell.X;
                                                y = randomCell.Y;
                                            }
                                        }

                                        monstersToSummon.Add(new MonsterToSummon((short)SecondData,
                                            new MapCell { X = x, Y = y }, null, canMove, aliveTimeMp: aliveTime,
                                            owner: sender));
                                    }

                                    EventHelper.Instance.RunEvent(new EventContainer(sender.MapInstance,
                                        EventActionType.SPAWNMONSTERS, monstersToSummon));
                                }
                            }

                            break;

                        case CardType.InflictSkill:
                            break;

                        case CardType.HideBarrelSkill:
                            break;

                        case CardType.FocusEnemyAttentionSkill:
                            break;

                        case CardType.TauntSkill:
                            switch (SubType)
                            {
                                case (byte)AdditionalTypes.TauntSkill.TauntWhenKnockdown:
                                    if (session.Buffs.Any(s => s.Card.CardId == 500) &&
                                        ServerManager.RandomNumber() < FirstData)
                                    {
                                        session.AddBuff(new Buff((short)SecondData, senderLevel), sender);
                                    }

                                    break;

                                case (byte)AdditionalTypes.TauntSkill.TauntWhenNormal:
                                    if (!session.Buffs.Any(s => s.Card.CardId == 500) &&
                                        ServerManager.RandomNumber() < FirstData)
                                    {
                                        session.AddBuff(new Buff((short)SecondData, senderLevel), sender);
                                    }

                                    break;
                            }

                            break;

                        case CardType.FireCannoneerRangeBuff:
                            break;

                        case CardType.VulcanoElementBuff:
                            break;

                        case CardType.DamageConvertingSkill:
                            if (SubType.Equals((byte)AdditionalTypes.DamageConvertingSkill.TransferInflictedDamage))
                            {
                                if (sender.Character != null)
                                {
                                    sender.Character.Session.SendPacket(UserInterfaceHelper.GenerateMsg(
                                        string.Format(Language.Instance.GetMessageFromKey("SACRIFICES_THEMSELVES"),
                                            sender.Character.Name,
                                            session.Character?.Name ?? session.Mate?.Name ??
                                            session.MapNpc?.Name ?? session.MapMonster?.Name), 3));
                                    session.Character?.Session.SendPacket(
                                        UserInterfaceHelper.GenerateMsg(
                                            string.Format(Language.Instance.GetMessageFromKey("SACRIFICES_THEMSELVES"),
                                                sender.Character.Name, session.Character.Name), 3));

                                    IDisposable bcardDisposable = null;
                                    bcardDisposable = Observable.Interval(TimeSpan.FromMilliseconds(1000)).Subscribe(
                                        s =>
                                        {
                                            if (session.BCardDisposables[BCardId] != bcardDisposable)
                                            {
                                                bcardDisposable.Dispose();
                                                return;
                                            }

                                            if (!sender.Character.Session.HasCurrentMapInstance ||
                                                Map.GetDistance(
                                                    new MapCell { X = session.PositionX, Y = session.PositionY },
                                                    new MapCell { X = sender.PositionX, Y = sender.PositionY }) > 10 ||
                                                session.MapInstance != sender.MapInstance ||
                                                !sender.Buffs.Any(c => c.Card.CardId == 546))
                                            {
                                                sender.Character.Session.SendPacket(
                                                    UserInterfaceHelper.GenerateMsg(
                                                        string.Format(
                                                            Language.Instance.GetMessageFromKey(
                                                                "SACRIFICE_OUT_OF_RANGE")), 3));
                                                session.Character?.Session.SendPacket(
                                                    UserInterfaceHelper.GenerateMsg(
                                                        string.Format(
                                                            Language.Instance.GetMessageFromKey(
                                                                "SACRIFICE_OUT_OF_RANGE")), 3));
                                                session.ClearSacrificeBuff();
                                                session.BCardDisposables[BCardId].Dispose();
                                            }
                                        });
                                    session.BCardDisposables[BCardId] = bcardDisposable;
                                }
                            }

                            break;

                        case CardType.MeditationSkill:
                            {
                                if (sender.Character is Character character)
                                {
                                    if (SubType.Equals((byte)AdditionalTypes.MeditationSkill.Sacrifice))
                                    {
                                        session.AddBuff(new Buff((short)SecondData, senderLevel), sender);
                                    }
                                    else if (character.LastSkillComboUse < DateTime.Now)
                                    {
                                        ItemInstance sp = null;

                                        if (character.Inventory != null)
                                        {
                                            sp = character.Inventory.LoadBySlotAndType((byte)EquipmentType.Sp,
                                                InventoryType.Wear);
                                        }

                                        if (sp != null)
                                        {
                                            var newSkillVNum = (short)SecondData;

                                            if (SkillVNum.HasValue
                                                && SubType.Equals((byte)AdditionalTypes.MeditationSkill.CausingChance)
                                                && ServerManager.RandomNumber() < firstData)
                                            {
                                                if (character.SkillComboCount < 7)
                                                {
                                                    if (SkillHelper.IsCausingChance(SkillVNum.Value)
                                                        && session != sender)
                                                    {
                                                        return;
                                                    }

                                                    var oldSkill = character.GetSkill(SkillVNum.Value);
                                                    var newSkill = character.GetSkill(newSkillVNum);

                                                    if (oldSkill?.Skill != null && newSkill?.Skill != null)
                                                    {
                                                        newSkill.FirstCastId = oldSkill.FirstCastId;

                                                        if (newSkillVNum == 1126 && character.SkillComboCount > 4
                                                            || newSkillVNum == 1140 && character.SkillComboCount > 8)
                                                        {
                                                            character.SkillComboCount = 0;
                                                            character.LastSkillComboUse = DateTime.Now.AddSeconds(3);
                                                        }
                                                        else
                                                        {
                                                            character.SkillComboCount++;
                                                            character.LastSkillComboUse = DateTime.Now;

                                                            Observable.Timer(TimeSpan.FromMilliseconds(100)).Subscribe(
                                                                observer =>
                                                                {
                                                                    character.Session.SendPacket(
                                                                        $"mslot {newSkill.Skill.SkillVNum} -1");

                                                                    var quicklistEntries =
                                                                        character.QuicklistEntries.Where(s =>
                                                                            s.Morph == sp.Item.Morph &&
                                                                            s.Pos.Equals(oldSkill.FirstCastId.Value));

                                                                    foreach (var quicklistEntry in quicklistEntries)
                                                                    {
                                                                        character.Session.SendPacket(
                                                                            $"qset {quicklistEntry.Q1} {quicklistEntry.Q2} {quicklistEntry.Type}.{quicklistEntry.Slot}.{newSkill.Skill.CastId}.0");
                                                                    }
                                                                });

                                                            if (oldSkill.Skill.CastId > 10)
                                                            {
                                                                Observable.Timer(
                                                                    TimeSpan.FromMilliseconds(
                                                                        oldSkill.Skill.Cooldown * 100 + 500)).Subscribe(
                                                                    observer =>
                                                                    {
                                                                        if (character.SkillsSp != null &&
                                                                            character.SkillsSp.Any(s =>
                                                                                s.Skill.SkillVNum == oldSkill.SkillVNum &&
                                                                                s.CanBeUsed()))
                                                                        {
                                                                            character.Session.SendPacket(
                                                                                StaticPacketHelper.SkillReset(oldSkill.Skill
                                                                                    .CastId));
                                                                        }
                                                                    });
                                                            }
                                                        }
                                                    }
                                                }
                                            }

                                            if (character.MeditationDictionary != null)
                                            {
                                                lock (character.MeditationDictionary)
                                                {
                                                    switch (SubType)
                                                    {
                                                        case (byte)AdditionalTypes.MeditationSkill.ShortMeditation:
                                                            character.MeditationDictionary[newSkillVNum] =
                                                                DateTime.Now.AddSeconds(4);
                                                            break;

                                                        case (byte)AdditionalTypes.MeditationSkill.RegularMeditation:
                                                            character.MeditationDictionary[newSkillVNum] =
                                                                DateTime.Now.AddSeconds(8);
                                                            break;

                                                        case (byte)AdditionalTypes.MeditationSkill.LongMeditation:
                                                            character.MeditationDictionary[newSkillVNum] =
                                                                DateTime.Now.AddSeconds(12);
                                                            break;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            break;

                        case CardType.FalconSkill:
                            if (SubType.Equals((byte)AdditionalTypes.FalconSkill.Hide) ||
                                SubType.Equals((byte)AdditionalTypes.FalconSkill.Ambush))
                            {
                                if (session.Character is Character chara)
                                {
                                    if (chara.MapInstance.MapInstanceType != MapInstanceType.NormalInstance ||
                                        chara.MapInstance.Map.MapId != 2004)
                                    {
                                        if (x != 0 || y != 0)
                                        {
                                            chara.PositionX = x;
                                            chara.PositionY = y;
                                            chara.Session.CurrentMapInstance.Broadcast(chara.GenerateTp());
                                        }

                                        chara.Invisible = true;
                                        chara.Mates.Where(s => s.IsTeamMember).ToList().ForEach(s =>
                                            chara.Session.CurrentMapInstance?.Broadcast(s.GenerateOut()));
                                        chara.Session.CurrentMapInstance?.Broadcast(chara.GenerateInvisible());
                                    }
                                    else if (card != null)
                                    {
                                        chara.RemoveBuff(card.CardId);
                                    }
                                }
                            }
                            else if (SubType.Equals((byte)AdditionalTypes.FalconSkill.CausingChanceLocation))
                            {
                                if (session.Character is Character chara)
                                {
                                    if (chara.Session.CurrentMapInstance != null)
                                    {
                                        var ownedMonsters = new ConcurrentBag<MapMonster>();
                                        ServerManager.GetAllMapInstances().ForEach(s =>
                                            s.Monsters.Where(m => m.Owner?.MapEntityId == chara.CharacterId).ToList()
                                                .ForEach(mon => ownedMonsters.Add(mon)));
                                        if (ownedMonsters.Count >= 3)
                                        {
                                            chara.BattleEntity.RemoveOwnedMonsters(true);
                                        }

                                        var monster = new MapMonster
                                        {
                                            MonsterVNum = (short)SecondData,
                                            MapX = x > 0 ? x : chara.Session.Character.PositionX,
                                            MapY = y > 0 ? y : chara.Session.Character.PositionY,
                                            MapId = chara.Session.Character.MapInstance.Map.MapId,
                                            Position = chara.Session.Character.Direction,
                                            MapMonsterId = chara.Session.CurrentMapInstance.GetNextMonsterId(),
                                            ShouldRespawn = false,
                                            Invisible = true,
                                            AliveTime = SecondData == 1436
                                                ? 20
                                                : ServerManager.GetNpcMonster((short)SecondData).BasicCooldown > 0
                                                    ? ServerManager.GetNpcMonster((short)SecondData).BasicCooldown
                                                    : ServerManager.GetNpcMonster((short)SecondData).RespawnTime,
                                            Owner = chara.BattleEntity
                                        };
                                        monster.Initialize(chara.Session.CurrentMapInstance);
                                        chara.Session.CurrentMapInstance.AddMonster(monster);
                                        chara.Session.CurrentMapInstance.Broadcast(monster.GenerateIn());

                                        if (monster.MonsterVNum == 1438 || monster.MonsterVNum == 1439)
                                        {
                                            monster.Target = monster.BattleEntity;
                                            monster.StartLife();
                                            var npcMonsterSkill = monster.Skills.FirstOrDefault();
                                            session.MapInstance.Broadcast(StaticPacketHelper.SkillUsed(UserType.Monster,
                                                monster.MapMonsterId, (byte)UserType.Monster, monster.MapMonsterId,
                                                npcMonsterSkill.SkillVNum, npcMonsterSkill.Skill.Cooldown,
                                                npcMonsterSkill.Skill.AttackAnimation, npcMonsterSkill.Skill.Effect,
                                                monster.MapX, monster.MapY,
                                                monster.CurrentHp > 0,
                                                (int)(monster.CurrentHp / monster.MaxHp * 100), 0,
                                                0, 0));
                                        }
                                    }
                                }
                            }
                            else if (SubType.Equals((byte)AdditionalTypes.FalconSkill.FalconFollowing))
                            {
                                if (sender.Character != null)
                                {
                                    sender.Character.BattleEntity.ClearOwnFalcon();
                                    sender.Character.BattleEntity.FalconFocusedEntityId = session.MapEntityId;
                                    sender.MapInstance.Broadcast(
                                        $"eff_ob  {(byte)session.UserType} {session.MapEntityId} 1 4269");
                                    if (skill != null)
                                    {
                                        session.MapInstance.Broadcast(StaticPacketHelper.SkillUsed(sender.UserType,
                                            sender.MapEntityId, (byte)session.UserType, session.MapEntityId,
                                            skill.SkillVNum, skill.Cooldown, skill.AttackAnimation, skill.Effect, x, y,
                                            true, session.Hp * 100 / session.HpMax, 0, -1, 0));
                                    }

                                    sender.Character.BattleEntity.BCardDisposables[BCardId]?.Dispose();
                                    IDisposable bcardDisposable = null;
                                    bcardDisposable = Observable.Timer(TimeSpan.FromSeconds(60)).Subscribe(s =>
                                    {
                                        if (sender.Character.BattleEntity.BCardDisposables[BCardId] != bcardDisposable)
                                        {
                                            bcardDisposable.Dispose();
                                            return;
                                        }

                                        sender.Character.BattleEntity.ClearOwnFalcon();
                                    });
                                    sender.Character.BattleEntity.BCardDisposables[BCardId] = bcardDisposable;
                                }
                            }

                            break;

                        case CardType.AbsorptionAndPowerSkill:
                            break;

                        case CardType.LeonaPassiveSkill:
                            break;

                        case CardType.FearSkill:
                            if (SubType.Equals((byte)AdditionalTypes.FearSkill.TimesUsed))
                            {
                                if (sender.Character != null)
                                {
                                    if (FirstData == 1 && sender.Character.SkillComboCount == 3 ||
                                        FirstData == 2 && sender.Character.SkillComboCount == 5)
                                    {
                                        sender.AddBuff(new Buff((short)SecondData, senderLevel), sender);
                                    }
                                }
                            }
                            else if (SubType.Equals((byte)AdditionalTypes.FearSkill.AttackRangedIncreased))
                            {
                                if (session.Character != null)
                                {
                                    session.Character.Session.SendPacket($"bf_d {FirstData} 1");
                                }
                            }
                            else if (SubType.Equals((byte)AdditionalTypes.FearSkill.MoveAgainstWill))
                            {
                                if (session.Character != null)
                                {
                                    session.Character.Session.SendPacket($"rv_m {session.MapEntityId} 1 1");
                                }
                            }
                            else if (SubType.Equals((byte)AdditionalTypes.FearSkill.ProduceWhenAmbushe))
                            {
                                if (sender == session && (x != 0 || y != 0) && SecondData > 0)
                                {
                                    if (ServerManager.RandomNumber() < firstData)
                                    {
                                        sender.AddBuff(new Buff((short)SecondData, senderLevel), sender);
                                    }
                                }
                            }

                            break;

                        case CardType.SniperAttack:
                            if (session == sender)
                            {
                                return;
                            }

                            if (SubType.Equals((byte)AdditionalTypes.SniperAttack.ChanceCausing))
                            {
                                if (ServerManager.RandomNumber() < firstData)
                                {
                                    var ChanceCausingBuff = new Buff((short)SecondData, senderLevel);
                                    if (ChanceCausingBuff.Card.BuffType == BuffType.Good ||
                                        ChanceCausingBuff.Card.BuffType == BuffType.Neutral)
                                    {
                                        sender.AddBuff(ChanceCausingBuff, sender);
                                    }
                                    else
                                    {
                                        session.AddBuff(ChanceCausingBuff, sender);
                                    }
                                }
                            }
                            else if (SubType.Equals((byte)AdditionalTypes.SniperAttack.ProduceChance))
                            {
                                if (sender.Buffs.Any(s => s.Card.BCards.Any(b =>
                                    b.Type == (byte)CardType.FalconSkill &&
                                    (b.SubType == (byte)AdditionalTypes.FalconSkill.Hide ||
                                     b.SubType == (byte)AdditionalTypes.FalconSkill.Ambush))))
                                {
                                    if (ServerManager.RandomNumber() < firstData)
                                    {
                                        var ProduceChanceBuff = new Buff((short)SecondData, senderLevel);
                                        if (ProduceChanceBuff.Card.BuffType == BuffType.Good ||
                                            ProduceChanceBuff.Card.BuffType == BuffType.Neutral)
                                        {
                                            sender.AddBuff(ProduceChanceBuff, sender);
                                        }
                                        else
                                        {
                                            session.AddBuff(ProduceChanceBuff, sender);
                                        }
                                    }
                                }
                            }

                            break;

                        case CardType.FrozenDebuff:
                            {
                                if (SubType == (byte)AdditionalTypes.FrozenDebuff.GlacerusSkill)
                                {
                                    var mapInstance = sender.MapInstance;

                                    if (mapInstance?.MapInstanceType == MapInstanceType.RaidInstance)
                                    {
                                        mapInstance.Broadcast(
                                            UserInterfaceHelper.GenerateMsg(
                                                Language.Instance.GetMessageFromKey("GLACERUS_GRRR"), 0));

                                        // SafeZone

                                        for (short monsterVNum = 4280; monsterVNum <= 4282; monsterVNum++)
                                        {
                                            EventHelper.Instance.RunEvent(new EventContainer(mapInstance,
                                                EventActionType.SPAWNMONSTER, new MonsterToSummon(monsterVNum,
                                                    new MapCell { X = 0, Y = 0 }, null, false)
                                                {
                                                    AfterSpawnEvents = new List<EventContainer>
                                                    {
                                                    new EventContainer(mapInstance, EventActionType.EFFECT,
                                                        new Tuple<short, int>(monsterVNum, 0)),
                                                    new EventContainer(mapInstance, EventActionType.REMOVEAFTER, 15)
                                                    }
                                                }));
                                        }

                                        Observable.Timer(TimeSpan.FromSeconds(5)).Subscribe(t =>
                                        {
                                            foreach (var character in mapInstance.Sessions.Where(s => s?.Character != null)
                                                .Select(s => s.Character))
                                            {
                                                // Wind

                                                character.Session.SendPacket(StaticPacketHelper.GenerateEff(UserType.Player,
                                                    character.CharacterId, 4293));

                                                // Freeze

                                                if (character.Hp < 1
                                                    || character.HasBuff(CardType.FrozenDebuff,
                                                        (byte)AdditionalTypes.FrozenDebuff.EternalIce))
                                                {
                                                    continue;
                                                }

                                                var safeZoneList = mapInstance
                                                    .GetMonsterInRangeList(character.PositionX, character.PositionY, 5)
                                                    .Where(m => m.MonsterVNum >= 4280 && m.MonsterVNum <= 4282);

                                                if (!safeZoneList.Any())
                                                {
                                                    character.AddBuff(new Buff(569, sender.Level), sender);

                                                    if (!mapInstance.Sessions.Any(s => s.Character != null
                                                                                       && !s.Character.HasBuff(
                                                                                           CardType.FrozenDebuff,
                                                                                           (byte)AdditionalTypes
                                                                                               .FrozenDebuff.EternalIce)))
                                                    {
                                                        EventHelper.Instance.RunEvent(new EventContainer(mapInstance,
                                                            EventActionType.SENDPACKET,
                                                            UserInterfaceHelper.GenerateMsg(
                                                                Language.Instance.GetMessageFromKey("ALL_FROZEN"), 0)));
                                                        Observable.Timer(TimeSpan.FromSeconds(5)).Subscribe(_ =>
                                                            EventHelper.Instance.RunEvent(new EventContainer(mapInstance,
                                                                EventActionType.SCRIPTEND, (byte)4)));
                                                    }
                                                }
                                            }
                                        });
                                    }
                                }
                            }
                            break;

                        case CardType.JumpBackPush:
                            if (!ServerManager.RandomProbabilityCheck(session.ResistForcedMovement))
                            {
                                if (SubType.Equals((byte)AdditionalTypes.JumpBackPush.JumpBackChance))
                                {
                                    if (ServerManager.RandomNumber() < firstData)
                                    {
                                        PushBackSession(SecondData, sender, session);
                                    }
                                }

                                if (SubType.Equals((byte)AdditionalTypes.JumpBackPush.PushBackChance))
                                {
                                    if (ServerManager.RandomNumber() < firstData)
                                    {
                                        PushBackSession(SecondData, session, sender);
                                    }
                                }
                            }

                            break;

                        case CardType.FairyXPIncrease:
                            break;

                        case CardType.SummonAndRecoverHP:
                            var summonParameters3 = new List<MonsterToSummon>();
                            if (ServerManager.RandomNumber() <= Math.Abs(ThirdData) || ThirdData == 0 || ThirdData < 0)
                            {
                                aliveTime = ServerManager.GetNpcMonster((short)SecondData).RespawnTime /
                                    (ServerManager.GetNpcMonster((short)SecondData).RespawnTime < 2400
                                        ? ServerManager.GetNpcMonster((short)SecondData).RespawnTime < 150
                                            ? 1
                                            : 10
                                        : 40) * (ServerManager.GetNpcMonster((short)SecondData).RespawnTime >=
                                                 150
                                        ? 4
                                        : 1);
                                switch (SubType)
                                {
                                    case 11:
                                        if (sender.GetOwnedMonsters().Where(s => s.MonsterVNum == SecondData).Count() ==
                                            0)
                                        {
                                            summonParameters3.Add(new MonsterToSummon((short)SecondData,
                                                new MapCell { X = sender.PositionX, Y = sender.PositionY }, null, true,
                                                aliveTime: aliveTime, owner: sender));
                                            EventHelper.Instance.RunEvent(new EventContainer(sender.MapInstance,
                                                EventActionType.SPAWNMONSTERS, summonParameters3));
                                        }

                                        break;

                                    case 12:
                                        Observable.Timer(TimeSpan.FromSeconds(1)).Subscribe(spawn =>
                                        {
                                            if (sender.MapMonster != null && sender.MapMonster.Owner != null &&
                                                sender.MapMonster.Owner.GetOwnedMonsters().Where(s =>
                                                    s.MonsterVNum == sender.MapMonster.MonsterVNum).Count() == 1)
                                            {
                                                for (var i = 0; i < firstData - 1; i++)
                                                {
                                                    x = (short)(ServerManager.RandomNumber(-1, 1) + sender.PositionX);
                                                    y = (short)(ServerManager.RandomNumber(-1, 1) + sender.PositionY);
                                                    summonParameters3.Add(new MonsterToSummon(
                                                        sender.MapMonster.MonsterVNum, new MapCell { X = x, Y = y }, null,
                                                        true, aliveTime: aliveTime, owner: sender.MapMonster.Owner));
                                                }

                                                EventHelper.Instance.RunEvent(new EventContainer(sender.MapInstance,
                                                    EventActionType.SPAWNMONSTERS, summonParameters3));
                                            }
                                        });
                                        break;
                                }
                            }

                            break;

                        case CardType.TeamArenaBuff:
                            break;

                        case CardType.ArenaCamera:
                            break;

                        case CardType.DarkCloneSummon:
                            {
                                if (SubType == (byte)AdditionalTypes.DarkCloneSummon.SummonDarkCloneChance)
                                {
                                    if (ServerManager.RandomNumber() < FirstData)
                                    {
                                        var monstersToSummon = new List<MonsterToSummon>();

                                        var monsterVNums = Enumerable.Range(2112, SecondData)
                                            .OrderBy(t => ServerManager.RandomNumber()).Select(t => (short)t).ToList();

                                        for (var i = 0; i < ServerManager.RandomNumber(1, monsterVNums.Count + 1); i++)
                                        {
                                            var mapX = (short)(sender.PositionX + ServerManager.RandomNumber(-2, 2));
                                            var mapY = (short)(sender.PositionY + ServerManager.RandomNumber(-2, 2));

                                            if (sender.MapInstance.Map.IsBlockedZone(mapX, mapY))
                                            {
                                                mapX = sender.PositionX;
                                                mapY = sender.PositionY;
                                            }

                                            monstersToSummon.Add(new MonsterToSummon(monsterVNums[i],
                                                new MapCell { X = mapX, Y = mapY }, session,
                                                true, false, false, false, false, sender, 5, 0, 0, 1 /* second(s) */));
                                        }

                                        EventHelper.Instance.RunEvent(new EventContainer(sender.MapInstance,
                                            EventActionType.SPAWNMONSTERS, monstersToSummon));
                                    }
                                }
                            }
                            break;

                        case CardType.AbsorbedSpirit:
                            {
                                var hasSpiritAbsorption = session.HasBuff(596);

                                switch (SubType)
                                {
                                    case (byte)AdditionalTypes.AbsorbedSpirit.ApplyEffectIfPresent:
                                        if (hasSpiritAbsorption)
                                        {
                                            session.AddBuff(new Buff((short)SecondData, session.Level), session);
                                            session.RemoveBuff(596);
                                        }

                                        break;

                                    case (byte)AdditionalTypes.AbsorbedSpirit.ApplyEffectIfNotPresent:
                                        if (!hasSpiritAbsorption && !session.HasBuff(599))
                                        {
                                            session.AddBuff(new Buff((short)SecondData, session.Level), session);
                                        }

                                        break;
                                }
                            }
                            break;

                        case CardType.AngerSkill:
                            break;

                        case CardType.MeteoriteTeleport:
                            {
                                if (SubType == (byte)AdditionalTypes.MeteoriteTeleport.SummonInVisualRange)
                                {
                                    var mapInstance = session?.MapInstance;

                                    if (mapInstance != null)
                                    {
                                        var monstersToSummon = new List<MonsterToSummon>();

                                        for (var i = 0; i < 73; i++)
                                        {
                                            monstersToSummon.Add(new MonsterToSummon(2328, new MapCell { X = 0, Y = 0 }, null, false, hasDelay: (short)ServerManager.RandomNumber(0, 5)));
                                        }

                                        EventHelper.Instance.RunEvent(new EventContainer(mapInstance, EventActionType.SPAWNMONSTERS, monstersToSummon));
                                    }
                                }
                                else if (SubType == (byte)AdditionalTypes.MeteoriteTeleport.TransformTarget)
                                {
                                    int[] morphVNums = { 1000099, 1000156 };

                                    if (sender != null && session?.Character?.MapInstance != null && !morphVNums.Contains(session.Character.Morph) && ServerManager.RandomNumber() < 50)
                                    {
                                        session.Character.MapInstance.Broadcast(session.Character.GenerateEff(4054));
                                        session.Character.IsMorphed = true;
                                        session.Character.PreviousMorph = session.Character.Morph;
                                        session.Character.Morph =
                                            morphVNums.OrderBy(rnd => ServerManager.RandomNumber()).First();

                                        switch (session.Character.Morph)
                                        {
                                            case 1000099: // Hamster
                                                session.AddBuff(new Buff(478, sender.Level, true), sender);
                                                break;

                                            case 1000156: // Bushtail
                                                session.AddBuff(new Buff(477, sender.Level, true), sender);
                                                break;
                                        }

                                        session.Character.MapInstance.Broadcast(session.Character.GenerateCMode());
                                    }
                                }
                                else if (SubType == (byte)AdditionalTypes.MeteoriteTeleport.TeleportForward)
                                {
                                    session.TeleportTo(session.MapInstance.Map.GetRandomPositionByDistance(session.PositionX, session.PositionY, (short)FirstData));
                                }
                                else if (SubType == (byte)AdditionalTypes.MeteoriteTeleport.CauseMeteoriteFall)
                                {
                                    var mapInstance = session?.MapInstance;

                                    if (mapInstance != null)
                                    {
                                        var monstersToSummon = new List<MonsterToSummon>();

                                        var range = 10;

                                        for (var i = 0; i < 10 + (int)(session.Level / (double)FirstData); i++)
                                        {
                                            var mapCell = new MapCell
                                            {
                                                X = (short)(session.PositionX + ServerManager.RandomNumber(-range, range + 1)),
                                                Y = (short)(session.PositionY + ServerManager.RandomNumber(-range, range + 1))
                                            };

                                            var test = ServerManager.TrueRandomNumber<short>(0, 10 + 1);

                                            monstersToSummon.Add(new MonsterToSummon((short)ServerManager.RandomNumber(2352, 2353 + 1), mapCell, null, false, owner: session, hasDelay: test)
                                            {
                                                IsMeteorite = true
                                            });
                                        }

                                        if (monstersToSummon.Any())
                                        {
                                            EventHelper.Instance.RunEvent(new EventContainer(mapInstance, EventActionType.SPAWNMONSTERS, monstersToSummon));
                                        }
                                    }
                                }
                                else if (SubType == (byte)AdditionalTypes.MeteoriteTeleport.TeleportYouAndGroupToSavedLocation)
                                {
                                    if (session.Character is Character character && character.CharacterId == sender?.Character?.CharacterId)
                                    {
                                        if (character.SavedLocation == null)
                                        {
                                            character.SavedLocation = new MapCell
                                            {
                                                X = character.PositionX,
                                                Y = character.PositionY
                                            };

                                            session.MapInstance?.Broadcast($"eff_g 4497 {character.CharacterId} {character.SavedLocation.X} {character.SavedLocation.Y} 0");
                                        }
                                        else
                                        {
                                            var mapCellTo = new MapCell
                                            {
                                                X = character.SavedLocation.X,
                                                Y = character.SavedLocation.Y
                                            };

                                            var mapCellFrom = new MapCell
                                            {
                                                X = session.PositionX,
                                                Y = session.PositionY
                                            };

                                            session.MapInstance?.Broadcast($"eff_g 4483 {character.CharacterId} {mapCellFrom.X} {mapCellFrom.Y} 0");

                                            Observable.Timer(TimeSpan.FromSeconds(1)).Subscribe(t =>
                                            {
                                                session.TeleportTo(mapCellTo);

                                                session.MapInstance?.Broadcast($"eff_g 4497 {character.CharacterId} {mapCellTo.X} {mapCellTo.Y} 1");

                                                var friendCharacters = new List<Character>();

                                                if (character.Family?.FamilyCharacters != null)
                                                {
                                                    friendCharacters.AddRange(character.Family.FamilyCharacters.Select(fc =>ServerManager.Instance.GetCharacterById(fc.CharacterId)).Where(c => c != null));
                                                }

                                                if (character.Group?.Sessions != null)
                                                {
                                                    friendCharacters.AddRange(character.Group.Sessions.Where(s => s.Character != null).Select(s => s.Character));
                                                }

                                                friendCharacters.Where(c => c.CharacterId != character.CharacterId && c.MapInstanceId == character.MapInstanceId && Map.GetDistance(c.BattleEntity.GetPos(), mapCellFrom) <= skill.TargetRange).OrderBy(c => Map.GetDistance(c.BattleEntity.GetPos(), mapCellFrom)).Take(FirstData).ToList().ForEach(c => c.BattleEntity.TeleportTo(mapCellTo, 3));
                                            });
                                        }
                                    }
                                }
                            }
                            break;

                        case CardType.StealBuff:
                            break;

                        case CardType.PvPFairyEffects:
                            break;

                        case CardType.EffectSummon:
                            break;

                        case CardType.DragonSkills:
                            switch (SubType)
                            {
                                case (byte)AdditionalTypes.DragonSkills.TransformationInverted:
                                    if (session.Character is Character reversedMorph)
                                    {
                                        reversedMorph.Morph = 29;
                                        reversedMorph.Session.SendPacket(reversedMorph.GenerateCMode());
                                        reversedMorph.Session.SendPacket(StaticPacketHelper.GenerateEff(UserType.Player,
                                            reversedMorph.CharacterId, 196));
                                        reversedMorph.DragonModeObservable?.Dispose();
                                        reversedMorph.RemoveBuff(676);
                                    }

                                    break;

                                case (byte)AdditionalTypes.DragonSkills.Transformation:
                                    if (!CardId.HasValue)
                                    {
                                        break;
                                    }

                                    var morphCard = ServerManager.GetCard(CardId.Value);

                                    if (morphCard == null)
                                    {
                                        return;
                                    }

                                    if (session.Character is Character morphedChar)
                                    {
                                        morphedChar.Morph = 30;
                                        morphedChar.Session.SendPacket(morphedChar.GenerateCMode());
                                        morphedChar.Session.SendPacket(StaticPacketHelper.GenerateEff(UserType.Player,
                                            morphedChar.CharacterId, 196));
                                        morphedChar.DragonModeObservable?.Dispose();

                                        morphedChar.DragonModeObservable = Observable
                                            .Timer(TimeSpan.FromSeconds(morphCard.Duration * 0.1)).Subscribe(s =>
                                            {
                                                morphedChar.Morph = 29;
                                                morphedChar.Session.SendPacket(morphedChar.GenerateCMode());
                                                morphedChar.Session.SendPacket(
                                                    StaticPacketHelper.GenerateEff(UserType.Player,
                                                        morphedChar.CharacterId, 196));
                                            });
                                    }

                                    break;
                            }

                            break;

                        case CardType.LotusSkills:
                            if (SubType == (byte)AdditionalTypes.LotusSkills.ChangeLotusSkills)
                            {
                                session.Character.Session.SendPacket("ms_c 0");
                                session.Character.Session.SendPackets(session.Character.GenerateQuicklist());
                            }

                            break;

                        case CardType.WolfMaster:
                            {
                                var user = sender.Character ?? session.Character;

                                if (user == null)
                                {
                                    break;
                                }

                                switch (SubType)
                                {
                                    case (byte)AdditionalTypes.WolfMaster.AddUltimatePoints:
                                        {
                                            user.AddUltimatePoints((short)FirstData);
                                            user.Session.SendPacket(user.GenerateFtPtPacket());
                                            user.AddWolfBuffs();
                                        }
                                        break;

                                    case (byte)AdditionalTypes.WolfMaster.CanExecuteUltimateSkills:
                                        {
                                            user.Session.SendPacket(user.GenerateFtPtPacket());
                                            user.Session.SendPackets(user.GenerateQuicklist());
                                        }
                                        break;
                                }
                            }
                            break;

                        case CardType.SpawnEqMonster:
                            break;

                        case CardType.IncreaseDamageVsChar:
                            break;

                        case CardType.GiveBuff:
                            break;

                        case CardType.A7Powers1:
                            switch (SubType)
                            {
                                case (byte)AdditionalTypes.A7Powers1.DamageApocalypsePower:

                                    if (sender == null) return;

                                    if (ServerManager.RandomProbabilityCheck(FirstData))
                                    {
                                        var damageToTake = sender.Level * 18;

                                        session.GetDamage(damageToTake, sender, true);
                                        session.MapInstance.Broadcast(session.GenerateDm(damageToTake));

                                        session.AddBuff(new Buff((short)SecondData, sender.Level), sender);
                                    }

                                    break;

                                case (byte)AdditionalTypes.A7Powers1.ReflectionPower:

                                    if (sender == null) return;

                                    if (ServerManager.RandomProbabilityCheck(FirstData))
                                    {
                                        sender.AddBuff(new Buff((short)SecondData, sender.Level), sender);
                                    }

                                    break;

                                case (byte)AdditionalTypes.A7Powers1.DamageWolfPower:

                                    if (sender == null) return;

                                    if (ServerManager.RandomProbabilityCheck(FirstData))
                                    {
                                        var damageToTake = sender.Level * 15;

                                        session.GetDamage(damageToTake, sender, true);
                                        session.MapInstance.Broadcast(session.GenerateDm(damageToTake));

                                        session.AddBuff(new Buff((short)SecondData, sender.Level), sender);
                                    }

                                    break;

                                case (byte)AdditionalTypes.A7Powers1.EnemyKnockedBack:

                                    if (sender == null) return;

                                    if (!ServerManager.RandomProbabilityCheck(session.ResistForcedMovement))
                                    {
                                        PushBackSession(4, session, sender);

                                        session.AddBuff(new Buff((short)SecondData, sender.Level), sender);
                                    }

                                    break;

                                case (byte)AdditionalTypes.A7Powers1.DamageExplosionPower:

                                    if (sender == null) return;

                                    if (ServerManager.RandomProbabilityCheck(FirstData))
                                    {
                                        var damageToTake = sender.Level * 17;

                                        session.GetDamage(damageToTake, sender, true);
                                        session.MapInstance.Broadcast(session.GenerateDm(damageToTake));

                                        session.AddBuff(new Buff((short)SecondData, sender.Level), sender);
                                    }

                                    break;
                            }

                            break;

                        case CardType.A7Powers2:
                            switch (SubType)
                            {
                                case (byte)AdditionalTypes.A7Powers2.ReceiveAgilityPower:

                                    if (sender == null) return;

                                    if (ServerManager.RandomProbabilityCheck(FirstData))
                                    {
                                        sender.AddBuff(new Buff((short)SecondData, sender.Level), sender);
                                    }

                                    break;

                                case (byte)AdditionalTypes.A7Powers2.DamageLightingPower:

                                    if (sender == null) return;

                                    if (ServerManager.RandomProbabilityCheck(FirstData))
                                    {
                                        var damageToTake = sender.Level * 18;

                                        session.GetDamage(damageToTake, sender, true);
                                        session.MapInstance.Broadcast(session.GenerateDm(damageToTake));

                                        session.AddBuff(new Buff((short)SecondData, sender.Level), sender);
                                    }

                                    break;

                                case (byte)AdditionalTypes.A7Powers2.TriggerCursePower:

                                    if (sender == null) return;

                                    if (ServerManager.RandomProbabilityCheck(FirstData))
                                    {
                                        session.AddBuff(new Buff((short)SecondData, sender.Level), sender);
                                    }

                                    break;

                                case (byte)AdditionalTypes.A7Powers2.DamageBearPower:

                                    if (sender == null) return;

                                    if (ServerManager.RandomProbabilityCheck(FirstData))
                                    {
                                        var damageToTake = sender.Level * 23;

                                        session.GetDamage(damageToTake, sender, true);
                                        session.MapInstance.Broadcast(session.GenerateDm(damageToTake));

                                        session.AddBuff(new Buff((short)SecondData, sender.Level), sender);
                                    }

                                    break;

                                case (byte)AdditionalTypes.A7Powers2.ReceiveFrostPower:

                                    if (sender == null) return;

                                    if (ServerManager.RandomProbabilityCheck(FirstData))
                                    {
                                        sender.AddBuff(new Buff((short)SecondData, sender.Level), sender);
                                    }

                                    break;
                            }

                            break;

                        default:
                            Logger.Log.Warn($"Card Type {Type} not defined!");
                            break;
                    }
                });

            void PushBackSession(int PushDistance, BattleEntity receiver, BattleEntity point)
            {
                if (receiver.MapMonster != null
                && (receiver.MapMonster.IsBoss
                 || ServerManager.Instance.BossVNums.Contains(receiver.MapMonster.MonsterVNum)
                 || receiver.MapMonster.Owner?.Character != null || receiver.MapMonster.Owner?.Mate != null))
                {
                    return;
                }

                if (receiver.MapNpc != null
                && (ServerManager.Instance.BossVNums.Contains(receiver.MapNpc.NpcVNum)))
                {
                    return;
                }

                receiver.Character?.WalkDisposable?.Dispose();

                var NewX = receiver.PositionX;
                var NewY = receiver.PositionY;

                if (point.PositionX == receiver.PositionX && point.PositionY == receiver.PositionY)
                {
                    var randomCell =
                        receiver.MapInstance.Map.GetRandomPositionByDistance(point.PositionX, point.PositionY,
                            (short)PushDistance);
                    if (randomCell != null)
                    {
                        NewX = randomCell.X;
                        NewY = randomCell.Y;
                    }
                }
                else
                {
                    var pointPosition = new Point3D(point.PositionX, point.PositionY, 0.0);
                    var receiverPosition = new Point3D(receiver.PositionX, receiver.PositionY, 0.0);
                    var newPosition = new Point3D(receiver.PositionX, receiver.PositionY, 0.0);
                    for (var i = 0;
                        i == 0 || PushDistance - i >= 0 && receiver.MapInstance.Map.IsBlockedZone(receiver.PositionX,
                            receiver.PositionY, (short)Math.Round(newPosition.X), (short)Math.Round(newPosition.Y));
                        i++)
                    {
                        var v = receiverPosition - pointPosition;
                        double offset = PushDistance - i;
                        newPosition = ExtendLine(receiverPosition, offset, v);
                    }
                    NewX = (short)Math.Round(newPosition.X);
                    NewY = (short)Math.Round(newPosition.Y);
                }

                receiver.PositionX = NewX;
                receiver.PositionY = NewY;

                if (receiver == sender)
                {
                    receiver.MapInstance.Broadcast($"guri 3 {(short)receiver.UserType} {receiver.MapEntityId} {receiver.PositionX} {receiver.PositionY} 3 {(int)(SecondData / 4)} 2 -1");
                }
                else
                {
                    receiver.MapInstance.Broadcast($"guri 3 {(short)receiver.UserType} {receiver.MapEntityId} {receiver.PositionX} {receiver.PositionY} 3 {SecondData} 2 -1");
                }
            }
        }

        private static Point3D ExtendLine(Point3D point, double offset, Vector3D vector)
        {
            vector.Normalize();
            double _offset = offset / vector.Length;
            Vector3D _vector = vector * _offset;
            Matrix3D m = new Matrix3D();
            m.Translate(_vector);
            Point3D result = m.Transform(point);
            return result;
        }

        #endregion
    }
}