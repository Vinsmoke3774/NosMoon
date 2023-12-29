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
using OpenNos.Data;
using OpenNos.Domain;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using NosByte.Shared;
using OpenNos.GameObject.Npc;
using static OpenNos.Domain.BCardType;
using OpenNos.GameObject.Algorithms.Geography;
using OpenNos.GameObject.Algorithms;
using OpenNos.Core.Logger;

namespace OpenNos.GameObject
{
    public class MapNpc : MapNpcDTO
    {
        #region Members

        public NpcMonster Npc;

        private int _movetime;

        private Random _random;

        #endregion

        #region Instantiation

        public MapNpc()
        {
            PVELockObject = new object();
            OnSpawnEvents = new List<EventContainer>();
        }

        public MapNpc(MapNpcDTO input) : this()
        {
            Dialog = input.Dialog;
            Effect = input.Effect;
            EffectDelay = input.EffectDelay;
            IsDisabled = input.IsDisabled;
            IsMoving = input.IsMoving;
            IsSitting = input.IsSitting;
            MapId = input.MapId;
            MapNpcId = input.MapNpcId;
            MapX = input.MapX;
            MapY = input.MapY;
            Name = input.Name;
            NpcVNum = input.NpcVNum;
            Position = input.Position;
        }

        #endregion

        #region Properties

        public int AliveTime { get; set; }

        public BattleEntity BattleEntity { get; set; }

        public ThreadSafeSortedList<short, Buff> Buff => BattleEntity.Buffs;

        public ThreadSafeSortedList<short, IDisposable> BuffObservables => BattleEntity.BuffObservables;

        public double CurrentHp { get; set; }

        public double CurrentMp { get; set; }

        public DateTime Death { get; set; }

        public bool EffectActivated { get; set; }

        public short FirstX { get; set; }

        public short FirstY { get; set; }

        public bool Invisible { get; private set; }

        public bool IsAlive { get; set; }

        public bool IsHostile { get; set; }

        public bool IsMate { get; set; }

        public bool IsOut { get; set; }

        public bool IsProtected { get; set; }

        public bool IsTsReward { get; set; }

        public DateTime LastDefence { get; set; }

        public DateTime LastEffect { get; set; }

        public DateTime LastMonsterAggro { get; set; }

        public DateTime LastMove { get; private set; }

        public DateTime LastProtectedEffect { get; private set; }

        public DateTime LastSkill { get; private set; }

        public IDisposable LifeEvent { get; set; }

        public MapInstance MapInstance { get; set; }

        public double MaxHp { get; set; }

        public double MaxMp { get; set; }

        public List<EventContainer> OnDeathEvents => BattleEntity.OnDeathEvents;

        public List<EventContainer> OnSpawnEvents { get; set; }

        public BattleEntity Owner { get; set; }

        public List<Tile> Path { get; set; }

        public object PVELockObject { get; set; }

        public List<Recipe> Recipes { get; set; }

        public short RunToX { get; set; }

        public short RunToY { get; set; }

        public byte Score { get; set; }

        public Shop Shop { get; set; }

        public bool? ShouldRespawn { get; set; }

        public List<NpcMonsterSkill> Skills { get; set; }

        public bool Started { get; internal set; }

        public long Target { get; set; }

        public List<TeleporterDTO> Teleporters { get; set; }

        #endregion

        #region Methods

        public void AddBuff(Buff indicator, BattleEntity battleEntity)
        {
            BattleEntity.AddBuff(indicator, battleEntity);
        }

        public void DisableBuffs(BuffType type, int level = 100)
        {
            BattleEntity.DisableBuffs(type, level);
        }

        public void DisableBuffs(List<BuffType> types, int level = 100)
        {
            BattleEntity.DisableBuffs(types, level);
        }

        public string GenerateIn(InRespawnType respawnType = InRespawnType.NoEffect)
        {
            NpcMonster npcinfo = ServerManager.GetNpcMonster(NpcVNum);
            if (npcinfo == null || IsDisabled || !IsAlive)
            {
                return "";
            }
            IsOut = false;
            return StaticPacketHelper.In(UserType.Npc, Npc.OriginalNpcMonsterVNum > 0 ? Npc.OriginalNpcMonsterVNum : NpcVNum, MapNpcId, MapX, MapY, Position, 100, 100, (short)Dialog, respawnType, IsSitting, string.IsNullOrEmpty(Name) ? "-" : Name, Invisible);
        }

        public string GenerateOut()
        {
            NpcMonster npcinfo = ServerManager.GetNpcMonster(NpcVNum);
            if (npcinfo == null || IsDisabled)
            {
                return "";
            }
            IsOut = true;
            return $"out 2 {MapNpcId}";
        }

        public string GenerateSay(string message, int type) => $"say 2 {MapNpcId} 2 {message}";

        public int[] GetBuff(CardType type, byte subtype) => BattleEntity.GetBuff(type, subtype);

        public string GetNpcDialog() => $"npc_req 2 {MapNpcId} {Dialog}";

        public bool HasBuff(CardType type, byte subtype) => BattleEntity.HasBuff(type, subtype);

        public void Initialize(MapInstance currentMapInstance)
        {
            MapInstance = currentMapInstance;
            Initialize();
        }

        public void Initialize()
        {
            if (MapInstance.MapInstanceType == MapInstanceType.BaseMapInstance && ServerManager.Instance.MapBossVNums.Contains(NpcVNum))
            {
                MapCell randomCell = MapInstance.Map.GetRandomPosition();
                if (randomCell != null)
                {
                    if (MapInstance.Portals.Any(s => Map.GetDistance(new MapCell { X = s.SourceX, Y = s.SourceY }, new MapCell { X = randomCell.X, Y = randomCell.Y }) < 5))
                    {
                        randomCell = MapInstance.Map.GetRandomPosition();
                    }
                    MapX = randomCell.X;
                    MapY = randomCell.Y;
                }
            }

            _random = new(MapNpcId);
            Npc = ServerManager.GetNpcMonster(NpcVNum);
            MaxHp = Npc.MaxHP;
            MaxMp = Npc.MaxMP;

            if (MapInstance?.MapInstanceType == MapInstanceType.TimeSpaceInstance)
            {
                if (IsProtected)
                {
                    MaxHp *= 8;
                    MaxMp *= 8;
                }
            }
            IsAlive = true;
            CurrentHp = MaxHp;
            CurrentMp = MaxMp;
            LastEffect = DateTime.Now;
            LastProtectedEffect = DateTime.Now;
            LastMove = DateTime.Now;
            LastSkill = DateTime.Now;
            IsHostile = Npc.IsHostile;
            ShouldRespawn = ShouldRespawn ?? true;
            FirstX = MapX;
            FirstY = MapY;
            Score = Score;
            EffectActivated = true;
            _movetime = ServerManager.RandomNumber(500, 3000);
            Path = new();
            Recipes = ServerManager.Instance.GetRecipesByMapNpcId(MapNpcId);
            Target = -1;
            Teleporters = ServerManager.Instance.GetTeleportersByNpcVNum(MapNpcId);
            Shop shop = ServerManager.Instance.GetShopByMapNpcId(MapNpcId);
            if (shop != null)
            {
                shop.Initialize();
                Shop = shop;
            }
            Skills = new List<NpcMonsterSkill>();
            foreach (NpcMonsterSkill ski in Npc.Skills)
            {
                Skills.Add(new NpcMonsterSkill { SkillVNum = ski.SkillVNum, Rate = ski.Rate });
            }
            BattleEntity = new(this);

            if (AliveTime > 0)
            {
                Thread AliveTimeThread = new(() => AliveTimeCheck());
                AliveTimeThread.Start();
            }

            if (NpcVNum == 1408)
            {
                OnDeathEvents.Add(new(MapInstance, EventActionType.SPAWNMONSTER, new MonsterToSummon(621, new MapCell { X = MapX, Y = MapY }, null, move: true)));
            }
            if (NpcVNum == 1409)
            {
                OnDeathEvents.Add(new(MapInstance, EventActionType.SPAWNMONSTER, new MonsterToSummon(622, new MapCell { X = MapX, Y = MapY }, null, move: true)));
            }
            if (NpcVNum == 1410)
            {
                OnDeathEvents.Add(new(MapInstance, EventActionType.SPAWNMONSTER, new MonsterToSummon(623, new MapCell { X = MapX, Y = MapY }, null, move: true)));
            }

            if (OnSpawnEvents.Any())
            {
                OnSpawnEvents.ToList().ForEach(e => { EventHelper.Instance.RunEvent(e, npc: this); });
                OnSpawnEvents.Clear();
            }
        }

        /// <summary>
        /// Check if the Monster is in the given Range.
        /// </summary>
        /// <param name="mapX">The X coordinate on the Map of the object to check.</param>
        /// <param name="mapY">The Y coordinate on the Map of the object to check.</param>
        /// <param name="distance">The maximum distance of the object to check.</param>
        /// <returns>True if the Monster is in range, False if not.</returns>
        public bool IsInRange(short mapX, short mapY, byte distance)
        {
            return Map.GetDistance(new MapCell
            {
                X = mapX,
                Y = mapY
            }, new MapCell
            {
                X = MapX,
                Y = MapY
            }) <= distance;
        }

        public void RemoveBuff(short cardId) => BattleEntity.RemoveBuff(cardId);

        public void RunDeathEvent()
        {
            MapInstance.InstanceBag.NpcsKilled++;
            OnDeathEvents.ForEach(e =>
            {
                if (e.EventActionType == EventActionType.THROWITEMS)
                {
                    Tuple<int, short, byte, int, int> evt = (Tuple<int, short, byte, int, int>)e.Parameter;
                    e.Parameter = new Tuple<int, short, byte, int, int>(MapNpcId, evt.Item2, evt.Item3, evt.Item4, evt.Item5);
                }
                EventHelper.Instance.RunEvent(e);
            });

            if (OnDeathEvents.Any(s => s.EventActionType == EventActionType.SPAWNMONSTERS)
            && (List<MonsterToSummon>)OnDeathEvents.FirstOrDefault(e => e.EventActionType == EventActionType.SPAWNMONSTERS).Parameter is List<MonsterToSummon> summonParameters)
            {
                Parallel.ForEach(summonParameters, npcMonster =>
                {
                    npcMonster.SpawnCell.X = MapX;
                    npcMonster.SpawnCell.Y = MapY;
                });
            }
            if (OnDeathEvents.Any(s => s.EventActionType == EventActionType.SPAWNNPC)
            && (NpcToSummon)OnDeathEvents.FirstOrDefault(e => e.EventActionType == EventActionType.SPAWNNPC).Parameter is NpcToSummon npcMonsterToSummon)
            {
                npcMonsterToSummon.SpawnCell.X = MapX;
                npcMonsterToSummon.SpawnCell.Y = MapY;
            }

            OnDeathEvents.RemoveAll(s => s != null);
        }

        public void SetDeathStatement()
        {
            if (Npc.BCards.Any(s => s.Type == (byte)CardType.NoDefeatAndNoDamage && s.SubType == (byte)AdditionalTypes.NoDefeatAndNoDamage.DecreaseHPNoDeath && s.FirstData == 1))
            {
                CurrentHp = MaxHp;
                return;
            }
            IsAlive = false;
            CurrentHp = 0;
            CurrentMp = 0;
            Death = DateTime.Now;
            LastMove = DateTime.Now;
            DisableBuffs(BuffType.All);

            // Respawn
            if (ShouldRespawn != null && !ShouldRespawn.Value)
            {
                MapInstance.RemoveNpc(this);
                MapInstance.Broadcast(GenerateOut());
            }
        }

        /// <summary>
        /// Remove the current Target from Npc.
        /// </summary>
        internal void RemoveTarget()
        {
            if (Target != -1)
            {
                Path.Clear();
                Target = -1;

                //return to origin
                var pathFinder = new PathFinder<Tile>(Heuristic._heuristic, MapInstance.Map.XLength * MapInstance.Map.YLength, MapInstance.Map.IndexMap(), MapInstance.Map.NeighboursManhattanAndDiagonal());
                Path = pathFinder.Path(MapInstance.Map.Tiles[MapX, MapY], MapInstance.Map.Tiles[FirstX, FirstY]).ToList();
            }
        }

        internal void StartLife()
        {
            try
            {
                if (!MapInstance.IsSleeping)
                {
                    npcLife();
                }
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
            }
        }

        private void AliveTimeCheck()
        {
            double PercentPerSecond = 100 / (double)AliveTime;
            for (int i = 0; i < AliveTime; i++)
            {
                if (!IsAlive || CurrentHp <= 0)
                {
                    return;
                }
                CurrentHp -= MaxHp * PercentPerSecond / 100;
                if (CurrentHp <= 0)
                {
                    break;
                }
                Thread.Sleep(1000);
            }
            MapInstance.Broadcast(StaticPacketHelper.Out(UserType.Npc, MapNpcId));
            MapInstance.RemoveNpc(this);
        }

        private void npcLife()
        {
            if (!MapInstance.Sessions.Any()) return;

            // Respawn
            if (CurrentHp <= 0 && ShouldRespawn != null && !ShouldRespawn.Value)
            {
                MapInstance.RemoveNpc(this);
                MapInstance.Broadcast(GenerateOut());
            }

            if (!IsAlive && ShouldRespawn != null && ShouldRespawn.Value)
            {
                double timeDeath = (DateTime.Now - Death).TotalSeconds;
                if (timeDeath >= Npc.RespawnTime / 10d)
                {
                    Respawn();
                }
            }

            if (LastProtectedEffect.AddMilliseconds(6000) <= DateTime.Now)
            {
                LastProtectedEffect = DateTime.Now;
                if (IsMate || IsProtected)
                {
                    if (NpcVNum != 1457)
                    {
                        MapInstance.Broadcast(StaticPacketHelper.GenerateEff(UserType.Npc, MapNpcId, 825), MapX, MapY);
                    }
                    else
                    {
                        MapInstance.Broadcast(StaticPacketHelper.GenerateEff(UserType.Npc, MapNpcId, 7739), MapX, MapY);
                    }
                }
            }

            double time = (DateTime.Now - LastEffect).TotalMilliseconds;

            if (EffectDelay > 0)
            {
                if (time > EffectDelay)
                {
                    if (Effect > 0 && EffectActivated)
                    {
                        MapInstance.Broadcast(StaticPacketHelper.GenerateEff(UserType.Npc, MapNpcId, Effect), MapX, MapY);
                    }

                    LastEffect = DateTime.Now;
                }
            }

            time = (DateTime.Now - LastMove).TotalMilliseconds;
            if (Target == -1 && IsMoving && Npc.Speed > 0 && time > _movetime && !HasBuff(CardType.Move, (byte)AdditionalTypes.Move.MovementImpossible))
            {
                _movetime = ServerManager.RandomNumber(500, 3000);
                byte point = (byte)ServerManager.RandomNumber(2, 4);
                byte fpoint = (byte)ServerManager.RandomNumber(0, 2);

                byte xpoint = (byte)ServerManager.RandomNumber(fpoint, point);
                byte ypoint = (byte)(point - xpoint);

                short mapX = FirstX;
                short mapY = FirstY;

                if (MapInstance.Map.GetFreePosition(ref mapX, ref mapY, xpoint, ypoint))
                {
                    double value = (xpoint + ypoint) / (double)(2 * Npc.Speed);
                    Observable.Timer(TimeSpan.FromMilliseconds(1000 * value)).Subscribe(x =>
                    {
                        MapX = mapX;
                        MapY = mapY;
                    });
                    LastMove = DateTime.Now.AddSeconds(value);
                    MapInstance.Broadcast(new BroadcastPacket(null, PacketFactory.Serialize(StaticPacketHelper.Move(UserType.Npc, MapNpcId, MapX, MapY, Npc.Speed)), ReceiverType.All, xCoordinate: mapX, yCoordinate: mapY));
                }
            }
            if (Target == -1)
            {
                if (IsHostile && Shop == null)
                {
                    MapMonster monster = MapInstance.Monsters.Find(s => MapInstance == s.MapInstance && Map.GetDistance(new MapCell { X = MapX, Y = MapY }, new MapCell { X = s.MapX, Y = s.MapY }) < (Npc.NoticeRange > 5 ? Npc.NoticeRange / 2 : Npc.NoticeRange));
                    ClientSession session = MapInstance.Sessions.FirstOrDefault(s => MapInstance == s.Character.MapInstance && Map.GetDistance(new MapCell { X = MapX, Y = MapY }, new MapCell { X = s.Character.PositionX, Y = s.Character.PositionY }) < Npc.NoticeRange);

                    if (monster != null && session != null)
                    {
                        Target = monster.MapMonsterId;
                    }
                }
            }
            else if (Target != -1)
            {
                MapMonster monster = MapInstance.Monsters.Find(s => s.MapMonsterId == Target);
                if (monster == null || monster.CurrentHp < 1)
                {
                    Target = -1;
                    return;
                }
                NpcMonsterSkill npcMonsterSkill = null;
                if (ServerManager.RandomNumber(0, 10) > 8)
                {
                    npcMonsterSkill = Skills.Where(s => (DateTime.Now - s.LastSkillUse).TotalMilliseconds >= 100 * s.Skill.Cooldown).OrderBy(rnd => _random.Next()).FirstOrDefault();
                }
                int hitmode = 0;
                bool onyxWings = false;
                bool zephyrWings = false;
                int damage = DamageHelper.Instance.CalculateDamage(new BattleEntity(this), new BattleEntity(monster), npcMonsterSkill?.Skill, ref hitmode, ref onyxWings, ref zephyrWings);
                if (monster.Monster.BCards.Find(s => s.Type == (byte)CardType.LightAndShadow && s.SubType == (byte)AdditionalTypes.LightAndShadow.InflictDamageToMP) is BCard card)
                {
                    int reduce = damage / 100 * card.FirstData;
                    if (monster.CurrentMp < reduce)
                    {
                        reduce = (int)monster.CurrentMp;
                        monster.CurrentMp = 0;
                    }
                    else
                    {
                        monster.DecreaseMp(reduce);
                    }
                    damage -= reduce;
                }
                int distance = Map.GetDistance(new MapCell { X = MapX, Y = MapY }, new MapCell { X = monster.MapX, Y = monster.MapY });
                if (monster.CurrentHp > 0 && ((npcMonsterSkill != null && distance < npcMonsterSkill.Skill.Range) || distance <= Npc.BasicRange) && !HasBuff(CardType.SpecialAttack, (byte)AdditionalTypes.SpecialAttack.NoAttack))
                {
                    if (((DateTime.Now - LastSkill).TotalMilliseconds >= 1000 + (Npc.BasicCooldown * 200)/* && Skills.Count == 0*/) || npcMonsterSkill != null)
                    {
                        if (npcMonsterSkill != null)
                        {
                            npcMonsterSkill.LastSkillUse = DateTime.Now;
                            MapInstance.Broadcast(StaticPacketHelper.CastOnTarget(UserType.Npc, MapNpcId, UserType.Monster, Target, npcMonsterSkill.Skill.CastAnimation, npcMonsterSkill.Skill.CastEffect, npcMonsterSkill.Skill.SkillVNum));
                        }

                        if (npcMonsterSkill != null && npcMonsterSkill.Skill.CastEffect != 0)
                        {
                            MapInstance.Broadcast(StaticPacketHelper.GenerateEff(UserType.Npc, MapNpcId, Effect));
                        }
                        monster.BattleEntity.GetDamage(damage, BattleEntity);
                        lock (monster.DamageList)
                        {
                            if (!monster.DamageList.Any(s => s.Key.MapEntityId == MapNpcId))
                            {
                                monster.AddToAggroList(BattleEntity);
                            }
                        }
                        MapInstance.Broadcast(npcMonsterSkill != null
                            ? StaticPacketHelper.SkillUsed(UserType.Npc, MapNpcId, 3, Target, npcMonsterSkill.SkillVNum, npcMonsterSkill.Skill.Cooldown, npcMonsterSkill.Skill.AttackAnimation, npcMonsterSkill.Skill.Effect, 0, 0, monster.CurrentHp > 0, (int)((float)monster.CurrentHp / (float)monster.MaxHp * 100), damage, hitmode, 0)
                            : StaticPacketHelper.SkillUsed(UserType.Npc, MapNpcId, 3, Target, 0, Npc.BasicCooldown, 11, Npc.BasicSkill, 0, 0, monster.CurrentHp > 0, (int)((float)monster.CurrentHp / (float)monster.MaxHp * 100), damage, hitmode, 0));
                        LastSkill = DateTime.Now;

                        if (npcMonsterSkill?.Skill.TargetType == 1 && npcMonsterSkill?.Skill.HitType == 2)
                        {
                            IEnumerable<ClientSession> clientSessions =
                                           MapInstance.Sessions?.Where(s =>
                                               s.Character.IsInRange(MapX,
                                                   MapY, npcMonsterSkill.Skill.TargetRange));
                            IEnumerable<Mate> mates = MapInstance.GetListMateInRange(MapX, MapY, npcMonsterSkill.Skill.TargetRange);

                            foreach (BCard skillBcard in npcMonsterSkill.Skill.BCards)
                            {
                                if (skillBcard.Type == 25 && skillBcard.SubType == 1 && new Buff((short)skillBcard.SecondData, Npc.Level)?.Card?.BuffType == BuffType.Good)
                                {
                                    if (clientSessions != null)
                                    {
                                        foreach (ClientSession clientSession in clientSessions)
                                        {
                                            if (clientSession.Character != null)
                                            {
                                                if (!BattleEntity.CanAttackEntity(clientSession.Character.BattleEntity))
                                                {
                                                    skillBcard.ApplyBCards(clientSession.Character.BattleEntity, BattleEntity);
                                                }
                                            }
                                        }
                                    }
                                    if (mates != null)
                                    {
                                        foreach (Mate mate in mates)
                                        {
                                            if (!BattleEntity.CanAttackEntity(mate.BattleEntity))
                                            {
                                                skillBcard.ApplyBCards(mate.BattleEntity, BattleEntity);
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if (monster.CurrentHp < 1 && monster.SetDeathStatement())
                        {
                            monster.RunDeathEvent();
                            RemoveTarget();
                        }
                    }
                }
                else
                {
                    int maxdistance = Npc.NoticeRange > 5 ? Npc.NoticeRange / 2 : Npc.NoticeRange;
                    if (IsMoving && !HasBuff(CardType.Move, (byte)AdditionalTypes.Move.MovementImpossible))
                    {
                        const short maxDistance = 5;
                        int maxindex = Path.Count > Npc.Speed / 2 && Npc.Speed > 1 ? Npc.Speed / 2 : Path.Count;
                        if (maxindex < 1)
                        {
                            maxindex = 1;
                        }
                        if ((Path.Count == 0 && distance >= 1 && distance < maxDistance) || (Path.Count >= maxindex && maxindex > 0))
                        {
                            short xoffset = (short)ServerManager.RandomNumber(-1, 1);
                            short yoffset = (short)ServerManager.RandomNumber(-1, 1);

                            //go to monster
                            var pathFinder = new PathFinder<Tile>(Heuristic._heuristic, MapInstance.Map.XLength * MapInstance.Map.YLength, MapInstance.Map.IndexMap(), MapInstance.Map.NeighboursManhattanAndDiagonal());
                            Path = pathFinder.Path(MapInstance.Map.Tiles[MapX, MapY], MapInstance.Map.Tiles[(short)(monster.MapX + xoffset), (short)(monster.MapY + yoffset)]).ToList();
                            maxindex = Path.Count > Npc.Speed / 2 && Npc.Speed > 1 ? Npc.Speed / 2 : Path.Count;
                        }
                        if (DateTime.Now > LastMove && Npc.Speed > 0 && Path.Count > 0)
                        {
                            byte speedIndex = (byte)(Npc.Speed / 2.5 < 1 ? 1 : Npc.Speed / 2.5);
                            maxindex = Path.Count > speedIndex ? speedIndex : Path.Count;

                            //short mapX = (short)ServerManager.RandomNumber(Path[maxindex - 1].X - 1, Path[maxindex - 1].X + 1);
                            //short mapY = (short)_random.Next(Path[maxindex - 1].Y - 1, Path[maxindex - 1].Y + 1);

                            short mapX = Path[maxindex - 1].X;
                            short mapY = Path[maxindex - 1].Y;
                            double waitingtime = Map.GetDistance(new MapCell { X = mapX, Y = mapY }, new MapCell { X = MapX, Y = MapY }) / (double)Npc.Speed;
                            MapInstance.Broadcast(new BroadcastPacket(null, PacketFactory.Serialize(StaticPacketHelper.Move(UserType.Npc, MapNpcId, mapX, mapY, Npc.Speed)), ReceiverType.All, xCoordinate: mapX, yCoordinate: mapY));
                            LastMove = DateTime.Now.AddSeconds(waitingtime > 1 ? 1 : waitingtime);

                            Observable.Timer(TimeSpan.FromMilliseconds((int)((waitingtime > 1 ? 1 : waitingtime) * 1000))).SafeSubscribe(x =>
                            {
                                MapX = mapX;
                                MapY = mapY;
                            });

                            Path.RemoveRange(0, maxindex);
                        }
                        if (Target != -1 && (MapId != monster.MapId || distance > maxDistance))
                        {
                            RemoveTarget();
                        }
                    }
                }
            }
        }

        private void Respawn()
        {
            if (Npc != null)
            {
                DisableBuffs(BuffType.All);
                IsAlive = true;
                Target = -1;
                MaxHp = Npc.MaxHP;
                MaxMp = Npc.MaxMP;
                CurrentHp = MaxHp;
                CurrentMp = MaxMp;
                MapX = FirstX;
                MapY = FirstY;
                if (MapInstance.MapInstanceType == MapInstanceType.BaseMapInstance && ServerManager.Instance.MapBossVNums.Contains(NpcVNum))
                {
                    MapCell randomCell = MapInstance.Map.GetRandomPosition();
                    if (randomCell != null)
                    {
                        MapX = randomCell.X;
                        MapY = randomCell.Y;
                    }
                }
                Path = new();
                MapInstance.Broadcast(GenerateIn());
                Npc.BCards.Where(s => s.Type != 25).ToList().ForEach(s => s.ApplyBCards(BattleEntity, BattleEntity));
            }
        }

        #endregion
    }
}