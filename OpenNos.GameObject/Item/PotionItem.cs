﻿/*
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

using OpenNos.Data;
using OpenNos.Domain;
using OpenNos.GameObject.Networking;
using System;
using System.Diagnostics;
using System.Linq;
using OpenNos.GameObject.Extension.CharacterExtensions;
using OpenNos.Core;
using System.Reactive.Linq;

namespace OpenNos.GameObject
{
    public class PotionItem : Item
    {
        #region Instantiation

        public PotionItem(ItemDTO item) : base(item)
        {
        }

        #endregion

        #region Methods

        public override void Use(ClientSession session, ref ItemInstance inv, byte Option = 0, string[] packetsplit = null)
        {
            session.Character.PotionRequests++;
            if (!session.HasCurrentMapInstance)
            {
                return;
            }

            if (session.CurrentMapInstance.MapInstanceType == MapInstanceType.ArenaInstance)
            {
                return;
            }

            if (session.CurrentMapInstance.MapInstanceType == MapInstanceType.RainbowBattleInstance)
            {
                return;
            }

            if (session.Character.IsLaurenaMorph())
            {
                return;
            }

            if ((DateTime.Now - session.Character.LastPotion).TotalMilliseconds < (session.CurrentMapInstance.Map.MapTypes.OrderByDescending(s => s.PotionDelay).FirstOrDefault()?.PotionDelay ?? 750))
            {
                return;
            }

            if (session.CurrentMapInstance.MapInstanceType.Equals(MapInstanceType.TalentArenaMapInstance))
            {
                return;
            }

            if (session.CurrentMapInstance.MapInstanceType != MapInstanceType.TalentArenaMapInstance && VNum == 5935)
            {
                return;
            }

            double currentRunningSeconds = (DateTime.Now - Process.GetCurrentProcess().StartTime.AddSeconds(-50)).TotalSeconds;

            double timeSpanSinceLastPortal = currentRunningSeconds - session.Character.LastPortal;

            if (ServerManager.Instance.ChannelId == 51
                && session.Character.MapId != 130
                && session.Character.MapId != 131
                && (session.Character.Group?.Raid == null || !session.Character.Group.Raid.InstanceBag.Lock)
                && session.Character.MapInstance.MapInstanceType != MapInstanceType.Act4Berios
                && session.Character.MapInstance.MapInstanceType != MapInstanceType.Act4Calvina
                && session.Character.MapInstance.MapInstanceType != MapInstanceType.Act4Hatus
                && session.Character.MapInstance.MapInstanceType != MapInstanceType.Act4Morcos
                && (inv.ItemVNum == 1242 || inv.ItemVNum == 1243 || inv.ItemVNum == 1244 || inv.ItemVNum == 9129 || inv.ItemVNum == 5582 || inv.ItemVNum == 5583 || inv.ItemVNum == 5584))
            {
                return;
            }

            if (session.Character.PotionRequests > 60)
            {
                PenaltyLogDTO log = new PenaltyLogDTO
                {
                    AccountId = session.Character.AccountId,
                    Reason = "Auto Ban Potion Infinite Abuse PL",
                    Penalty = PenaltyType.Banned,
                    DateStart = DateTime.Now,
                    DateEnd = DateTime.Now.AddYears(1),
                    AdminName = "NosMoon System"
                };
                Character.InsertOrUpdatePenalty(log);
                session?.Disconnect();
                return;
            }
            Observable.Timer(TimeSpan.FromSeconds(10)).Subscribe(x =>
            {
                if (session?.Character?.PotionRequests > 0)
                {
                    session.Character.PotionRequests = 0;
                }
            });

            session.Character.LastPotion = DateTime.Now;

            switch (Effect)
            {
                default:
                    {
                        bool hasPotionBeenUsed = false;

                        int hpLoad = (int)session.Character.HPLoad();
                        int mpLoad = (int)session.Character.MPLoad();

                        if (session.Character.Hp > 0
                            && (session.Character.Hp < hpLoad || session.Character.Mp < mpLoad))
                        {
                            hasPotionBeenUsed = true;

                            double buffRc = session.Character.GetBuff(BCardType.CardType.LeonaPassiveSkill, (byte)AdditionalTypes.LeonaPassiveSkill.IncreaseRecoveryItems)[0] / 100D;

                            int hpAmount = Hp + (int)(Hp * buffRc);
                            int mpAmount = Mp + (int)(Mp * buffRc);

                            if (session.Character.Hp + hpAmount > hpLoad)
                            {
                                hpAmount = hpLoad - session.Character.Hp;
                            }

                            if (session.Character.Mp + mpAmount > mpLoad)
                            {
                                mpAmount = mpLoad - session.Character.Mp;
                            }

                            bool convertRecoveryToDamage = ServerManager.RandomNumber() < session.Character.GetBuff(BCardType.CardType.DarkCloneSummon, (byte)AdditionalTypes.DarkCloneSummon.ConvertRecoveryToDamage)[0];

                            if (convertRecoveryToDamage)
                            {
                                session.CurrentMapInstance.Broadcast(session.Character.GenerateDm(hpAmount));

                                session.Character.Hp -= hpAmount;

                                if (session.Character.Hp < 1)
                                {
                                    session.Character.Hp = 1;
                                }
                            }
                            else
                            {
                                session.CurrentMapInstance.Broadcast(session.Character.GenerateRc(hpAmount));

                                session.Character.Hp += hpAmount;
                            }

                            session.Character.Mp += mpAmount;

                            switch (inv.ItemVNum)
                            {
                                // Full HP Potion
                                case 1242:
                                case 5582:
                                    {
                                        if (convertRecoveryToDamage)
                                        {
                                            session.CurrentMapInstance.Broadcast(session.Character.GenerateDm(session.Character.Hp - 1));
                                            session.Character.Hp = 1;
                                        }
                                        else
                                        {
                                            session.CurrentMapInstance.Broadcast(session.Character.GenerateRc(hpLoad - session.Character.Hp));
                                            session.Character.Hp = hpLoad;
                                        }
                                    }
                                    break;

                                // Full MP Potion
                                case 1243:
                                case 5583:
                                    {
                                        session.Character.Mp = mpLoad;
                                    }
                                    break;

                                // Full HP & MP Potion
                                case 1244:
                                case 5584:
                                case 9129:
                                    {
                                        if (convertRecoveryToDamage)
                                        {
                                            session.CurrentMapInstance.Broadcast(session.Character.GenerateDm(session.Character.Hp - 1));
                                            session.Character.Hp = 1;
                                        }
                                        else
                                        {
                                            session.CurrentMapInstance.Broadcast(session.Character.GenerateRc(hpLoad - session.Character.Hp));
                                            session.Character.Hp = hpLoad;
                                        }

                                        session.Character.Mp = mpLoad;
                                    }
                                    break;
                            }

                            session.SendPacket(session.Character.GenerateStat());
                        }

                        foreach (Mate mate in session.Character.Mates.Where(s => s.IsTeamMember && s.IsAlive && !s.IsEgg))
                        {
                            hpLoad = (int)mate.MaxHp;
                            mpLoad = (int)mate.MaxMp;

                            if (mate.Hp <= 0 || (mate.Hp == hpLoad && mate.Mp == mpLoad))
                            {
                                continue;
                            }

                            hasPotionBeenUsed = true;

                            int hpAmount = Hp;
                            int mpAmount = Mp;

                            if (mate.Hp + hpAmount > hpLoad)
                            {
                                hpAmount = hpLoad - (int)mate.Hp;
                            }

                            if (mate.Mp + mpAmount > mpLoad)
                            {
                                mpAmount = mpLoad - (int)mate.Mp;
                            }

                            mate.Hp += hpAmount;
                            mate.Mp += mpAmount;

                            session.CurrentMapInstance.Broadcast(mate.GenerateRc(hpAmount));

                            switch (inv.ItemVNum)
                            {
                                // Full HP Potion
                                case 1242:
                                case 5582:
                                    session.CurrentMapInstance.Broadcast(mate.GenerateRc(hpLoad - (int)mate.Hp));
                                    mate.Hp = hpLoad;
                                    break;

                                // Full MP Potion
                                case 1243:
                                case 5583:
                                    mate.Mp = mpLoad;
                                    break;

                                // Full HP & MP Potion
                                case 1244:
                                case 5584:
                                case 9129:
                                    session.CurrentMapInstance.Broadcast(mate.GenerateRc(hpLoad - (int)mate.Hp));
                                    mate.Hp = hpLoad;
                                    mate.Mp = mpLoad;
                                    break;
                            }

                            session.SendPacket(mate.GenerateStatInfo());
                        }

                        if (session.Character.Mates.Any(m => m.IsTeamMember && m.IsAlive))
                        {
                            session.SendPackets(session.Character.GeneratePst());
                        }

                        if (hasPotionBeenUsed)
                        {
                            session.Character.Inventory.RemoveItemFromInventory(inv.Id);
                        }
                    }
                    break;
            }
        }

        #endregion
    }
}