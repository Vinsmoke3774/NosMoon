using NosByte.Shared;
using OpenNos.Core;
using OpenNos.Core.Logger;
using OpenNos.Data;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Battle;
using OpenNos.GameObject.Event;
using OpenNos.GameObject.Extension.CharacterExtensions;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;
using OpenNos.GameObject.RainbowBattle;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using static OpenNos.Domain.BCardType;
// ReSharper disable ConditionIsAlwaysTrueOrFalse

namespace OpenNos.Handler.SharedMethods
{
    public static class SharedBattleMethods
    {
        public static void PvpHit(this ClientSession session, HitRequest hitRequest, ClientSession target)
        {
            if (target?.Character.Hp > 0 && hitRequest?.Session.Character.Hp > 0)
            {
                if (target.Character.IsSitting)
                {
                    target.Character.Rest();
                }

                double cooldownReduction = session.Character.GetBuff(CardType.Morale, (byte)AdditionalTypes.Morale.SkillCooldownDecreased)[0] * -1 + session.Character.GetBuff(CardType.Casting, (byte)AdditionalTypes.Casting.EffectDurationIncreased)[0];

                int[] increaseEnemyCooldownChance = session.Character.GetBuff(CardType.DarkCloneSummon, (byte)AdditionalTypes.DarkCloneSummon.IncreaseEnemyCooldownChance);

                if (ServerManager.RandomNumber() < increaseEnemyCooldownChance[0])
                {
                    cooldownReduction += increaseEnemyCooldownChance[1];
                }

                int hitmode = 0;
                bool onyxWings = false;
                bool zephyrWings = false;
                BattleEntity battleEntity = new BattleEntity(hitRequest.Session.Character, hitRequest.Skill);
                BattleEntity battleEntityDefense = new BattleEntity(target.Character, null);
                int damage = DamageHelper.Instance.CalculateDamage(battleEntity, battleEntityDefense, hitRequest.Skill, ref hitmode, ref onyxWings, ref zephyrWings);

                if (target.Character.HasGodMode || target.Character.IsFrozen)
                {
                    damage = 0;
                    hitmode = 4;
                }
                else if (target.Character?.LastPVPRevive > DateTime.Now.AddSeconds(-5) || hitRequest?.Session?.Character.LastPVPRevive > DateTime.Now.AddSeconds(-5))
                {
                    target?.SendPacket(StaticPacketHelper.Cancel(2, target.SessionId));
                    hitRequest?.Session?.SendPacket(StaticPacketHelper.Cancel(2, hitRequest.Session.SessionId));
                    return;
                }

                if (ServerManager.RandomNumber() < target.Character.GetBuff(CardType.DarkCloneSummon,
                    (byte)AdditionalTypes.DarkCloneSummon.ConvertDamageToHPChance)[0])
                {
                    int amount = damage / 2;

                    target.Character.ConvertedDamageToHP += amount;
                    target.Character.MapInstance?.Broadcast(target.Character.GenerateRc(amount));
                    target.Character.Hp += amount;

                    if (target.Character.Hp > target.Character.HPLoad())
                    {
                        target.Character.Hp = (int)target.Character.HPLoad();
                    }

                    target.SendPacket(target.Character.GenerateStat());

                    damage = 0;
                }

                if (hitmode != 4 && hitmode != 2 && damage > 0)
                {
                    session.Character.RemoveBuffByBCardTypeSubType(new List<KeyValuePair<byte, byte>>
                    {
                        new((byte)CardType.SpecialActions, (byte)AdditionalTypes.SpecialActions.Hide)
                    });
                    target.Character.RemoveBuffByBCardTypeSubType(new List<KeyValuePair<byte, byte>>
                    {
                        new((byte)CardType.SpecialActions, (byte)AdditionalTypes.SpecialActions.Hide)
                    });
                    session.Character.RemoveBuffByBCardTypeSubType(new List<KeyValuePair<byte, byte>>
                    {
                        new((byte)CardType.FalconSkill, (byte)AdditionalTypes.FalconSkill.Hide)
                    });
                    target.Character.RemoveBuffByBCardTypeSubType(new List<KeyValuePair<byte, byte>>
                    {
                        new((byte)CardType.FalconSkill, (byte)AdditionalTypes.FalconSkill.Hide)
                    });
                    target.Character.RemoveBuff(36);
                    target.Character.RemoveBuff(548);
                }

                if (hitmode == 4 && target.Character.HasBuff(688))
                {
                    target.Character.RemoveBuff(688);
                    target.Character?.AddBuff(new Buff(689, target.Character.Level), target.Character.BattleEntity);
                }

                if (!SkillHelper.Instance.IsNoDamage(hitRequest.Skill.SkillVNum) && session.Character.Buff.FirstOrDefault(s => s.Card.BCards.Any(b => b.Type == (byte)CardType.FalconSkill && b.SubType.Equals((byte)AdditionalTypes.FalconSkill.Hide))) is Buff FalconHideBuff)
                {
                    session.Character.RemoveBuff(FalconHideBuff.Card.CardId);
                    session.Character.AddBuff(new Buff(560, session.Character.Level), session.Character.BattleEntity);
                }

                int[] manaShield = target.Character.GetBuff(CardType.LightAndShadow,
                    (byte)AdditionalTypes.LightAndShadow.InflictDamageToMP);
                if (manaShield[0] != 0 && hitmode != 4)
                {
                    int reduce = damage / 100 * manaShield[0];
                    if (target.Character.Mp < reduce)
                    {
                        reduce = target.Character.Mp;
                        target.Character.Mp = 0;
                    }
                    else
                    {
                        target.Character.DecreaseMp(reduce);
                    }
                    damage -= reduce;
                }

                if (onyxWings && hitmode != 4 && hitmode != 2)
                {
                    short onyxX = (short)(hitRequest.Session.Character.PositionX + 2);
                    short onyxY = (short)(hitRequest.Session.Character.PositionY + 2);
                    int onyxId = target.CurrentMapInstance.GetNextMonsterId();
                    MapMonster onyx = new MapMonster
                    {
                        MonsterVNum = 2371,
                        MapX = onyxX,
                        MapY = onyxY,
                        MapMonsterId = onyxId,
                        IsHostile = false,
                        IsMoving = false,
                        ShouldRespawn = false
                    };
                    target?.CurrentMapInstance?.Broadcast(UserInterfaceHelper.GenerateGuri(31, 1,
                        hitRequest.Session.Character.CharacterId, onyxX, onyxY));
                    onyx.Initialize(target.CurrentMapInstance);
                    target.CurrentMapInstance.AddMonster(onyx);
                    target.CurrentMapInstance.Broadcast(onyx.GenerateIn());
                    target.Character.GetDamage((int)(damage / 2D), battleEntity);
                    Observable.Timer(TimeSpan.FromMilliseconds(350)).SafeSubscribe(o =>
                    {
                        if (target == null)
                        {
                            return;
                        }

                        target?.CurrentMapInstance?.Broadcast(StaticPacketHelper.SkillUsed(UserType.Monster, onyxId, 1,
                            target.Character.CharacterId, -1, 0, -1, hitRequest.Skill.Effect, -1, -1, true, 92,
                            (int)(damage / 2D), 0, 0));
                        target?.CurrentMapInstance?.RemoveMonster(onyx);
                        target?.CurrentMapInstance?.Broadcast(StaticPacketHelper.Out(UserType.Monster,
                            onyx.MapMonsterId));
                    });
                }
                if (zephyrWings && hitmode != 1)
                {
                    target.Character.GetDamage(damage / 4, battleEntity);
                    var damage1 = damage;
                    target.CurrentMapInstance.Broadcast(StaticPacketHelper.SkillUsed(UserType.Player, hitRequest.Session.Character.CharacterId, 1,
                        target.Character.CharacterId, -1, 0, -1, 4211, -1, -1, true, 92, damage1 / 4, 0, 1));
                }

                if (target.Character.GetBuff(BCardType.CardType.TauntSkill, (byte)AdditionalTypes.TauntSkill.ReflectsMaximumDamageFromNegated)[0] > 0)
                {
                    hitRequest.Session.Character.GetDamage(damage / 2, new BattleEntity(target.Character, null), true);
                    hitRequest.Session.SendPacket($"bf 1 {hitRequest.Session.Character.CharacterId} 0.0.0 {hitRequest.Session.Character.Level}");
                    hitRequest.Session.Character.LastDefence = DateTime.Now;
                    target.Character.LastDefence = DateTime.Now;
                    target.CurrentMapInstance.Broadcast(StaticPacketHelper.SkillUsed(UserType.Player, target.Character.CharacterId, 1,
                        hitRequest.Session.Character.CharacterId, -1, 0, -1, hitRequest.Skill.Effect, -1, -1, true, 92,
                        damage, 0, 0));
                    hitRequest.Session.SendPacket(target.Character.GenerateStat());
                    damage = 0;
                }

                target.Character.GetDamage(damage / 2, battleEntity);
                target.SendPacket(target.Character.GenerateStat());

                // Magical Fetters

                if (damage > 0)
                {
                    if (target.Character.HasMagicalFetters)
                    {
                        // Magic Spell

                        target.Character.AddBuff(new Buff(617, target.Character.Level), target.Character.BattleEntity);

                        int castId = 10 + session.Character.Element;

                        if (castId == 10)
                        {
                            castId += 5; // No element
                        }

                        target.Character.LastComboCastId = castId;
                        target.SendPacket($"mslot {castId} -1");
                    }
                }

                bool isAlive = target.Character.Hp > 0;
                if (!isAlive && target.HasCurrentMapInstance)
                {
                    if (target.Character.IsVehicled)
                    {
                        target.Character.RemoveVehicle();
                    }

                    if (hitRequest.Session.Character != null && hitRequest.SkillBCards.FirstOrDefault(s => s.Type == (byte)CardType.TauntSkill && s.SubType == (byte)AdditionalTypes.TauntSkill.EffectOnKill) is BCard EffectOnKill && ServerManager.RandomNumber() < EffectOnKill.FirstData)
                    {
                        hitRequest.Session.Character.AddBuff(new Buff((short)EffectOnKill.SecondData, hitRequest.Session.Character.Level), hitRequest.Session.Character.BattleEntity);
                    }

                    target.Character.LastPvPKiller = session;
                    if (target.CurrentMapInstance.Map?.MapTypes.Any(s => s.MapTypeId == (short)MapTypeEnum.Act4) == true)
                    {
                        if (ServerManager.Instance.ChannelId == 51 && ServerManager.Instance.Act4DemonStat.Mode == 0 && ServerManager.Instance.Act4AngelStat.Mode == 0)
                        {
                            ServerManager.Instance.IncreaseFcPercentage(100, session.Character.Faction);
                        }

                        hitRequest.Session.Character.LastKilledChars.Add(target.Character.CharacterId);
                        var hits = hitRequest.Session.Character.LastKilledChars.Count(s => s == target.Character.CharacterId);

                        if (hitRequest.Session.Character.LastKilledChars.Count > 6)
                        {
                            hitRequest.Session.Character.LastKilledChars.RemoveAt(0);
                        }

                        if (target.Character.Session.CleanIpAddress != hitRequest.Session.CleanIpAddress && hits < 3)
                        {
                            var targetLevel = target.Character.Level + target.Character.HeroLevel;
                            var attackerLevel = hitRequest.Session.Character.Level + hitRequest.Session.Character.HeroLevel;
                            int levelDifference = Math.Abs(targetLevel - attackerLevel);
                            var repValue = 0;
                            hitRequest.Session.Character.Act4Kill++;

                            switch (hitRequest.Session.Character.Act4Kill)
                            {
                                case 250:
                                    if (hitRequest.Session.Character.Inventory.Any(i => i.Value.ItemVNum == 9403))
                                    {
                                        return;
                                    }

                                    hitRequest.Session.Character.GiftAdd(9403, 1);
                                    break;
                                case 500:
                                    if (hitRequest.Session.Character.Inventory.Any(i => i.Value.ItemVNum == 9402))
                                    {
                                        return;
                                    }

                                    hitRequest.Session.Character.GiftAdd(9402, 1);
                                    break;
                                case 1000:
                                    if (hitRequest.Session.Character.Inventory.Any(i => i.Value.ItemVNum == 9401))
                                    {
                                        return;
                                    }

                                    hitRequest.Session.Character.GiftAdd(9401, 1);
                                    break;
                            }

                            target.Character.Act4Dead++;
                            target.Character.Act4Points--;

                            if (levelDifference > 5 && levelDifference <= 10)
                            {
                                repValue = targetLevel * 100; // for a 99+60 hitting a 99+50, it results in a 15.9k rep kill
                                hitRequest.Session.Character.Act4Points += 1;
                            }
                            else if (levelDifference <= 5)
                            {
                                repValue = targetLevel * 200; // here it results in a 31.8k kill
                                hitRequest.Session.Character.Act4Points += 2;
                            }

                            if (repValue > 0)
                            {
                                hitRequest.Session.Character.Reputation += repValue;
                                hitRequest.Session.SendPacket(hitRequest.Session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("WIN_REPUT"), (short)repValue), 12));
                                hitRequest.Session.SendPacket(hitRequest.Session.Character.GenerateLev());
                            }
                            else
                            {
                                hitRequest.Session.SendPacket(hitRequest.Session.Character.GenerateSay("The level difference is too big, you don't win any reputation.", 12));
                            }
                        }
                        else
                        {
                            if (target.Character.Session.CleanIpAddress == hitRequest.Session.CleanIpAddress)
                            {
                                hitRequest.Session.SendPacket(hitRequest.Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("TARGET_SAME_IP"), 11));
                                hitRequest.Session.Character.Act4Points--;
                                hitRequest.Session.Character.Act4Kill--;
                                hitRequest.Session.Character.Reputation -= 10000;
                                hitRequest.Session.SendPacket(hitRequest.Session.Character.GenerateFd());
                            }
                            else
                            {
                                hitRequest.Session.SendPacket(hitRequest.Session.Character.GenerateSay("You killed this target multiple times already.", 11));
                            }
                        }

                        foreach (ClientSession sess in ServerManager.Instance.Sessions.Where(
                            s => s.HasSelectedCharacter))
                        {
                            if (sess.Character.Faction == session.Character.Faction)
                            {
                                sess.SendPacket(sess.Character.GenerateSay(
                                    string.Format(
                                        Language.Instance.GetMessageFromKey(
                                            $"ACT4_PVP_KILL{(int)target.Character.Faction}"), session.Character.Name),
                                    12));
                            }
                            else if (sess.Character.Faction == target.Character.Faction)
                            {
                                sess.SendPacket(sess.Character.GenerateSay(
                                    string.Format(
                                        Language.Instance.GetMessageFromKey(
                                            $"ACT4_PVP_DEATH{(int)target.Character.Faction}"), target.Character.Name),
                                    11));
                            }

                            var quest = ServerManager.Instance.BattlePassQuests.Find(s => s.MissionSubType == BattlePassMissionSubType.KillPlayersInGlaceron);
                            if (quest != null)
                            {
                                sess.Character.IncreaseBattlePassQuestObjectives(quest.Id, 1);
                            }
                        }

                        target.SendPacket(target.Character.GenerateFd());
                        target.CurrentMapInstance?.Broadcast(target, target.Character.GenerateIn(broadcastEffect: 1), ReceiverType.AllExceptMe);
                        target.CurrentMapInstance?.Broadcast(target, target.Character.GenerateGidx(), ReceiverType.AllExceptMe);
                        hitRequest.Session.SendPacket(hitRequest.Session.Character.GenerateFd());
                        hitRequest.Session.CurrentMapInstance?.Broadcast(hitRequest.Session, hitRequest.Session.Character.GenerateIn(broadcastEffect: 1), ReceiverType.AllExceptMe);
                        hitRequest.Session.CurrentMapInstance?.Broadcast(hitRequest.Session, hitRequest.Session.Character.GenerateGidx(), ReceiverType.AllExceptMe);
                        target.Character.DisableBuffs(BuffType.All);

                        if (target.Character.MapInstance == CaligorRaid.CaligorMapInstance)
                        {
                            ServerManager.Instance.AskRevive(target.Character.CharacterId);
                        }
                        else
                        {
                            target.SendPacket(
                                target.Character.GenerateSay(Language.Instance.GetMessageFromKey("ACT4_PVP_DIE"), 11));
                            target.SendPacket(
                                UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("ACT4_PVP_DIE"), 0));

                            if (target == null)
                            {
                                return;
                            }

                            target.Character.SetSeal();
                        }
                    }
                    else
                    {
                        switch (target.CurrentMapInstance.MapInstanceType)
                        {
                            case MapInstanceType.DeathMatch:
                                IDisposable obs2 = null;
                                obs2 = Observable.Interval(TimeSpan.FromSeconds(1)).SafeSubscribe(s =>
                                {
                                    if (target?.Character == null)
                                    {
                                        obs2?.Dispose();
                                        return;
                                    }

                                    if (!target.Character.IsFrozen)
                                    {
                                        obs2.Dispose();
                                        return;
                                    }

                                    target.CurrentMapInstance?.Broadcast(target.Character?.GenerateEff(35));
                                });

                                target.Character.IsFrozen = true;

                                session.Character.DeathMatchMember.Kills += 1;
                                session.SendPacket(session.Character.GenerateSay("-----------------DeathMatch Information-----------------", 11));
                                session.SendPacket(session.Character.GenerateSay($"Player with the most kills: {session.CurrentMapInstance.Sessions.Select(s => s.Character.DeathMatchMember).OrderByDescending(s => s.Kills).First().Name}", 12));
                                session.SendPacket(session.Character.GenerateSay($"You have {session.Character.DeathMatchMember.Kills}/30 kills", 12));
                                session.SendPacket(session.Character.GenerateSay("-----------------END-----------------", 11));

                                Observable.Timer(TimeSpan.FromSeconds(10)).SafeSubscribe(s =>
                                {
                                    target.Character.Hp = (int)target.Character.HPLoad();
                                    target.Character.Mp = (int)target.Character.MPLoad();
                                    target.SendPacket(target.Character.GenerateStat());
                                    Observable.Timer(TimeSpan.FromSeconds(1)).SafeSubscribe(s =>
                                    {
                                        target.SendPacket(target.Character.GenerateRevive());
                                        ServerManager.Instance.TeleportOnRandomPlaceInMap(target, target.CurrentMapInstance.MapInstanceId);
                                    });
                                    target.Character.IsFrozen = false;
                                });
                                break;

                            case MapInstanceType.OneVersusOne:
                                target.Character.OneVersusOneBattle.IsDead = true;
                                break;

                            case MapInstanceType.TwoVersusTwo:
                                IDisposable obs1 = null;
                                obs1 = Observable.Interval(TimeSpan.FromSeconds(1)).SafeSubscribe(s =>
                                {
                                    if (target?.Character == null)
                                    {
                                        obs1?.Dispose();
                                        return;
                                    }

                                    if (!target.Character.IsFrozen)
                                    {
                                        obs1.Dispose();
                                        return;
                                    }

                                    target.CurrentMapInstance?.Broadcast(target.Character?.GenerateEff(35));
                                });

                                target.Character.Hp = (int)target.Character.HPLoad();
                                target.Character.Mp = (int)target.Character.MPLoad();
                                target.SendPacket(target.Character.GenerateStat());
                                target.Character.NoMove = true;
                                target.Character.NoAttack = true;
                                target.Character.IsFrozen = true;
                                target.SendPacket(target.Character.GenerateCond());
                                target.Character.TwoVersusTwoBattle.IsDead = true;
                                break;

                            case MapInstanceType.TalentArenaMapInstance:
                                hitRequest.Session.Character.BattleEntity.ApplyTalentArenaScore(target.Character.BattleEntity);
                                break;

                            case MapInstanceType.RainbowBattleInstance:
                                {
                                    var rbb = ServerManager.Instance.RainbowBattleMembers.Find(s => s.Session.Contains(target));
                                    IDisposable obs = null;
                                    obs = Observable.Interval(TimeSpan.FromSeconds(1)).SafeSubscribe(s =>
                                    {
                                        if (session?.Character == null || target == null)
                                        {
                                            obs?.Dispose();
                                            return;
                                        }

                                        target.CurrentMapInstance?.Broadcast(target.Character?.GenerateEff(35));
                                    });

                                    RainbowBattleManager.SendFbs(target.CurrentMapInstance);

                                    isAlive = true;
                                    hitRequest.Session.CurrentMapInstance?.Broadcast((UserInterfaceHelper.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("RAINBOW_KILL"),
                                        hitRequest.Session.Character.Name, target.Character.Name), 0)));
                                    target.CurrentMapInstance?.Broadcast(session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("RESP_RBB"), target.Character.Name), 10));

                                    target.Character.Hp = (int)target.Character.HPLoad();
                                    target.Character.Mp = (int)target.Character.MPLoad();
                                    target.SendPacket(target?.Character?.GenerateStat());
                                    target.Character.NoMove = true;
                                    target.Character.NoAttack = true;
                                    target.Character.IsFrozen = true;
                                    target.SendPacket(target?.Character?.GenerateCond());
                                    Observable.Timer(TimeSpan.FromSeconds(20)).SafeSubscribe(o =>
                                    {
                                        if (target?.Character == null)
                                        {
                                            obs?.Dispose();
                                            return;
                                        }

                                        if (target.Character.IsFrozen)
                                        {
                                            target.Character.PositionX = rbb.TeamEntity == RainbowTeamBattleType.Red ? ServerManager.RandomNumber<short>(30, 32) : ServerManager.RandomNumber<short>(83, 85);
                                            target.Character.PositionY = rbb.TeamEntity == RainbowTeamBattleType.Red ? ServerManager.RandomNumber<short>(73, 76) : ServerManager.RandomNumber<short>(2, 4);
                                            target?.CurrentMapInstance?.Broadcast(target.Character.GenerateTp());
                                            target.Character.NoAttack = false;
                                            target.Character.NoMove = false;
                                            target.Character.IsFrozen = false;
                                            target?.SendPacket(target.Character.GenerateCond());
                                        }
                                        obs?.Dispose();
                                    });
                                    break;
                                }
                            default:
                                hitRequest.Session.Character.BattleEntity.ApplyScoreArena(target.Character.BattleEntity);
                                hitRequest.Session.CurrentMapInstance?.Broadcast(session.Character.GenerateSay(
                                    string.Format(Language.Instance.GetMessageFromKey("PVP_KILL"),
                                        hitRequest.Session.Character.Name, target.Character.Name), 10));
                                hitRequest.Session.CurrentMapInstance?.Broadcast((UserInterfaceHelper.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("PVP_KILL"),
                                    hitRequest.Session.Character.Name, target.Character.Name), 0)));
                                {
                                    target.Character.RemoveVehicle();
                                }
                                Observable.Timer(TimeSpan.FromMilliseconds(1000)).SafeSubscribe(o =>
                                {
                                    if (target == null)
                                    {
                                        return;
                                    }

                                    ServerManager.Instance.AskPvpRevive(target.Character.CharacterId);
                                });
                                break;
                        }
                    }
                }

                battleEntity.BCards.Where(s => s.CastType == 1).ForEach(s =>
                {
                    if (s.Type != (byte)CardType.Buff)
                    {
                        s.ApplyBCards(target.Character.BattleEntity, session.Character.BattleEntity);
                    }
                });

                hitRequest.SkillBCards.Where(s => !s.Type.Equals((byte)CardType.Buff) && !s.Type.Equals((byte)CardType.Capture) && s.CardId == null).ToList()
                    .ForEach(s => s.ApplyBCards(target.Character.BattleEntity, session.Character.BattleEntity));

                if (hitmode != 4 && hitmode != 2)
                {
                    battleEntity.BCards.Where(s => s.CastType == 1).ForEach(s =>
                    {
                        if (s.Type == (byte)CardType.Buff)
                        {
                            Buff b = new Buff((short)s.SecondData, battleEntity.Level);
                            if (b.Card != null)
                            {
                                switch (b.Card?.BuffType)
                                {
                                    case BuffType.Bad:
                                        s.ApplyBCards(target.Character.BattleEntity, session.Character.BattleEntity);
                                        break;

                                    case BuffType.Good:
                                    case BuffType.Neutral:
                                        s.ApplyBCards(session.Character.BattleEntity, session.Character.BattleEntity);
                                        break;
                                }
                            }
                        }
                    });

                    battleEntityDefense.BCards.Where(s => s.CastType == 0).ForEach(s =>
                    {
                        if (s.Type == (byte)CardType.Buff)
                        {
                            Buff b = new Buff((short)s.SecondData, battleEntityDefense.Level);
                            if (b.Card != null)
                            {
                                switch (b.Card?.BuffType)
                                {
                                    case BuffType.Bad:
                                        s.ApplyBCards(session.Character.BattleEntity, target.Character.BattleEntity);
                                        break;

                                    case BuffType.Good:
                                    case BuffType.Neutral:
                                        s.ApplyBCards(target.Character.BattleEntity, target.Character.BattleEntity);
                                        break;
                                }
                            }
                        }
                    });

                    hitRequest.SkillBCards.Where(s => s.Type.Equals((byte)CardType.Buff) && new Buff((short)s.SecondData, session.Character.Level).Card?.BuffType == BuffType.Bad).ToList()
                        .ForEach(s => s.ApplyBCards(target.Character.BattleEntity, session.Character.BattleEntity));

                    hitRequest.SkillBCards.Where(s => s.Type.Equals((byte)CardType.SniperAttack)).ToList()
                        .ForEach(s => s.ApplyBCards(target.Character.BattleEntity, session.Character.BattleEntity));

                    if (battleEntity?.ShellWeaponEffects != null)
                    {
                        var applicableShellDebuffs = new[]
                        {
                            ShellWeaponEffectType.Blackout, ShellWeaponEffectType.DeadlyBlackout,
                            ShellWeaponEffectType.MinorBleeding, ShellWeaponEffectType.Bleeding,
                            ShellWeaponEffectType.HeavyBleeding, ShellWeaponEffectType.Freeze
                        };

                        var debuffs = battleEntity?.ShellWeaponEffects.Where(s => applicableShellDebuffs.Contains((ShellWeaponEffectType)s.Effect));

                        foreach (var effect in debuffs)
                        {
                            if (ServerManager.RandomNumber(0, 250) <= effect.Value)
                            {
                                if (!target.Character.HasBuff(CardType.SpecialAttack, (byte)AdditionalTypes.SpecialAttack.NoAttack))
                                {
                                    switch (effect.Effect)
                                    {
                                        case (byte)ShellWeaponEffectType.Blackout:
                                            target.Character.AddBuff(new Buff(7, battleEntity.Level), battleEntity);
                                            break;

                                        case (byte)ShellWeaponEffectType.DeadlyBlackout:
                                            target.Character.AddBuff(new Buff(66, battleEntity.Level), battleEntity);
                                            break;

                                        case (byte)ShellWeaponEffectType.MinorBleeding:
                                            target.Character.AddBuff(new Buff(1, battleEntity.Level), battleEntity);
                                            break;

                                        case (byte)ShellWeaponEffectType.Bleeding:
                                            target.Character.AddBuff(new Buff(21, battleEntity.Level), battleEntity);
                                            break;

                                        case (byte)ShellWeaponEffectType.HeavyBleeding:
                                            target.Character.AddBuff(new Buff(42, battleEntity.Level), battleEntity);
                                            break;

                                        case (byte)ShellWeaponEffectType.Freeze:
                                            target.Character.AddBuff(new Buff(27, battleEntity.Level), battleEntity);
                                            break;

                                    }
                                }
                            }
                        }
                    }
                }

                if (hitmode != 2)
                {
                    switch (hitRequest.TargetHitType)
                    {
                        case TargetHitType.SingleTargetHit:

                            if (hitRequest.Session.Character != null)
                            {
                                if (hitRequest.Skill.SkillVNum == 1607)
                                {
                                    hitRequest.Session.Character.PositionX = target.Character.PositionX;
                                    hitRequest.Session.Character.PositionY = target.Character.PositionY;
                                    hitRequest.Session.CurrentMapInstance.Broadcast(hitRequest.Session.Character.GenerateTp());
                                }
                            }

                            hitRequest.Session.CurrentMapInstance?.Broadcast(StaticPacketHelper.SkillUsed(
                                UserType.Player,
                                hitRequest.Session.Character.CharacterId, 1, target.Character.CharacterId,
                                hitRequest.Skill.SkillVNum,
                                (short)(hitRequest.Skill.Cooldown - (hitRequest.Skill.Cooldown * (cooldownReduction / 100D))),
                                hitRequest.Skill.AttackAnimation,
                                hitRequest.SkillEffect, hitRequest.Session.Character.PositionX,
                                hitRequest.Session.Character.PositionY, isAlive,
                                (int)(target.Character.Hp / (float)target.Character.HPLoad() * 100), damage, hitmode,
                                (byte)(hitRequest.Skill.SkillType - 1)));
                            break;

                        case TargetHitType.SingleTargetHitCombo:
                            hitRequest.Session.CurrentMapInstance?.Broadcast(StaticPacketHelper.SkillUsed(
                                UserType.Player,
                                hitRequest.Session.Character.CharacterId, 1, target.Character.CharacterId,
                                hitRequest.Skill.SkillVNum,
                                (short)(hitRequest.Skill.Cooldown - (hitRequest.Skill.Cooldown * (cooldownReduction / 100D))),
                                hitRequest.SkillCombo.Animation,
                                hitRequest.SkillCombo.Effect, hitRequest.Session.Character.PositionX,
                                hitRequest.Session.Character.PositionY, isAlive,
                                (int)(target.Character.Hp / (float)target.Character.HPLoad() * 100), damage, hitmode,
                                (byte)(hitRequest.Skill.SkillType - 1)));
                            break;

                        case TargetHitType.SingleAOETargetHit:
                            if (hitRequest.ShowTargetHitAnimation)
                            {
                                if (hitRequest.Skill.SkillVNum == 1617 || hitRequest.Skill.SkillVNum == 1085 || hitRequest.Skill.SkillVNum == 1091 ||
                                    hitRequest.Skill.SkillVNum == 1060 || hitRequest.Skill.SkillVNum == 718)
                                {
                                    hitRequest.Session.Character.PositionX = target.Character.PositionX;
                                    hitRequest.Session.Character.PositionY = target.Character.PositionY;
                                    hitRequest.Session.CurrentMapInstance.Broadcast(hitRequest.Session.Character.GenerateTp());
                                }

                                hitRequest.Session.CurrentMapInstance?.Broadcast(StaticPacketHelper.SkillUsed(
                                    UserType.Player, hitRequest.Session.Character.CharacterId, 1,
                                    target.Character.CharacterId,
                                    hitRequest.Skill.SkillVNum,
                                    (short)(hitRequest.Skill.Cooldown - (hitRequest.Skill.Cooldown * (cooldownReduction / 100D))),
                                    hitRequest.Skill.AttackAnimation, hitRequest.SkillEffect,
                                    hitRequest.Session.Character.PositionX, hitRequest.Session.Character.PositionY,
                                    isAlive,
                                    (int)(target.Character.Hp / (float)target.Character.HPLoad() * 100), damage,
                                    hitmode,
                                    (byte)(hitRequest.Skill.SkillType - 1)));
                            }
                            else
                            {
                                switch (hitmode)
                                {
                                    case 1:
                                    case 4:
                                        hitmode = 7;
                                        break;

                                    case 2:
                                        hitmode = 2;
                                        break;

                                    case 3:
                                        hitmode = 6;
                                        break;

                                    default:
                                        hitmode = 5;
                                        break;
                                }

                                hitRequest.Session.CurrentMapInstance?.Broadcast(StaticPacketHelper.SkillUsed(
                                    UserType.Player, hitRequest.Session.Character.CharacterId, 1,
                                    target.Character.CharacterId,
                                    -1,
                                    (short)(hitRequest.Skill.Cooldown - (hitRequest.Skill.Cooldown * (cooldownReduction / 100D))),
                                    hitRequest.Skill.AttackAnimation, hitRequest.SkillEffect,
                                    hitRequest.Session.Character.PositionX, hitRequest.Session.Character.PositionY,
                                    isAlive,
                                    (int)(target.Character.Hp / (float)target.Character.HPLoad() * 100), damage,
                                    hitmode,
                                    (byte)(hitRequest.Skill.SkillType - 1)));
                            }

                            break;

                        case TargetHitType.AOETargetHit:
                            switch (hitmode)
                            {
                                case 1:
                                case 4:
                                    hitmode = 7;
                                    break;

                                case 2:
                                    hitmode = 2;
                                    break;

                                case 3:
                                    hitmode = 6;
                                    break;

                                default:
                                    hitmode = 5;
                                    break;
                            }

                            hitRequest.Session.CurrentMapInstance?.Broadcast(StaticPacketHelper.SkillUsed(
                                UserType.Player,
                                hitRequest.Session.Character.CharacterId, 1, target.Character.CharacterId,
                                hitRequest.Skill.SkillVNum,
                                (short)(hitRequest.Skill.Cooldown - (hitRequest.Skill.Cooldown * (cooldownReduction / 100D))),
                                hitRequest.Skill.AttackAnimation,
                                hitRequest.SkillEffect, hitRequest.Session.Character.PositionX,
                                hitRequest.Session.Character.PositionY, isAlive,
                                (int)(target.Character.Hp / (float)target.Character.HPLoad() * 100), damage, hitmode,
                                (byte)(hitRequest.Skill.SkillType - 1)));
                            break;

                        case TargetHitType.ZoneHit:
                            hitRequest.Session.CurrentMapInstance?.Broadcast(StaticPacketHelper.SkillUsed(
                                UserType.Player,
                                hitRequest.Session.Character.CharacterId, 1, target.Character.CharacterId,
                                hitRequest.Skill.SkillVNum,
                                (short)(hitRequest.Skill.Cooldown - (hitRequest.Skill.Cooldown * (cooldownReduction / 100D))),
                                hitRequest.Skill.AttackAnimation,
                                hitRequest.SkillEffect, hitRequest.MapX, hitRequest.MapY, isAlive,
                                (int)(target.Character.Hp / (float)target.Character.HPLoad() * 100), damage, hitmode,
                                (byte)(hitRequest.Skill.SkillType - 1)));
                            break;

                        case TargetHitType.SpecialZoneHit:
                            hitRequest.Session.CurrentMapInstance?.Broadcast(StaticPacketHelper.SkillUsed(
                                UserType.Player,
                                hitRequest.Session.Character.CharacterId, 1, target.Character.CharacterId,
                                hitRequest.Skill.SkillVNum,
                                (short)(hitRequest.Skill.Cooldown - (hitRequest.Skill.Cooldown * (cooldownReduction / 100D))),
                                hitRequest.Skill.AttackAnimation,
                                hitRequest.SkillEffect, hitRequest.Session.Character.PositionX,
                                hitRequest.Session.Character.PositionY, isAlive,
                                (int)(target.Character.Hp / target.Character.HPLoad() * 100), damage, hitmode,
                                (byte)(hitRequest.Skill.SkillType - 1)));
                            break;

                        default:
                            Logger.Log.Warn("Not Implemented TargetHitType Handling!");
                            break;
                    }
                }
                else
                {
                    if (target != null)
                    {
                        hitRequest?.Session.SendPacket(StaticPacketHelper.Cancel(2, target.Character.CharacterId));
                    }
                }
            }
            else
            {
                // monster already has been killed, send cancel
                if (target != null)
                {
                    hitRequest?.Session.SendPacket(StaticPacketHelper.Cancel(2, target.Character.CharacterId));
                }
            }
        }

        public static void TargetHit(this ClientSession session, int castingId, UserType targetType, int targetId, bool isPvp = false)
        {
            // O gods of software development and operations, I have sinned.

            bool shouldCancel = true;
            bool isSacrificeSkill = false;

            if ((DateTime.Now - session.Character.LastTransform).TotalSeconds < 3)
            {
                session.SendPacket(StaticPacketHelper.Cancel());
                session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("CANT_ATTACK"), 0));
                return;
            }

            List<CharacterSkill> skills = session.Character.GetSkills();

            if (skills != null)
            {
                CharacterSkill ski = skills.FirstOrDefault(s => s.Skill?.CastId == castingId && (s.Skill?.UpgradeSkill == 0 || s.Skill?.SkillType == 1));

                if (ski != null)
                {
                    if (!session.Character.WeaponLoaded(ski) || !ski.CanBeUsed())
                    {
                        session.SendPacket(StaticPacketHelper.Cancel(2, targetId));
                        return;
                    }

                    //TODO: Clean it up issou
                    if (ski.SkillVNum == 656)
                    {
                        session.Character.RemoveUltimatePoints(2000);
                    }
                    else if (ski.SkillVNum == 657)
                    {
                        session.Character.RemoveUltimatePoints(1000);
                    }
                    else if (ski.SkillVNum == 658 || ski.SkillVNum == 659)
                    {
                        session.Character.RemoveUltimatePoints(3000);
                    }

                    if (session.Character.LastSkillComboUse > DateTime.Now && ski.SkillVNum != SkillHelper.GetOriginalSkill(ski.Skill)?.SkillVNum)
                    {
                        session.SendPacket(StaticPacketHelper.Cancel(2, targetId));
                        return;
                    }

                    BattleEntity targetEntity = null;

                    switch (targetType)
                    {
                        case UserType.Player:
                            {
                                targetEntity = ServerManager.Instance.GetSessionByCharacterId(targetId)?.Character?.BattleEntity;
                            }
                            break;

                        case UserType.Npc:
                            {
                                targetEntity = session.Character.MapInstance?.Npcs?.ToList().FirstOrDefault(n => n.MapNpcId == targetId)?.BattleEntity
                                    ?? session.Character.MapInstance?.Sessions?.Where(s => s?.Character?.Mates != null).SelectMany(s => s.Character.Mates.ToList()).FirstOrDefault(m => m.MateTransportId == targetId)?.BattleEntity;
                            }
                            break;

                        case UserType.Monster:
                            {
                                targetEntity = session.Character.MapInstance?.Monsters?.ToList().FirstOrDefault(m => m.Owner?.Character == null && m.MapMonsterId == targetId)?.BattleEntity;
                            }
                            break;
                    }

                    if (targetEntity == null)
                    {
                        session.SendPacket(StaticPacketHelper.Cancel(2));
                        return;
                    }

                    foreach (BCard bc in ski.GetSkillBCards().ToList().Where(s => s.Type.Equals((byte)CardType.MeditationSkill)
                        && (!s.SubType.Equals((byte)AdditionalTypes.MeditationSkill.CausingChance) || SkillHelper.IsCausingChance(ski.SkillVNum))))
                    {
                        shouldCancel = false;

                        if (bc.SubType.Equals((byte)AdditionalTypes.MeditationSkill.Sacrifice))
                        {
                            isSacrificeSkill = true;
                            if (targetEntity == session.Character.BattleEntity || targetEntity.MapMonster != null || targetEntity.MapNpc != null)
                            {
                                session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("INVALID_TARGET"), 0));
                                session.SendPacket(StaticPacketHelper.Cancel(2, targetId));
                                return;
                            }
                        }

                        bc.ApplyBCards(session.Character.BattleEntity, session.Character.BattleEntity);
                    }

                    if (ski.Skill.SkillVNum == 1098 && ski.GetSkillBCards().FirstOrDefault(s => s.Type.Equals((byte)CardType.SpecialisationBuffResistance) && s.SubType.Equals((byte)AdditionalTypes.SpecialisationBuffResistance.RemoveBadEffects)) is BCard RemoveBadEffectsBcard)
                    {
                        if (session.Character.BattleEntity.BCardDisposables[RemoveBadEffectsBcard.BCardId] != null)
                        {
                            session.SendPacket(StaticPacketHelper.SkillResetWithCoolDown(castingId, 300));
                            ski.LastUse = DateTime.Now.AddSeconds(29);
                            Observable.Timer(TimeSpan.FromSeconds(30)).SafeSubscribe(o =>
                            {
                                if (session?.Character == null)
                                {
                                    return;
                                }

                                CharacterSkill skill = session.Character.GetSkills().Find(s => s.Skill?.CastId == castingId && (s.Skill?.UpgradeSkill == 0 || s.Skill?.SkillType == 1));
                                if (skill != null && skill.LastUse <= DateTime.Now)
                                {
                                    session.SendPacket(StaticPacketHelper.SkillReset(castingId));
                                }
                            });
                            RemoveBadEffectsBcard.ApplyBCards(session.Character.BattleEntity, session.Character.BattleEntity);
                            session.SendPacket(StaticPacketHelper.Cancel(2, targetId));
                            return;
                        }
                    }

                    double cooldownReduction = session.Character.GetBuff(CardType.Morale, (byte)AdditionalTypes.Morale.SkillCooldownDecreased)[0] * -1 + session.Character.GetBuff(CardType.Casting, (byte)AdditionalTypes.Casting.EffectDurationIncreased)[0];

                    int[] increaseEnemyCooldownChance = session.Character.GetBuff(CardType.DarkCloneSummon, (byte)AdditionalTypes.DarkCloneSummon.IncreaseEnemyCooldownChance);

                    if (ServerManager.RandomNumber() < increaseEnemyCooldownChance[0])
                    {
                        cooldownReduction += increaseEnemyCooldownChance[1];
                    }

                    if (ski.GetSkillBCards().Any(s => s.Type == (byte)CardType.Morale && s.SubType == ((byte)AdditionalTypes.Morale.SkillCooldownCancelled)))
                    {
                        cooldownReduction = 0;
                    }

                    short mpCost = ski.MpCost();
                    short hpCost = 0;

                    mpCost = (short)(mpCost * ((100 - session.Character.CellonOptions.Where(s => s.Type == CellonOptionType.MPUsage).Sum(s => s.Value)) / 100D));

                    if (session.Character.GetBuff(CardType.HealingBurningAndCasting, (byte)AdditionalTypes.HealingBurningAndCasting.HPDecreasedByConsumingMP)[0] is int HPDecreasedByConsumingMP)
                    {
                        if (HPDecreasedByConsumingMP < 0)
                        {
                            int amountDecreased = -(ski.MpCost() * HPDecreasedByConsumingMP / 100) * -1;
                            hpCost = (short)amountDecreased;
                            mpCost -= (short)amountDecreased;
                        }
                    }

                    if (session.Character.Mp >= mpCost && session.Character.Hp > hpCost && session.HasCurrentMapInstance)
                    {
                        if (!session.Character.HasGodMode)
                        {
                            session.Character.DecreaseMp(ski.MpCost());
                        }

                        ski.LastUse = DateTime.Now;

                        // We save the reduced cooldown amount for using it later
                        var reducedCooldown = ski.Skill.Cooldown * (cooldownReduction / 100D);

                        // We will check if there's a cooldown reduction in queue
                        if (cooldownReduction != 0)
                        {
                            ski.Skill.Cooldown = (short)(ski.Skill.Cooldown - reducedCooldown);
                            ski.LastUse = ski.LastUse.AddMilliseconds((reducedCooldown) * -1 * 100);
                        }

                        session.Character.PyjamaDead = ski.SkillVNum == 801;

                        // Area on attacker
                        if (ski.Skill.TargetType == 1 && ski.Skill.HitType == 1)
                        {
                            if (session.Character.MapInstance.MapInstanceType == MapInstanceType.TalentArenaMapInstance && !session.Character.MapInstance.IsPVP)
                            {
                                session.SendPacket(StaticPacketHelper.Cancel(2, targetId));
                                return;
                            }

                            session.SendPacket(session.Character.GenerateStat());
                            CharacterSkill skillinfo = session.Character.Skills.FirstOrDefault(s =>
                                s.Skill.UpgradeSkill == ski.Skill.SkillVNum && s.Skill.Effect > 0
                                                                            && s.Skill.SkillType == 2);

                            session.CurrentMapInstance.Broadcast(StaticPacketHelper.CastOnTarget(UserType.Player,
                                session.Character.CharacterId, targetType, targetId,
                                ski.Skill.CastAnimation, skillinfo?.Skill.CastEffect ?? ski.Skill.CastEffect,
                                ski.Skill.SkillVNum));

                            short skillEffect = skillinfo?.Skill.Effect ?? ski.Skill.Effect;

                            if (session.Character.BattleEntity.HasBuff(CardType.FireCannoneerRangeBuff, (byte)AdditionalTypes.FireCannoneerRangeBuff.AOEIncreased) && ski.Skill.Effect == 4569)
                            {
                                skillEffect = 4572;
                            }

                            #region SP2 MA
                            if (ski.Skill.SkillVNum == 1618)
                            {
                                foreach (BCard bc in ski.Skill.BCards.Where(s => s.Type == (byte)CardType.Summons && s.SubType == (byte)AdditionalTypes.Summons.Summons))
                                {
                                    bc.ApplyBCards(session.Character.BattleEntity, session.Character.BattleEntity);
                                }
                            }
                            #endregion

                            byte targetRange = ski.TargetRange();

                            if (targetRange != 0)
                            {
                                ski.GetSkillBCards().Where(s => (s.Type.Equals((byte)CardType.Buff) && new Buff((short)s.SecondData, session.Character.Level).Card?.BuffType == BuffType.Good)
                                    || (s.Type.Equals((byte)CardType.SpecialEffects2) && s.SubType.Equals((byte)AdditionalTypes.SpecialEffects2.TeleportInRadius))).ToList()
                                    .ForEach(s => s.ApplyBCards(session.Character.BattleEntity, session.Character.BattleEntity, partnerBuffLevel: ski.TattooLevel));
                            }

                            if (ski.Skill != null && ski.Skill.BCards != null && ski.Skill.BCards.Any(s =>
                                s.Type == (byte)CardType.Morale &&
                                s.SubType == (byte)AdditionalTypes.Morale.SkillCooldownCancelled))
                            {
                                cooldownReduction = 0;
                            }

                            session.CurrentMapInstance.Broadcast(StaticPacketHelper.SkillUsed(UserType.Player,
                            session.Character.CharacterId, 1, session.Character.CharacterId, ski.Skill.SkillVNum,
                            (short)(ski.Skill.Cooldown),
                            ski.Skill.AttackAnimation,
                            skillEffect, session.Character.PositionX,
                            session.Character.PositionY, true,
                            (int)(session.Character.Hp / session.Character.HPLoad() * 100), 0, -2,
                            (byte)(ski.Skill.SkillType - 1)));

                            if (targetRange != 0)
                            {
                                foreach (ClientSession character in ServerManager.Instance.Sessions.Where(s =>
                                    s.CurrentMapInstance == session.CurrentMapInstance
                                    && s.Character.CharacterId != session.Character.CharacterId
                                    && s.Character.IsInRange(session.Character.PositionX, session.Character.PositionY,
                                        ski.TargetRange())))
                                {
                                    if (session.Character.BattleEntity.CanAttackEntity(character.Character.BattleEntity))
                                    {
                                        session.PvpHit(new HitRequest(TargetHitType.AOETargetHit, session, ski.Skill, skillBCards: ski.GetSkillBCards()),
                                            character);
                                    }
                                }

                                foreach (MapMonster mon in session.CurrentMapInstance
                                    .GetMonsterInRangeList(session.Character.PositionX, session.Character.PositionY,
                                        ski.TargetRange()).Where(s => session.Character.BattleEntity.CanAttackEntity(s.BattleEntity)))
                                {
                                    lock (mon._onHitLockObject)
                                    {
                                        mon.OnReceiveHit(new HitRequest(TargetHitType.AOETargetHit, session, ski.Skill,
                                            skillinfo?.Skill.Effect ?? ski.Skill.Effect));
                                    }
                                }

                                foreach (Mate mate in session.CurrentMapInstance
                                    .GetListMateInRange(session.Character.PositionX, session.Character.PositionY,
                                        ski.TargetRange()).Where(s => session.Character.BattleEntity.CanAttackEntity(s.BattleEntity)))
                                {
                                    mate.HitRequest(new HitRequest(TargetHitType.AOETargetHit, session, ski.Skill,
                                        skillinfo?.Skill.Effect ?? ski.Skill.Effect, skillBCards: ski.GetSkillBCards()));
                                }
                            }
                        }
                        else if (ski.Skill.TargetType == 2 && ski.Skill.HitType == 0 || isSacrificeSkill)
                        {
                            ConcurrentBag<ArenaTeamMember> team = null;
                            if (session.Character.MapInstance.MapInstanceType == MapInstanceType.TalentArenaMapInstance)
                            {
                                team = ServerManager.Instance.ArenaTeams.ToList().FirstOrDefault(s => s.Any(o => o.Session == session));
                            }

                            if (session.Character.BattleEntity.CanAttackEntity(targetEntity)
                             || (team != null && team.FirstOrDefault(s => s.Session == session)?.ArenaTeamType != team.FirstOrDefault(s => s.Session == targetEntity.Character.Session)?.ArenaTeamType))
                            {
                                targetEntity = session.Character.BattleEntity;
                            }
                            if (session.Character.MapInstance == ServerManager.Instance.ArenaInstance && targetEntity.Mate?.Owner != session.Character && targetEntity != session.Character.BattleEntity && (session.Character.Group == null || !session.Character.Group.IsMemberOfGroup(targetEntity.MapEntityId)))
                            {
                                targetEntity = session.Character.BattleEntity;
                            }
                            if (session.Character.MapInstance == ServerManager.Instance.FamilyArenaInstance && targetEntity.Mate?.Owner != session.Character && targetEntity != session.Character.BattleEntity && session.Character.Family != (targetEntity.Character?.Family ?? targetEntity.Mate?.Owner.Family ?? targetEntity.MapMonster?.Owner?.Character?.Family))
                            {
                                targetEntity = session.Character.BattleEntity;
                            }

                            if (session.Character.MapInstance == ServerManager.Instance.ArenaInstance)
                            {
                                targetEntity = session.Character.BattleEntity;
                            }

                            if (session.Character.MapInstance == ServerManager.Instance.FamilyArenaInstance)
                            {
                                targetEntity = session.Character.BattleEntity;
                            }

                            if (targetEntity.Character != null && targetEntity.Character.IsSitting)
                            {
                                targetEntity.Character.IsSitting = false;
                                session.CurrentMapInstance?.Broadcast(targetEntity.Character.GenerateRest());
                            }

                            if (targetEntity.Mate != null && targetEntity.Mate.IsSitting)
                            {
                                session.CurrentMapInstance?.Broadcast(targetEntity.Mate.GenerateRest(false));
                            }

                            ski.GetSkillBCards().ToList().Where(s => !s.Type.Equals((byte)CardType.MeditationSkill)).ToList()
                                .ForEach(s => s.ApplyBCards(targetEntity, session.Character.BattleEntity, partnerBuffLevel: ski.TattooLevel));

                            targetEntity.MapInstance.Broadcast(StaticPacketHelper.CastOnTarget(UserType.Player,
                                session.Character.CharacterId, targetEntity.UserType, targetEntity.MapEntityId,
                                ski.Skill.CastAnimation, ski.Skill.CastEffect, ski.Skill.SkillVNum));
                            targetEntity.MapInstance.Broadcast(StaticPacketHelper.SkillUsed(UserType.Player,
                                session.Character.CharacterId, (byte)targetEntity.UserType, targetEntity.MapEntityId,
                                ski.Skill.SkillVNum,
                                (short)(ski.Skill.Cooldown),
                                ski.Skill.AttackAnimation, ski.Skill.Effect, targetEntity.PositionX,
                                targetEntity.PositionY, true,
                                (int)(targetEntity.Hp / targetEntity.HPLoad() * 100), 0, -1,
                                (byte)(ski.Skill.SkillType - 1)));
                        }
                        else if (ski.Skill.TargetType == 1 && ski.Skill.HitType != 1)
                        {
                            session.CurrentMapInstance.Broadcast(StaticPacketHelper.CastOnTarget(UserType.Player,
                                session.Character.CharacterId, UserType.Player, session.Character.CharacterId,
                                ski.Skill.CastAnimation, ski.Skill.CastEffect, ski.Skill.SkillVNum));

                            if (ski.Skill.CastEffect != 0)
                            {
                                Thread.Sleep(ski.Skill.CastTime * 100);
                            }

                            session.CurrentMapInstance.Broadcast(StaticPacketHelper.SkillUsed(UserType.Player,
                                            session.Character.CharacterId, 1, session.Character.CharacterId, ski.Skill.SkillVNum,
                                            (short)(ski.Skill.Cooldown),
                                            ski.Skill.AttackAnimation, ski.Skill.Effect,
                                            session.Character.PositionX, session.Character.PositionY, true,
                                            (int)(session.Character.Hp / session.Character.HPLoad() * 100), 0, -1,
                                            (byte)(ski.Skill.SkillType - 1)));

                            if (ski.SkillVNum != 1330)
                            {
                                switch (ski.Skill.HitType)
                                {
                                    case 0:
                                    case 4:
                                        if (!SkillHelper.Instance.IsNoDamage(ski.Skill.SkillVNum) && session.Character.Buff.FirstOrDefault(s => s.Card.BCards.Any(b => b.Type == (byte)CardType.FalconSkill && b.SubType.Equals((byte)AdditionalTypes.FalconSkill.Hide))) is Buff FalconHideBuff)
                                        {
                                            session.Character.RemoveBuff(FalconHideBuff.Card.CardId);
                                            session.Character.AddBuff(new Buff(560, session.Character.Level), session.Character.BattleEntity);
                                        }
                                        break;

                                    case 2:
                                        ConcurrentBag<ArenaTeamMember> team = null;
                                        if (session.Character.MapInstance.MapInstanceType == MapInstanceType.TalentArenaMapInstance)
                                        {
                                            team = ServerManager.Instance.ArenaTeams.ToList().FirstOrDefault(s => s.Any(o => o.Session == session));
                                        }

                                        IEnumerable<ClientSession> clientSessions =
                                            session.CurrentMapInstance.Sessions?.Where(s => s.Character.CharacterId != session.Character.CharacterId &&
                                                s.Character.IsInRange(session.Character.PositionX,
                                                    session.Character.PositionY, ski.TargetRange()));
                                        if (clientSessions != null)
                                        {
                                            foreach (ClientSession target in clientSessions)
                                            {
                                                if (session.Character.MapInstance == ServerManager.Instance.ArenaInstance)
                                                {
                                                    targetEntity = session.Character.BattleEntity;
                                                }
                                                else if (session.Character.MapInstance == ServerManager.Instance.FamilyArenaInstance)
                                                {
                                                    targetEntity = session.Character.BattleEntity;
                                                }
                                                else if (!session.Character.BattleEntity.CanAttackEntity(target.Character.BattleEntity)
                                                  && (team == null || team.FirstOrDefault(s => s.Session == session)?.ArenaTeamType == team.FirstOrDefault(s => s.Session == target.Character.Session)?.ArenaTeamType))
                                                {
                                                    if (session.Character.MapInstance == ServerManager.Instance.ArenaInstance && (session.Character.Group == null || !session.Character.Group.IsMemberOfGroup(target.Character.CharacterId)))
                                                    {
                                                        continue;
                                                    }
                                                    if (session.Character.MapInstance == ServerManager.Instance.FamilyArenaInstance && session.Character.Family != target.Character.Family)
                                                    {
                                                        continue;
                                                    }

                                                    ski.GetSkillBCards().ToList().Where(s => !s.Type.Equals((byte)CardType.MeditationSkill))
                                                    .ToList().ForEach(s =>
                                                        s.ApplyBCards(target.Character.BattleEntity, session.Character.BattleEntity, partnerBuffLevel: ski.TattooLevel));

                                                    session.CurrentMapInstance.Broadcast(StaticPacketHelper.SkillUsed(
                                                            UserType.Player,
                                                            session.Character.CharacterId, 1, target.Character.CharacterId,
                                                            ski.Skill.SkillVNum,
                                                            (short)(ski.Skill.Cooldown),
                                                            ski.Skill.AttackAnimation, ski.Skill.Effect,
                                                            target.Character.PositionX, target.Character.PositionY, true,
                                                            (int)(target.Character.Hp / target.Character.HPLoad() * 100),
                                                            0, -1,
                                                            (byte)(ski.Skill.SkillType - 1)));
                                                }
                                            }
                                        }

                                        IEnumerable<Mate> mates = session.CurrentMapInstance.GetListMateInRange(session.Character.PositionX, session.Character.PositionY, ski.TargetRange());
                                        if (mates != null)
                                        {
                                            foreach (Mate target in mates)
                                            {
                                                if (!session.Character.BattleEntity.CanAttackEntity(target.BattleEntity))
                                                {
                                                    if (session.Character.MapInstance == ServerManager.Instance.ArenaInstance && (session.Character.Group == null || !session.Character.Group.IsMemberOfGroup(target.Owner.CharacterId)))
                                                    {
                                                        continue;
                                                    }
                                                    if (session.Character.MapInstance == ServerManager.Instance.FamilyArenaInstance && session.Character.Family != target.Owner.Family)
                                                    {
                                                        continue;
                                                    }

                                                    ski.GetSkillBCards().ToList().Where(s => !s.Type.Equals((byte)CardType.MeditationSkill))
                                                    .ToList().ForEach(s =>
                                                        s.ApplyBCards(target.BattleEntity, session.Character.BattleEntity, partnerBuffLevel: ski.TattooLevel));

                                                    session.CurrentMapInstance.Broadcast(StaticPacketHelper.SkillUsed(
                                                            UserType.Player,
                                                            session.Character.CharacterId,
                                                            (byte)target.BattleEntity.UserType, target.MateTransportId,
                                                            ski.Skill.SkillVNum,
                                                            (short)(ski.Skill.Cooldown),
                                                            ski.Skill.AttackAnimation, ski.Skill.Effect,
                                                            target.PositionX, target.PositionY, true,
                                                            (int)(target.Hp / target.HpLoad() * 100), 0, -1,
                                                            (byte)(ski.Skill.SkillType - 1)));
                                                }
                                            }
                                        }

                                        IEnumerable<MapMonster> monsters = session.CurrentMapInstance.GetMonsterInRangeList(session.Character.PositionX, session.Character.PositionY, ski.TargetRange());
                                        if (monsters != null)
                                        {
                                            foreach (MapMonster target in monsters)
                                            {
                                                if (!session.Character.BattleEntity.CanAttackEntity(target.BattleEntity))
                                                {
                                                    if (target.Owner != null)
                                                    {
                                                        if (target.Owner.Character != null)
                                                        {
                                                            continue;
                                                        }
                                                        if (session.Character.MapInstance == ServerManager.Instance.ArenaInstance && (session.Character.Group == null || !session.Character.Group.IsMemberOfGroup(target.Owner.MapEntityId)))
                                                        {
                                                            continue;
                                                        }
                                                        if (session.Character.MapInstance == ServerManager.Instance.FamilyArenaInstance && session.Character.Family != target.Owner.Character?.Family)
                                                        {
                                                            continue;
                                                        }
                                                    }

                                                    ski.GetSkillBCards().ToList().Where(s => !s.Type.Equals((byte)CardType.MeditationSkill))
                                                    .ToList().ForEach(s =>
                                                        s.ApplyBCards(target.BattleEntity, session.Character.BattleEntity, partnerBuffLevel: ski.TattooLevel));

                                                    session.CurrentMapInstance.Broadcast(StaticPacketHelper.SkillUsed(
                                                            UserType.Player,
                                                            session.Character.CharacterId,
                                                            (byte)target.BattleEntity.UserType, target.MapMonsterId,
                                                            ski.Skill.SkillVNum,
                                                            (short)(ski.Skill.Cooldown),
                                                            ski.Skill.AttackAnimation, ski.Skill.Effect,
                                                            target.MapX, target.MapY, true,
                                                            (int)(target.CurrentHp / target.MaxHp * 100), 0, -1,
                                                            (byte)(ski.Skill.SkillType - 1)));
                                                }
                                            }
                                        }

                                        IEnumerable<MapNpc> npcs = session.CurrentMapInstance.GetListNpcInRange(session.Character.PositionX, session.Character.PositionY, ski.TargetRange());
                                        if (npcs != null)
                                        {
                                            foreach (MapNpc target in npcs)
                                            {
                                                if (!session.Character.BattleEntity.CanAttackEntity(target.BattleEntity))
                                                {
                                                    if (target.Owner != null)
                                                    {
                                                        if (session.Character.MapInstance == ServerManager.Instance.ArenaInstance && (session.Character.Group == null || !session.Character.Group.IsMemberOfGroup(target.Owner.MapEntityId)))
                                                        {
                                                            continue;
                                                        }
                                                        if (session.Character.MapInstance == ServerManager.Instance.FamilyArenaInstance && session.Character.Family != target.Owner.Character?.Family)
                                                        {
                                                            continue;
                                                        }
                                                    }

                                                    ski.GetSkillBCards().ToList().Where(s => !s.Type.Equals((byte)CardType.MeditationSkill))
                                                    .ToList().ForEach(s =>
                                                        s.ApplyBCards(target.BattleEntity, session.Character.BattleEntity, partnerBuffLevel: ski.TattooLevel));

                                                    session.CurrentMapInstance.Broadcast(StaticPacketHelper.SkillUsed(
                                                            UserType.Player,
                                                            session.Character.CharacterId,
                                                            (byte)target.BattleEntity.UserType, target.MapNpcId,
                                                            ski.Skill.SkillVNum,
                                                            (short)(ski.Skill.Cooldown),
                                                            ski.Skill.AttackAnimation, ski.Skill.Effect,
                                                            target.MapX, target.MapY, true,
                                                            (int)(target.CurrentHp / target.MaxHp * 100), 0, -1,
                                                            (byte)(ski.Skill.SkillType - 1)));
                                                }
                                            }
                                        }

                                        break;
                                }
                            }

                            ski.GetSkillBCards().ToList().Where(s => !s.Type.Equals((byte)CardType.MeditationSkill)).ToList().ForEach(s => s.ApplyBCards(session.Character.BattleEntity, session.Character.BattleEntity, partnerBuffLevel: ski.TattooLevel));
                        }
                        else if (ski.Skill.TargetType == 0)
                        {
                            if (session.Character.MapInstance.MapInstanceType == MapInstanceType.TalentArenaMapInstance && !session.Character.MapInstance.IsPVP)
                            {
                                session.SendPacket(StaticPacketHelper.Cancel(2, targetId));
                                return;
                            }

                            if (isPvp)
                            {
                                //ClientSession playerToAttack = ServerManager.Instance.GetSessionByCharacterId(targetId);
                                ClientSession playerToAttack = targetEntity.Character?.Session;

                                if (playerToAttack != null && !playerToAttack.Character.IsFrozen)
                                {
                                    if (Map.GetDistance(
                                            new MapCell
                                            {
                                                X = session.Character.PositionX,
                                                Y = session.Character.PositionY
                                            },
                                            new MapCell
                                            {
                                                X = playerToAttack.Character.PositionX,
                                                Y = playerToAttack.Character.PositionY
                                            }) <= ski.Skill.Range + 5)
                                    {

                                        if (ski.SkillVNum == 1061)
                                        {
                                            session.CurrentMapInstance.Broadcast($"eff 1 {targetId} 4968");
                                            session.CurrentMapInstance.Broadcast($"eff 1 {session.Character.CharacterId} 4968");
                                        }

                                        session.SendPacket(session.Character.GenerateStat());
                                        CharacterSkill characterSkillInfo = session.Character.Skills.FirstOrDefault(s =>
                                            s.Skill.UpgradeSkill == ski.Skill.SkillVNum && s.Skill.Effect > 0
                                                                                        && s.Skill.SkillType == 2);
                                        session.CurrentMapInstance.Broadcast(
                                            StaticPacketHelper.CastOnTarget(UserType.Player,
                                                session.Character.CharacterId, UserType.Player, targetId, ski.Skill.CastAnimation,
                                                characterSkillInfo?.Skill.CastEffect ?? ski.Skill.CastEffect,
                                                ski.Skill.SkillVNum));
                                        session.Character.Skills.Where(s => s.Id != ski.Id).ForEach(i => i.Hit = 0);

                                        // Generate scp
                                        if ((DateTime.Now - ski.LastUse).TotalSeconds > 3)
                                        {
                                            ski.Hit = 0;
                                        }
                                        else
                                        {
                                            ski.Hit++;
                                        }

                                        ski.LastUse = DateTime.Now;

                                        // We will check if there's a cooldown reduction in queue
                                        if (cooldownReduction != 0)
                                        {
                                            ski.LastUse = ski.LastUse.AddMilliseconds((reducedCooldown) * -1 * 100);
                                        }

                                        if (ski.Skill.CastEffect != 0)
                                        {
                                            Thread.Sleep(ski.Skill.CastTime * 100);
                                        }

                                        if (ski.Skill.HitType == 3)
                                        {
                                            int count = 0;
                                            if (playerToAttack.CurrentMapInstance == session.CurrentMapInstance
                                                && playerToAttack.Character.CharacterId !=
                                                session.Character.CharacterId)
                                            {
                                                if (session.Character.BattleEntity.CanAttackEntity(playerToAttack.Character.BattleEntity))
                                                {
                                                    count++;
                                                    session.PvpHit(
                                                        new HitRequest(TargetHitType.SingleAOETargetHit, session,
                                                            ski.Skill, skillBCards: ski.GetSkillBCards(), showTargetAnimation: true), playerToAttack);
                                                }
                                                else
                                                {
                                                    session.SendPacket(StaticPacketHelper.Cancel(2, targetId));
                                                }
                                            }

                                            //foreach (long id in Session.Character.MTListTargetQueue.Where(s => s.EntityType == UserType.Player).Select(s => s.TargetId))
                                            foreach (long id in session.Character.GetMTListTargetQueue_QuickFix(ski, UserType.Player))
                                            {
                                                ClientSession character = ServerManager.Instance.GetSessionByCharacterId(id);

                                                if (character != null
                                                    && character.CurrentMapInstance == session.CurrentMapInstance
                                                    && character.Character.CharacterId != session.Character.CharacterId
                                                    && character != playerToAttack)
                                                {
                                                    if (session.Character.BattleEntity.CanAttackEntity(character.Character.BattleEntity))
                                                    {
                                                        count++;
                                                        session.PvpHit(new HitRequest(TargetHitType.SingleAOETargetHit, session, ski.Skill, showTargetAnimation: count == 1, skillBCards: ski.GetSkillBCards()), character);
                                                    }
                                                }
                                            }

                                            if (count == 0)
                                            {
                                                session.SendPacket(StaticPacketHelper.Cancel(2, targetId));
                                            }
                                        }
                                        else
                                        {
                                            // check if we will hit mutltiple targets
                                            if (ski.TargetRange() != 0)
                                            {
                                                ComboDTO skillCombo = ski.Skill.Combos.Find(s => ski.Hit == s.Hit);
                                                if (skillCombo != null)
                                                {
                                                    if (ski.Skill.Combos.OrderByDescending(s => s.Hit).First().Hit == ski.Hit)
                                                    {
                                                        ski.Hit = 0;
                                                    }

                                                    IEnumerable<ClientSession> playersInAoeRange =
                                                        ServerManager.Instance.Sessions.Where(s =>
                                                            s.CurrentMapInstance == session.CurrentMapInstance
                                                            && s.Character.CharacterId != session.Character.CharacterId
                                                            && s != playerToAttack
                                                            && s.Character.IsInRange(playerToAttack.Character.PositionX,
                                                                playerToAttack.Character.PositionY, ski.TargetRange()));
                                                    int count = 0;
                                                    if (session.Character.BattleEntity.CanAttackEntity(playerToAttack.Character.BattleEntity))
                                                    {
                                                        count++;
                                                        session.PvpHit(new HitRequest(TargetHitType.SingleTargetHitCombo, session, ski.Skill, skillCombo: skillCombo, skillBCards: ski.GetSkillBCards()), playerToAttack);
                                                    }
                                                    else
                                                    {
                                                        session.SendPacket(StaticPacketHelper.Cancel(2, targetId));
                                                    }

                                                    foreach (ClientSession character in playersInAoeRange)
                                                    {
                                                        if (session.Character.BattleEntity.CanAttackEntity(character.Character.BattleEntity))
                                                        {
                                                            count++;
                                                            session.PvpHit(new HitRequest(TargetHitType.SingleTargetHitCombo, session, ski.Skill,
                                                                skillCombo: skillCombo, showTargetAnimation: count == 1, skillBCards: ski.GetSkillBCards()), character);
                                                        }
                                                    }

                                                    if (playerToAttack.Character.Hp <= 0 || count == 0)
                                                    {
                                                        session.SendPacket(StaticPacketHelper.Cancel(2, targetId));
                                                    }
                                                }
                                                else
                                                {
                                                    IEnumerable<ClientSession> playersInAoeRange =
                                                        ServerManager.Instance.Sessions.Where(s =>
                                                            s.CurrentMapInstance == session.CurrentMapInstance
                                                            && s.Character.CharacterId != session.Character.CharacterId
                                                            && s != playerToAttack
                                                            && s.Character.IsInRange(playerToAttack.Character.PositionX,
                                                                playerToAttack.Character.PositionY, ski.TargetRange()));

                                                    int count = 0;

                                                    // hit the targetted player
                                                    if (session.Character.BattleEntity.CanAttackEntity(playerToAttack.Character.BattleEntity))
                                                    {
                                                        count++;
                                                        session.PvpHit(new HitRequest(TargetHitType.SingleAOETargetHit, session, ski.Skill, showTargetAnimation: true, skillBCards: ski.GetSkillBCards()), playerToAttack);
                                                    }
                                                    else
                                                    {
                                                        session.SendPacket(StaticPacketHelper.Cancel(2, targetId));
                                                    }

                                                    //hit all other players
                                                    foreach (ClientSession character in playersInAoeRange)
                                                    {
                                                        count++;
                                                        if (session.Character.BattleEntity.CanAttackEntity(character.Character.BattleEntity))
                                                        {
                                                            session.PvpHit(new HitRequest(TargetHitType.SingleAOETargetHit, session, ski.Skill, showTargetAnimation: count == 1, skillBCards: ski.GetSkillBCards()), character);
                                                        }
                                                    }

                                                    if (playerToAttack.Character.Hp <= 0)
                                                    {
                                                        session.SendPacket(StaticPacketHelper.Cancel(2, targetId));
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                ComboDTO skillCombo = ski.Skill.Combos.Find(s => ski.Hit == s.Hit);
                                                if (skillCombo != null)
                                                {
                                                    if (ski.Skill.Combos.OrderByDescending(s => s.Hit).First().Hit == ski.Hit)
                                                    {
                                                        ski.Hit = 0;
                                                    }

                                                    if (session.Character.BattleEntity.CanAttackEntity(playerToAttack.Character.BattleEntity))
                                                    {
                                                        session.PvpHit(new HitRequest(TargetHitType.SingleTargetHitCombo, session, ski.Skill, skillCombo: skillCombo, skillBCards: ski.GetSkillBCards()), playerToAttack);
                                                    }
                                                    else
                                                    {
                                                        session.SendPacket(StaticPacketHelper.Cancel(2, targetId));
                                                    }
                                                }
                                                else
                                                {
                                                    if (session.Character.BattleEntity.CanAttackEntity(playerToAttack.Character.BattleEntity))
                                                    {
                                                        session.PvpHit(new HitRequest(TargetHitType.SingleTargetHit, session, ski.Skill, showTargetAnimation: true, skillBCards: ski.GetSkillBCards()), playerToAttack);
                                                    }
                                                    else
                                                    {
                                                        session.SendPacket(StaticPacketHelper.Cancel(2, targetId));
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        session.SendPacket(StaticPacketHelper.Cancel(2, targetId));
                                    }
                                }
                                else if (playerToAttack.Character.IsFrozen)
                                {
                                    session.SendPacket(StaticPacketHelper.Cancel(2, targetId));
                                    var rbb = ServerManager.Instance.RainbowBattleMembers.Find(s => s.Session.Contains(playerToAttack));
                                    var rbb2 = ServerManager.Instance.RainbowBattleMembers.Find(s => s.Session.Contains(session));

                                    if (rbb != rbb2)
                                    {
                                        return;
                                    }

                                    if (playerToAttack.Character.LastPvPKiller == null || playerToAttack.Character.LastPvPKiller != session)
                                    {
                                        session.SendPacket($"delay 2000 5 #guri^504^1^{playerToAttack.Character.CharacterId}");
                                    }
                                }
                                else
                                {
                                    session.SendPacket(StaticPacketHelper.Cancel(2, targetId));
                                }
                            }
                            else
                            {
                                MapMonster monsterToAttack = targetEntity.MapMonster;

                                if (monsterToAttack != null)
                                {
                                    if (Map.GetDistance(new MapCell { X = session.Character.PositionX, Y = session.Character.PositionY },
                                        new MapCell { X = monsterToAttack.MapX, Y = monsterToAttack.MapY }) <= ski.Skill.Range + 5 + monsterToAttack.Monster.BasicArea)
                                    {

                                        #region Taunt

                                        if (ski.SkillVNum == 1061)
                                        {
                                            session.CurrentMapInstance.Broadcast($"eff 3 {targetId} 4968");
                                            session.CurrentMapInstance.Broadcast($"eff 1 {session.Character.CharacterId} 4968");
                                        }

                                        #endregion

                                        ski.GetSkillBCards().ToList().Where(s => s.CastType == 1).ToList()
                                            .ForEach(s => s.ApplyBCards(monsterToAttack.BattleEntity, session.Character.BattleEntity, partnerBuffLevel: ski.TattooLevel));

                                        session.SendPacket(session.Character.GenerateStat());

                                        CharacterSkill ski2 = session.Character.Skills.FirstOrDefault(s => s.Skill.UpgradeSkill == ski.Skill.SkillVNum
                                            && s.Skill.Effect > 0 && s.Skill.SkillType == 2);

                                        session.CurrentMapInstance.Broadcast(StaticPacketHelper.CastOnTarget(UserType.Player, session.Character.CharacterId, UserType.Monster, monsterToAttack.MapMonsterId,
                                            ski.Skill.CastAnimation, ski2?.Skill.CastEffect ?? ski.Skill.CastEffect, ski.Skill.SkillVNum));

                                        session.Character.Skills.Where(x => x.Id != ski.Id).ForEach(x => x.Hit = 0);

                                        #region Generate scp

                                        if ((DateTime.Now - ski.LastUse).TotalSeconds > 3)
                                        {
                                            ski.Hit = 0;
                                        }
                                        else
                                        {
                                            ski.Hit++;
                                        }

                                        #endregion

                                        ski.LastUse = DateTime.Now;

                                        // We will check if there's a cooldown reduction in queue
                                        if (cooldownReduction != 0)
                                        {
                                            ski.LastUse = ski.LastUse.AddMilliseconds((reducedCooldown) * -1 * 100);
                                        }

                                        if (ski.Skill.CastEffect != 0)
                                        {
                                            Thread.Sleep(ski.Skill.CastTime * 100);
                                        }

                                        if (ski.Skill.HitType == 3)
                                        {
                                            monsterToAttack.HitQueue.Enqueue(new HitRequest(TargetHitType.SingleAOETargetHit, session,
                                                ski.Skill, ski2?.Skill.Effect ?? ski.Skill.Effect, showTargetAnimation: true, skillBCards: ski.GetSkillBCards()));

                                            //foreach (long id in Session.Character.MTListTargetQueue.Where(s => s.EntityType == UserType.Monster).Select(s => s.TargetId))
                                            foreach (long id in session.Character.GetMTListTargetQueue_QuickFix(ski, UserType.Monster))
                                            {
                                                MapMonster mon = session.CurrentMapInstance.GetMonsterById(id);

                                                if (mon?.CurrentHp > 0)
                                                {
                                                    mon.HitQueue.Enqueue(new HitRequest(TargetHitType.SingleAOETargetHit, session,
                                                        ski.Skill, ski2?.Skill.Effect ?? ski.Skill.Effect, skillBCards: ski.GetSkillBCards()));
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (ski.TargetRange() != 0 || ski.Skill.HitType == 1)
                                            {
                                                ComboDTO skillCombo = ski.Skill.Combos.Find(s => ski.Hit == s.Hit);

                                                List<MapMonster> monstersInAoeRange = session.CurrentMapInstance?.GetMonsterInRangeList(monsterToAttack.MapX, monsterToAttack.MapY, ski.TargetRange())?
                                                        .Where(m => session.Character.BattleEntity.CanAttackEntity(m.BattleEntity)).ToList();

                                                if (skillCombo != null)
                                                {
                                                    if (ski.Skill.Combos.OrderByDescending(s => s.Hit).First().Hit == ski.Hit)
                                                    {
                                                        ski.Hit = 0;
                                                    }

                                                    if (monsterToAttack.IsAlive && monstersInAoeRange?.Count != 0)
                                                    {
                                                        foreach (MapMonster mon in monstersInAoeRange)
                                                        {
                                                            mon.HitQueue.Enqueue(new HitRequest(TargetHitType.SingleTargetHitCombo, session,
                                                                ski.Skill, skillCombo: skillCombo, skillBCards: ski.GetSkillBCards()));
                                                        }
                                                    }
                                                    else
                                                    {
                                                        session.SendPacket(StaticPacketHelper.Cancel(2, targetId));
                                                    }
                                                }
                                                else
                                                {
                                                    monsterToAttack.HitQueue.Enqueue(new HitRequest(TargetHitType.SingleAOETargetHit, session,
                                                            ski.Skill, ski2?.Skill.Effect ?? ski.Skill.Effect, showTargetAnimation: true, skillBCards: ski.GetSkillBCards()));

                                                    if (monsterToAttack.IsAlive && monstersInAoeRange?.Count != 0)
                                                    {
                                                        foreach (MapMonster mon in monstersInAoeRange.Where(m => m.MapMonsterId != monsterToAttack.MapMonsterId))
                                                        {
                                                            mon.HitQueue.Enqueue(new HitRequest(TargetHitType.SingleAOETargetHit, session, ski.Skill, ski2?.Skill.Effect ?? ski.Skill.Effect, skillBCards: ski.GetSkillBCards()));
                                                        }
                                                    }
                                                    else
                                                    {
                                                        session.SendPacket(StaticPacketHelper.Cancel(2, targetId));
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                ComboDTO skillCombo = ski.Skill.Combos.Find(s => ski.Hit == s.Hit);

                                                if (skillCombo != null)
                                                {
                                                    if (ski.Skill.Combos.OrderByDescending(s => s.Hit).First().Hit == ski.Hit)
                                                    {
                                                        ski.Hit = 0;
                                                    }

                                                    monsterToAttack.HitQueue.Enqueue(new HitRequest(TargetHitType.SingleTargetHitCombo, session,
                                                        ski.Skill, skillCombo: skillCombo, skillBCards: ski.GetSkillBCards()));
                                                }
                                                else
                                                {
                                                    monsterToAttack.HitQueue.Enqueue(new HitRequest(TargetHitType.SingleTargetHit, session,
                                                        ski.Skill, skillBCards: ski.GetSkillBCards()));
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        session.SendPacket(StaticPacketHelper.Cancel(2, targetId));
                                    }
                                }
                                else if (targetEntity.Mate is Mate mateToAttack)
                                {
                                    if (!session.Character.BattleEntity.CanAttackEntity(mateToAttack.BattleEntity))
                                    {
                                        session.Character.Session.SendPacket(StaticPacketHelper.Cancel(2, mateToAttack.BattleEntity.MapEntityId));
                                        return;
                                    }

                                    if (Map.GetDistance(new MapCell
                                    {
                                        X = session.Character.PositionX,
                                        Y = session.Character.PositionY
                                    },
                                    new MapCell { X = mateToAttack.PositionX, Y = mateToAttack.PositionY }) <= ski.Skill.Range + 5 + mateToAttack.Monster.BasicArea)
                                    {

                                        if (ski.SkillVNum == 1061)
                                        {
                                            session.CurrentMapInstance.Broadcast($"eff 2 {targetId} 4968");
                                            session.CurrentMapInstance.Broadcast($"eff 1 {session.Character.CharacterId} 4968");
                                        }

                                        ski.GetSkillBCards().ToList().Where(s => s.CastType == 1).ToList().ForEach(s => s.ApplyBCards(mateToAttack.BattleEntity, session.Character.BattleEntity, partnerBuffLevel: ski.TattooLevel));

                                        session.SendPacket(session.Character.GenerateStat());
                                        CharacterSkill characterSkillInfo = session.Character.Skills.FirstOrDefault(s =>
                                            s.Skill.UpgradeSkill == ski.Skill.SkillVNum && s.Skill.Effect > 0
                                                                                        && s.Skill.SkillType == 2);

                                        session.CurrentMapInstance.Broadcast(StaticPacketHelper.CastOnTarget(
                                            UserType.Player, session.Character.CharacterId, UserType.Npc,
                                            mateToAttack.MateTransportId, ski.Skill.CastAnimation,
                                            characterSkillInfo?.Skill.CastEffect ?? ski.Skill.CastEffect,
                                            ski.Skill.SkillVNum));
                                        session.Character.Skills.Where(s => s.Id != ski.Id).ForEach(i => i.Hit = 0);

                                        // Generate scp
                                        if ((DateTime.Now - ski.LastUse).TotalSeconds > 3)
                                        {
                                            ski.Hit = 0;
                                        }
                                        else
                                        {
                                            ski.Hit++;
                                        }

                                        ski.LastUse = DateTime.Now;

                                        // We will check if there's a cooldown reduction in queue
                                        if (cooldownReduction != 0)
                                        {
                                            ski.LastUse = ski.LastUse.AddMilliseconds((reducedCooldown) * -1 * 100);
                                        }

                                        if (ski.Skill.CastEffect != 0)
                                        {
                                            Thread.Sleep(ski.Skill.CastTime * 100);
                                        }

                                        if (ski.Skill.HitType == 3)
                                        {
                                            mateToAttack.HitRequest(new HitRequest(TargetHitType.SingleAOETargetHit, session, ski.Skill, characterSkillInfo?.Skill.Effect ?? ski.Skill.Effect, showTargetAnimation: true, skillBCards: ski.GetSkillBCards()));

                                            //foreach (long id in Session.Character.MTListTargetQueue.Where(s => s.EntityType == UserType.Monster).Select(s => s.TargetId))
                                            foreach (long id in session.Character.GetMTListTargetQueue_QuickFix(ski, UserType.Monster))
                                            {
                                                Mate mate = session.CurrentMapInstance.GetMate(id);
                                                if (mate != null && mate.Hp > 0 && session.Character.BattleEntity.CanAttackEntity(mate.BattleEntity))
                                                {
                                                    mate.HitRequest(new HitRequest(
                                                        TargetHitType.SingleAOETargetHit, session, ski.Skill,
                                                        characterSkillInfo?.Skill.Effect ?? ski.Skill.Effect, skillBCards: ski.GetSkillBCards()));
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (ski.TargetRange() != 0 || ski.Skill.HitType == 1) // check if we will hit mutltiple targets
                                            {
                                                ComboDTO skillCombo = ski.Skill.Combos.Find(s => ski.Hit == s.Hit);
                                                if (skillCombo != null)
                                                {
                                                    if (ski.Skill.Combos.OrderByDescending(s => s.Hit).First().Hit == ski.Hit)
                                                    {
                                                        ski.Hit = 0;
                                                    }

                                                    List<Mate> monstersInAoeRange = session.CurrentMapInstance?
                                                        .GetListMateInRange(mateToAttack.MapX,
                                                            mateToAttack.MapY, ski.TargetRange()).Where(m => session.Character.BattleEntity.CanAttackEntity(m.BattleEntity)).ToList();
                                                    if (monstersInAoeRange.Count != 0)
                                                    {
                                                        foreach (Mate mate in monstersInAoeRange)
                                                        {
                                                            mate.HitRequest(new HitRequest(TargetHitType.SingleTargetHitCombo, session, ski.Skill, skillCombo: skillCombo, skillBCards: ski.GetSkillBCards()));
                                                        }
                                                    }
                                                    else
                                                    {
                                                        session.SendPacket(StaticPacketHelper.Cancel(2, targetId));
                                                    }

                                                    if (!mateToAttack.IsAlive)
                                                    {
                                                        session.SendPacket(StaticPacketHelper.Cancel(2, targetId));
                                                    }
                                                }
                                                else
                                                {
                                                    List<Mate> matesInAoeRange = session.CurrentMapInstance?.GetListMateInRange(mateToAttack.MapX, mateToAttack.MapY,
                                                        ski.TargetRange())?.Where(m => session.Character.BattleEntity.CanAttackEntity(m.BattleEntity)).ToList();

                                                    //hit the targetted mate
                                                    mateToAttack.HitRequest(new HitRequest(TargetHitType.SingleAOETargetHit, session, ski.Skill, characterSkillInfo?.Skill.Effect ??
                                                        ski.Skill.Effect, showTargetAnimation: true, skillBCards: ski.GetSkillBCards()));

                                                    //hit all other mates
                                                    if (matesInAoeRange != null && matesInAoeRange.Count != 0)
                                                    {
                                                        foreach (Mate mate in matesInAoeRange.Where(m =>
                                                            m.MateTransportId != mateToAttack.MateTransportId)
                                                        ) //exclude targetted mates
                                                        {
                                                            mate.HitRequest(
                                                                new HitRequest(TargetHitType.SingleAOETargetHit,
                                                                    session, ski.Skill,
                                                                    characterSkillInfo?.Skill.Effect ??
                                                                    ski.Skill.Effect, skillBCards: ski.GetSkillBCards()));
                                                        }
                                                    }
                                                    else
                                                    {
                                                        session.SendPacket(StaticPacketHelper.Cancel(2, targetId));
                                                    }

                                                    if (!mateToAttack.IsAlive)
                                                    {
                                                        session.SendPacket(StaticPacketHelper.Cancel(2, targetId));
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                ComboDTO skillCombo = ski.Skill.Combos.Find(s => ski.Hit == s.Hit);
                                                if (skillCombo != null)
                                                {
                                                    if (ski.Skill.Combos.OrderByDescending(s => s.Hit).First().Hit
                                                        == ski.Hit)
                                                    {
                                                        ski.Hit = 0;
                                                    }

                                                    mateToAttack.HitRequest(
                                                        new HitRequest(TargetHitType.SingleTargetHitCombo, session,
                                                            ski.Skill, skillCombo: skillCombo, skillBCards: ski.GetSkillBCards()));
                                                }
                                                else
                                                {
                                                    mateToAttack.HitRequest(
                                                        new HitRequest(TargetHitType.SingleTargetHit, session,
                                                            ski.Skill, skillBCards: ski.GetSkillBCards()));
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        session.SendPacket(StaticPacketHelper.Cancel(2, targetId));
                                    }
                                }
                                else
                                {
                                    session.SendPacket(StaticPacketHelper.Cancel(2, targetId));
                                }
                            }

                            if (ski.Skill.HitType == 3)
                            {
                                session.Character.MTListTargetQueue.Clear();
                            }

                            ski.GetSkillBCards().Where(s =>
                               (s.Type.Equals((byte)CardType.Buff) && new Buff((short)s.SecondData, session.Character.Level).Card?.BuffType == BuffType.Good)).ToList()
                                .ForEach(s => s.ApplyBCards(session.Character.BattleEntity, session.Character.BattleEntity, partnerBuffLevel: ski.TattooLevel));
                        }
                        else
                        {
                            session.SendPacket(StaticPacketHelper.Cancel(2, targetId));
                        }

                        /* disable it it's useless.
                        if (ski.Skill.SkillVNum != 1098 && ski.Skill.SkillVNum != 1330)
                        {
                            session.SendPacket(StaticPacketHelper.SkillResetWithCoolDown(castingId, ski.Skill.Cooldown));
                        }
                        */

                        var cdResetMilliseconds = ski.Skill.CastId != 0 ? ski.Skill.Cooldown * 105 : ski.Skill.Cooldown * 160;
                        Observable.Timer(TimeSpan.FromMilliseconds(cdResetMilliseconds)).Subscribe(o =>
                        {
                            if (session.Character != null)
                            {
                                sendSkillReset();
                            }

                            if (cdResetMilliseconds <= 500)
                            {
                                Observable.Timer(TimeSpan.FromMilliseconds(500)).Subscribe(obs => sendSkillReset());
                            }

                            void sendSkillReset()
                            {
                                var charSkills = session.Character.GetSkills();

                                CharacterSkill skill = charSkills.Find(s => s.Skill?.CastId == castingId && (s.Skill?.UpgradeSkill == 0 || s.Skill?.SkillType == 1));

                                var dateTimeNow = DateTime.Now;
                                if (skill != null)
                                {
                                    session.SendPacket(StaticPacketHelper.SkillReset(castingId));
                                    skill.ReinstantiateSkill();
                                }
                            }
                        });

                        // This will reset skill's cooldown if you have fairy wings
                        int[] fairyWings = session.Character.GetBuff(CardType.EffectSummon, (byte)AdditionalTypes.EffectSummon.LastSkillReset);
                        int random = ServerManager.RandomNumber();
                        if (fairyWings[0] > random)
                        {
                            Observable.Timer(TimeSpan.FromSeconds(1)).SafeSubscribe(o =>
                            {
                                if (session == null)
                                {
                                    return;
                                }

                                if (ski != null)
                                {
                                    ski.LastUse = DateTime.Now.AddMilliseconds(ski.Skill.Cooldown * 100 * -1);
                                    session.SendPacket(StaticPacketHelper.SkillReset(ski.Skill.CastId));
                                }
                            });
                        }
                    }
                    else
                    {
                        session.SendPacket(StaticPacketHelper.Cancel(2, targetId));
                        session.SendPacket(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("NOT_ENOUGH_MP"), 10));
                    }
                }
            }
            else
            {
                session.SendPacket(StaticPacketHelper.Cancel(2, targetId));
            }

            if ((castingId != 0 && castingId < 11 && shouldCancel) || session.Character.SkillComboCount > 7)
            {
                if (!session.Character.HasMagicSpellCombo && session.Character.SkillComboCount > 7)
                {
                    session.SendPacket($"mslot {session.Character.LastComboCastId} 0");
                }
            }

            session.Character.LastSkillUse = DateTime.Now;
        }

        public static void ZoneHit(this ClientSession session, int castingId, short x, short y)
        {
            CharacterSkill characterSkill = session.Character.GetSkills()?.Find(s => s.Skill?.CastId == castingId);
            if (characterSkill == null || !session.Character.WeaponLoaded(characterSkill) || !session.HasCurrentMapInstance
                                       || ((x != 0 || y != 0) && !session.Character.IsInRange(x, y, characterSkill.GetSkillRange() + 1)) || characterSkill.Skill.TargetType != 3)
            {
                session.SendPacket(StaticPacketHelper.Cancel(2));
                return;
            }

            if (characterSkill.CanBeUsed())
            {
                short mpCost = characterSkill.MpCost();
                short hpCost = 0;

                mpCost = (short)(mpCost * ((100 - session.Character.CellonOptions.Where(s => s.Type == CellonOptionType.MPUsage).Sum(s => s.Value)) / 100D));

                if (session.Character.GetBuff(CardType.HealingBurningAndCasting, (byte)AdditionalTypes.HealingBurningAndCasting.HPDecreasedByConsumingMP)[0] is int HPDecreasedByConsumingMP)
                {
                    if (HPDecreasedByConsumingMP < 0)
                    {
                        int amountDecreased = -(characterSkill.MpCost() * HPDecreasedByConsumingMP / 100 * -1);
                        hpCost = (short)amountDecreased;
                        mpCost -= (short)amountDecreased;
                    }
                }

                if (session.Character.Mp >= mpCost && session.Character.Hp > hpCost && session.HasCurrentMapInstance)
                {
                    session.Character.LastSkillUse = DateTime.Now;

                    double cooldownReduction = session.Character.GetBuff(CardType.Morale, (byte)AdditionalTypes.Morale.SkillCooldownDecreased)[0] * -1 + session.Character.GetBuff(CardType.Casting, (byte)AdditionalTypes.Casting.EffectDurationIncreased)[0];

                    int[] increaseEnemyCooldownChance = session.Character.GetBuff(CardType.DarkCloneSummon, (byte)AdditionalTypes.DarkCloneSummon.IncreaseEnemyCooldownChance);

                    if (ServerManager.RandomNumber() < increaseEnemyCooldownChance[0])
                    {
                        cooldownReduction += increaseEnemyCooldownChance[1];
                    }

                    if (characterSkill?.Skill?.BCards != null && characterSkill.Skill.BCards.Any(s =>
                        s.Type == (byte)CardType.Morale &&
                        s.SubType == (byte)AdditionalTypes.Morale.SkillCooldownCancelled))
                    {
                        cooldownReduction = 0;
                    }

                    session.CurrentMapInstance.Broadcast($"ct_n 1 {session.Character.CharacterId} 3 -1 {characterSkill.Skill.CastAnimation}" + $" {characterSkill.Skill.CastEffect} {characterSkill.Skill.SkillVNum}");
                    characterSkill.LastUse = DateTime.Now;

                    // We save the reduced cooldown amount for using it later
                    var reducedCooldown = (characterSkill.Skill.Cooldown * (cooldownReduction / 100D));

                    // We will check if there's a cooldown reduction in queue
                    if (cooldownReduction != 0)
                    {
                        characterSkill.LastUse = characterSkill.LastUse.AddMilliseconds((reducedCooldown) * -1 * 100);
                    }

                    if (!session.Character.HasGodMode)
                    {
                        session.Character.DecreaseMp(characterSkill.MpCost());
                    }

                    characterSkill.LastUse = DateTime.Now;
                    Observable.Timer(TimeSpan.FromMilliseconds(characterSkill.Skill.CastTime * 100)).SafeSubscribe(o =>
                    {
                        if (session == null)
                        {
                            return;
                        }

                        session.CurrentMapInstance.Broadcast($"bs 1 {session.Character.CharacterId} {x} {y} {characterSkill.Skill.SkillVNum}" +
                            $" {(short)(characterSkill.Skill.Cooldown - reducedCooldown)} {characterSkill.Skill.AttackAnimation}" +
                            $" {characterSkill.Skill.Effect} 0 0 1 1 0 0 0");

                        byte Range = characterSkill.TargetRange();
                        if (characterSkill.GetSkillBCards().Any(s => s.Type == (byte)CardType.FalconSkill && s.SubType == (byte)AdditionalTypes.FalconSkill.FalconFocusLowestHP))
                        {
                            if (session.CurrentMapInstance?.BattleEntities?.Where(s => s != null && s.IsInRange(x, y, Range)
                                && session.Character.BattleEntity.CanAttackEntity(s)).OrderBy(s => s.Hp).FirstOrDefault() is BattleEntity lowestHPEntity)
                            {
                                session.Character.MTListTargetQueue.Push(new MTListHitTarget(lowestHPEntity.UserType, lowestHPEntity.MapEntityId, (TargetHitType)characterSkill.Skill.HitType));
                            }
                        }
                        else if (session.Character.MTListTargetQueue.Count == 0)
                        {
                            session?.CurrentMapInstance?.BattleEntities?
                            .Where(s => s.IsInRange(x, y, Range) && session.Character.BattleEntity.CanAttackEntity(s))
                            .ToList().ForEach(s => session.Character.MTListTargetQueue.Push(new MTListHitTarget(s.UserType, s.MapEntityId, (TargetHitType)characterSkill.Skill.HitType)));
                        }

                        int count = 0;

                        //foreach (long id in Session.Character.MTListTargetQueue.Where(s => s.EntityType == UserType.Monster).Select(s => s.TargetId))
                        foreach (long id in session.Character.GetMTListTargetQueue_QuickFix(characterSkill, UserType.Monster))
                        {
                            MapMonster mon = session?.CurrentMapInstance?.GetMonsterById(id);
                            if (mon?.CurrentHp > 0 && mon?.Owner?.MapEntityId != session.Character.CharacterId)
                            {
                                count++;
                                mon.HitQueue.Enqueue(new HitRequest(TargetHitType.SingleAOETargetHit, session,
                                    characterSkill.Skill, characterSkill.Skill.Effect, x, y, showTargetAnimation: count == 0, skillBCards: characterSkill.GetSkillBCards()));
                            }
                        }

                        //foreach (long id in Session.Character.MTListTargetQueue.Where(s => s.EntityType == UserType.Player).Select(s => s.TargetId))
                        foreach (long id in session.Character.GetMTListTargetQueue_QuickFix(characterSkill, UserType.Player))
                        {
                            ClientSession character = ServerManager.Instance.GetSessionByCharacterId(id);
                            if (character != null && character.CurrentMapInstance == session.CurrentMapInstance
                                                  && character.Character.CharacterId != session.Character.CharacterId)
                            {
                                if (session.Character.BattleEntity.CanAttackEntity(character.Character.BattleEntity))
                                {
                                    count++;
                                    session.PvpHit(
                                        new HitRequest(TargetHitType.SingleAOETargetHit, session, characterSkill.Skill, characterSkill.Skill.Effect, x, y, showTargetAnimation: count == 0, skillBCards: characterSkill.GetSkillBCards()),
                                        character);
                                }
                            }
                        }

                        characterSkill.GetSkillBCards().ToList().Where(s =>
                           (s.Type.Equals((byte)CardType.Buff) && new Buff((short)s.SecondData, session.Character.Level).Card.BuffType.Equals(BuffType.Good))
                        || (s.Type.Equals((byte)CardType.FalconSkill) && s.SubType.Equals((byte)AdditionalTypes.FalconSkill.CausingChanceLocation))
                        || (s.Type.Equals((byte)CardType.FearSkill) && s.SubType.Equals((byte)AdditionalTypes.FearSkill.ProduceWhenAmbushe))).ToList()
                        .ForEach(s => s.ApplyBCards(session.Character.BattleEntity, session.Character.BattleEntity, x, y, partnerBuffLevel: characterSkill.TattooLevel));

                        session.Character.MTListTargetQueue.Clear();
                    });

                    Observable.Timer(TimeSpan.FromMilliseconds((short)(characterSkill.Skill.Cooldown - reducedCooldown) * 100)).Subscribe(o =>
                    {
                        if (session?.Character == null)
                        {
                            return;
                        }

                        var skill = session.Character.GetSkills().Find(s => s.Skill?.CastId == castingId && (s.Skill?.UpgradeSkill == 0 || s?.Skill?.SkillType == 1));
                        if (skill != null && skill.LastUse.AddMilliseconds((short)(characterSkill.Skill.Cooldown - reducedCooldown) * 100 - 100) <= DateTime.Now)
                        {
                            session.SendPacket(StaticPacketHelper.SkillReset(castingId));
                        }
                    });
                }
                else
                {
                    session.SendPacket(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("NOT_ENOUGH_MP"), 10));
                    session.SendPacket(StaticPacketHelper.Cancel(2));
                }
            }
            else
            {
                session.SendPacket(StaticPacketHelper.Cancel(2));
            }
        }
    }
}
