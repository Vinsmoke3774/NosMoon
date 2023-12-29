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

using NosByte.Shared;
using OpenNos.Core;
using OpenNos.Core.Logger;
using OpenNos.Data;
using OpenNos.Domain;
using OpenNos.GameObject.Algorithms;
using OpenNos.GameObject.Algorithms.Geography;
using OpenNos.GameObject.Event;
using OpenNos.GameObject.Event.ARENA;
using OpenNos.GameObject.Extension.CharacterExtensions;
using OpenNos.GameObject.HttpClients;
using OpenNos.GameObject.Networking;
using OpenNos.Master.Library.Client;
using OpenNos.Master.Library.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OpenNos.GameObject.Helpers
{
    public class EventHelper : Singleton<EventHelper>
    {
        #region Members

        private static readonly KeepAliveClient _keepAliveClient = KeepAliveClient.Instance;

        #endregion

        #region Properties

        #endregion

        #region Methods

        public static int CalculateComboPoint(int n)
        {
            int a = 4;
            int b = 7;
            for (int i = 0; i < n; i++)
            {
                int temp = a;
                a = b;
                b = temp + b;
            }
            return a;
        }

        public static void GenerateEvent(EventType type, int lodChannelId = -1, int LvlBracket = -1)
        {
            try
            {

                if (!ServerManager.Instance.StartedEvents.Any(s => s == type))
                {
                    Task.Factory.StartNew(() =>
                    {
                        ServerManager.Instance.StartedEvents.Add(type);
                        switch (type)
                        {
                            case EventType.RAINBOWBATTLE:
                                if (ServerManager.Instance.ChannelId != 51)
                                {
                                    Event.RAINBOWBATTLE.RainbowBattle.GenerateEvent();
                                }
                                else
                                {
                                    ServerManager.Instance.StartedEvents.Remove(type);
                                    return;
                                }
                                break;

                            case EventType.TALENTARENA:
                                TalentArenaEvent.Run();
                                break;

                            case EventType.UNRANKEDTALENTARENA:
                                UnrankedArenaEvent.Run();
                                break;

                            case EventType.RANKINGREFRESH:
                                ServerManager.Instance.RefreshRanking();
                                ServerManager.Instance.StartedEvents.Remove(type);
                                break;

                            case EventType.MINILANDREFRESHEVENT:
                                MinilandRefresh.GenerateMinilandEvent();
                                ServerManager.Instance.StartedEvents.Remove(type);
                                break;

                            case EventType.ACT4SHIP:
                                //ACT4SHIP.GenerateAct4Ship(1);
                                //ACT4SHIP.GenerateAct4Ship(2);
                                break;

                            case EventType.CALIGOR:
                                if (ServerManager.Instance.ChannelId == 51)
                                {
                                    CaligorRaid.Run();
                                }
                                else
                                {
                                    ServerManager.Instance.StartedEvents.Remove(type);
                                    return;
                                }
                                break;
                        }
                    });
                }

            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
            }
        }

        public static TimeSpan GetMilisecondsBeforeTime(TimeSpan time)
        {
            TimeSpan now = TimeSpan.Parse(DateTime.Now.ToString("HH:mm"));
            TimeSpan timeLeftUntilFirstRun = time - now;
            if (timeLeftUntilFirstRun.TotalHours < 0)
            {
                timeLeftUntilFirstRun += new TimeSpan(24, 0, 0);
            }
            return timeLeftUntilFirstRun;
        }

        public void RunEvent(EventContainer evt, ClientSession session = null, MapMonster monster = null, MapNpc npc = null)
        {
            if (evt != null)
            {
                if (session != null)
                {
                    evt.MapInstance = session.CurrentMapInstance;
                    switch (evt.EventActionType)
                    {
                        #region EventForUser

                        case EventActionType.NPCDIALOG:
                            session.SendPacket(session.Character.GenerateNpcDialog((int)evt.Parameter));
                            break;

                        case EventActionType.SENDPACKET:
                            session.SendPacket((string)evt.Parameter);
                            break;

                        case EventActionType.SENDPACKETAFTER:
                            session.SendPacketAfter((string)evt.Parameter, (int)evt.OptionalParameter);
                            break;

                        case EventActionType.NPCDIALOGEND:
                            Thread.Sleep((int)evt.Parameter);
                            session.SendPacket(session.Character.GenerateNpcDialogEnd());
                            break;

                            #endregion
                    }
                }
                if (evt.MapInstance != null)
                {
                    switch (evt.EventActionType)
                    {
                        #region EventForUser

                        case EventActionType.NPCDIALOG:
                        case EventActionType.SENDPACKET:
                        case EventActionType.SENDPACKETAFTER:
                            if (session == null)
                            {
                                evt.MapInstance.Sessions.ToList().ForEach(e => RunEvent(evt, e));
                            }
                            break;

                        #endregion

                        #region MapInstanceEvent

                        case EventActionType.REGISTEREVENT:
                            Tuple<string, List<EventContainer>> even = (Tuple<string, List<EventContainer>>)evt.Parameter;
                            switch (even.Item1)
                            {
                                case "OnCharacterDiscoveringMap":
                                    even.Item2.ForEach(s => evt.MapInstance.OnCharacterDiscoveringMapEvents.Add(new Tuple<EventContainer, List<long>>(s, new List<long>())));
                                    break;

                                case "OnMoveOnMap":
                                    evt.MapInstance.OnMoveOnMapEvents.AddRange(even.Item2);
                                    break;

                                case "OnMapClean":
                                    evt.MapInstance.OnMapClean.AddRange(even.Item2);
                                    break;

                                case "OnLockerOpen":
                                    evt.MapInstance.UnlockEvents.AddRange(even.Item2);
                                    break;
                            }
                            break;

                        case EventActionType.REGISTERWAVE:
                            evt.MapInstance.WaveEvents.Add((EventWave)evt.Parameter);
                            break;

                        case EventActionType.SETAREAENTRY:
                            ZoneEvent even2 = (ZoneEvent)evt.Parameter;
                            evt.MapInstance.OnAreaEntryEvents.Add(even2);
                            break;

                        case EventActionType.REMOVEMONSTERLOCKER:
                            EventContainer evt2 = (EventContainer)evt.Parameter;
                            if (evt.MapInstance.InstanceBag.MonsterLocker.Current > 0)
                            {
                                evt.MapInstance.InstanceBag.MonsterLocker.Current--;
                            }
                            if (evt.MapInstance.InstanceBag.MonsterLocker.Current == 0 && evt.MapInstance.InstanceBag.ButtonLocker.Current == 0)
                            {
                                List<EventContainer> UnlockEventsCopy = evt.MapInstance.UnlockEvents.ToList();
                                UnlockEventsCopy.ForEach(e => RunEvent(e));
                                evt.MapInstance.UnlockEvents.RemoveAll(s => s != null && UnlockEventsCopy.Contains(s));
                            }
                            break;

                        case EventActionType.REMOVEBUTTONLOCKER:
                            evt2 = (EventContainer)evt.Parameter;
                            if (evt.MapInstance.InstanceBag.ButtonLocker.Current > 0)
                            {
                                evt.MapInstance.InstanceBag.ButtonLocker.Current--;
                            }
                            if (evt.MapInstance.InstanceBag.MonsterLocker.Current == 0 && evt.MapInstance.InstanceBag.ButtonLocker.Current == 0)
                            {
                                List<EventContainer> UnlockEventsCopy = evt.MapInstance.UnlockEvents.ToList();
                                UnlockEventsCopy.ForEach(e => RunEvent(e));
                                evt.MapInstance.UnlockEvents.RemoveAll(s => s != null && UnlockEventsCopy.Contains(s));
                            }
                            break;

                        case EventActionType.EFFECT:
                            {
                                Tuple<short, int> effectTuple = (Tuple<short, int>)evt.Parameter;

                                short effectId = effectTuple.Item1;
                                int delay = effectTuple.Item2;

                                Observable.Timer(TimeSpan.FromMilliseconds(delay)).SafeSubscribe(obs =>
                                {
                                    if (monster != null)
                                    {
                                        monster.LastEffect = DateTime.Now;
                                        evt.MapInstance.Broadcast(StaticPacketHelper.GenerateEff(UserType.Monster, monster.MapMonsterId, effectId));
                                    }
                                    else
                                    {
                                        evt.MapInstance.Sessions.Where(s => s?.Character != null).ToList()
                                            .ForEach(s => s.SendPacket(StaticPacketHelper.GenerateEff(UserType.Player, s.Character.CharacterId, effectId)));
                                    }
                                });
                            }
                            break;

                        case EventActionType.CONTROLEMONSTERINRANGE:
                            if (monster != null)
                            {
                                Tuple<short, byte, List<EventContainer>> evnt = (Tuple<short, byte, List<EventContainer>>)evt.Parameter;
                                List<MapMonster> MapMonsters = evt.MapInstance.GetMonsterInRangeList(monster.MapX, monster.MapY, evnt.Item2);
                                if (evnt.Item1 != 0)
                                {
                                    MapMonsters.RemoveAll(s => s.MonsterVNum != evnt.Item1);
                                }
                                MapMonsters.ForEach(s => evnt.Item3.ForEach(e => RunEvent(e, monster: s)));
                            }
                            break;

                        case EventActionType.ONTARGET:
                            if (monster.MoveEvent?.InZone(monster.MapX, monster.MapY) == true)
                            {
                                monster.MoveEvent = null;
                                monster.Path = new();
                                ((List<EventContainer>)evt.Parameter).ForEach(s => RunEvent(s, monster: monster));
                            }
                            break;

                        case EventActionType.MOVE:
                            ZoneEvent evt4 = (ZoneEvent)evt.Parameter;
                            if (monster != null)
                            {
                                monster.MoveEvent = evt4;
                                var pathFinder = new PathFinder<Tile>(Heuristic._heuristic, monster.MapInstance.Map.XLength * monster.MapInstance.Map.YLength, monster.MapInstance.Map.IndexMap(), monster.MapInstance.Map.NeighboursManhattanAndDiagonal());
                                monster.Path = pathFinder.Path(monster.MapInstance.Map.Tiles[monster.MapX, monster.MapY], monster.MapInstance.Map.Tiles[evt4.X, evt4.Y]).ToList();
                                monster.RunToX = evt4.X;
                                monster.RunToY = evt4.Y;
                            }
                            else if (npc != null)
                            {
                                //npc.MoveEvent = evt4;
                                var pathFinder = new PathFinder<Tile>(Heuristic._heuristic, npc.MapInstance.Map.XLength * npc.MapInstance.Map.YLength, npc.MapInstance.Map.IndexMap(), npc.MapInstance.Map.NeighboursManhattanAndDiagonal());
                                monster.Path = pathFinder.Path(monster.MapInstance.Map.Tiles[npc.MapX, npc.MapY], npc.MapInstance.Map.Tiles[evt4.X, evt4.Y]).ToList();
                                npc.RunToX = evt4.X;
                                npc.RunToY = evt4.Y;
                            }
                            break;

                        case EventActionType.STARTACT4RAIDWAVES:
                            IDisposable spawnsDisposable = null;
                            spawnsDisposable = Observable.Interval(TimeSpan.FromSeconds(60)).SafeSubscribe(s =>
                            {
                                int count = evt.MapInstance.Sessions.Count();

                                if (count <= 0)
                                {
                                    spawnsDisposable.Dispose();
                                    return;
                                }

                                if (count > 5)
                                {
                                    count = 5;
                                }
                                List<MonsterToSummon> mobWave = new List<MonsterToSummon>();
                                for (int i = 0; i < count; i++)
                                {
                                    switch (evt.MapInstance.MapInstanceType)
                                    {
                                        case MapInstanceType.Act4Morcos:
                                            mobWave.Add(new MonsterToSummon(561, evt.MapInstance.Map.GetRandomPosition(), null, true));
                                            mobWave.Add(new MonsterToSummon(561, evt.MapInstance.Map.GetRandomPosition(), null, true));
                                            mobWave.Add(new MonsterToSummon(561, evt.MapInstance.Map.GetRandomPosition(), null, true));
                                            mobWave.Add(new MonsterToSummon(562, evt.MapInstance.Map.GetRandomPosition(), null, true));
                                            mobWave.Add(new MonsterToSummon(562, evt.MapInstance.Map.GetRandomPosition(), null, true));
                                            mobWave.Add(new MonsterToSummon(562, evt.MapInstance.Map.GetRandomPosition(), null, true));
                                            mobWave.Add(new MonsterToSummon(851, evt.MapInstance.Map.GetRandomPosition(), null, false));
                                            break;

                                        case MapInstanceType.Act4Hatus:
                                            mobWave.Add(new MonsterToSummon(574, evt.MapInstance.Map.GetRandomPosition(), null, true));
                                            mobWave.Add(new MonsterToSummon(574, evt.MapInstance.Map.GetRandomPosition(), null, true));
                                            mobWave.Add(new MonsterToSummon(575, evt.MapInstance.Map.GetRandomPosition(), null, true));
                                            mobWave.Add(new MonsterToSummon(575, evt.MapInstance.Map.GetRandomPosition(), null, true));
                                            mobWave.Add(new MonsterToSummon(576, evt.MapInstance.Map.GetRandomPosition(), null, true));
                                            mobWave.Add(new MonsterToSummon(576, evt.MapInstance.Map.GetRandomPosition(), null, true));
                                            break;

                                        case MapInstanceType.Act4Calvina:
                                            mobWave.Add(new MonsterToSummon(770, evt.MapInstance.Map.GetRandomPosition(), null, true));
                                            mobWave.Add(new MonsterToSummon(770, evt.MapInstance.Map.GetRandomPosition(), null, true));
                                            mobWave.Add(new MonsterToSummon(770, evt.MapInstance.Map.GetRandomPosition(), null, true));
                                            mobWave.Add(new MonsterToSummon(771, evt.MapInstance.Map.GetRandomPosition(), null, true));
                                            mobWave.Add(new MonsterToSummon(771, evt.MapInstance.Map.GetRandomPosition(), null, true));
                                            mobWave.Add(new MonsterToSummon(771, evt.MapInstance.Map.GetRandomPosition(), null, true));
                                            break;

                                        case MapInstanceType.Act4Berios:
                                            mobWave.Add(new MonsterToSummon(780, evt.MapInstance.Map.GetRandomPosition(), null, true));
                                            mobWave.Add(new MonsterToSummon(781, evt.MapInstance.Map.GetRandomPosition(), null, true));
                                            mobWave.Add(new MonsterToSummon(782, evt.MapInstance.Map.GetRandomPosition(), null, true));
                                            mobWave.Add(new MonsterToSummon(782, evt.MapInstance.Map.GetRandomPosition(), null, true));
                                            mobWave.Add(new MonsterToSummon(783, evt.MapInstance.Map.GetRandomPosition(), null, true));
                                            mobWave.Add(new MonsterToSummon(783, evt.MapInstance.Map.GetRandomPosition(), null, true));
                                            break;
                                    }
                                }
                                evt.MapInstance.SummonMonsters(mobWave);
                            });

                            Observable.Timer(TimeSpan.FromMinutes(30)).SafeSubscribe(x =>
                            {
                                spawnsDisposable?.Dispose();
                            });
                            break;

                        case EventActionType.SETMONSTERLOCKERS:
                            evt.MapInstance.InstanceBag.MonsterLocker.Current = Convert.ToByte(evt.Parameter);
                            evt.MapInstance.InstanceBag.MonsterLocker.Initial = Convert.ToByte(evt.Parameter);
                            break;

                        case EventActionType.SETBUTTONLOCKERS:
                            evt.MapInstance.InstanceBag.ButtonLocker.Current = Convert.ToByte(evt.Parameter);
                            evt.MapInstance.InstanceBag.ButtonLocker.Initial = Convert.ToByte(evt.Parameter);
                            break;

                        case EventActionType.SCRIPTEND:
                            switch (evt.MapInstance.MapInstanceType)
                            {
                                case MapInstanceType.SkyTowerInstance:
                                    evt.MapInstance.InstanceBag.Clock.StopClock();
                                    evt.MapInstance.Clock.StopClock();
                                    evt.MapInstance.InstanceBag.EndState = (byte)evt.Parameter;
                                    ClientSession clients = evt.MapInstance.Sessions.ToList().Where(s => s.Character?.SkyTower != null).FirstOrDefault();
                                    if (clients != null && clients.Character?.SkyTower != null && evt.MapInstance.InstanceBag.EndState != 10)
                                    {
                                        Guid MapInstanceId = ServerManager.GetBaseMapInstanceIdByMapId(clients.Character.MapId);
                                        ScriptedInstance si = ServerManager.Instance.SkyTowers.FirstOrDefault(s => s.Id == clients.Character.SkyTower.Id);
                                        if (si == null)
                                        {
                                            return;
                                        }
                                        byte penalty = 0;
                                        if (penalty > (clients.Character.Level - si.LevelMinimum) * 2)
                                        {
                                            penalty = penalty > 100 ? (byte)100 : penalty;
                                            clients.SendPacket(clients.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("TS_PENALTY"), penalty), 10));
                                        }
                                        int point = evt.MapInstance.InstanceBag.Point * (100 - penalty) / 100;
                                        string perfection = "";
                                        perfection += evt.MapInstance.InstanceBag.MonstersKilled >= si.MonsterAmount ? 1 : 0;
                                        perfection += evt.MapInstance.InstanceBag.NpcsKilled == 0 ? 1 : 0;
                                        perfection += evt.MapInstance.InstanceBag.RoomsVisited >= si.RoomAmount ? 1 : 0;
                                        foreach (MapInstance mapInstance in clients.Character.SkyTower._mapInstanceDictionary.Values)
                                        {
                                            mapInstance.Broadcast($"score {(evt.MapInstance.InstanceBag.EndState)} {point} 27 47 18 {(si.DrawItems?.Count)} {evt.MapInstance.InstanceBag.MonstersKilled} {si.NpcAmount - evt.MapInstance.InstanceBag.NpcsKilled} {evt.MapInstance.InstanceBag.RoomsVisited} {perfection} 1 1");
                                        }

                                        if (evt.MapInstance.InstanceBag.EndState == 5)
                                        {
                                            if (clients.Character.Inventory.Values.FirstOrDefault(s => s.Item.ItemType == ItemType.Special && s.Item.Effect == 140 && s.Item.EffectValue == si.Id) is ItemInstance tsStone)
                                            {
                                                clients.Character.Inventory.RemoveItemFromInventory(tsStone.Id);
                                            }
                                            ClientSession[] tsmembers = new ClientSession[40];
                                            clients.Character.SkyTower._mapInstanceDictionary.SelectMany(s => s.Value?.Sessions).ToList().CopyTo(tsmembers);
                                            foreach (ClientSession targetSession in tsmembers)
                                            {
                                                if (targetSession != null)
                                                {
                                                    targetSession.Character.IncrementQuests(QuestType.TimesSpace, si.QuestTimeSpaceId);
                                                }
                                            }
                                        }

                                        ClientSession[] tsmembers1 = new ClientSession[40];
                                        clients.Character.Timespace._mapInstanceDictionary.SelectMany(s => s.Value?.Sessions).ToList().CopyTo(tsmembers1);
                                        bool isFailed = evt.MapInstance.InstanceBag.EndState == 2 || evt.MapInstance.InstanceBag.EndState == 3 || evt.MapInstance.InstanceBag.EndState == 1;
                                        foreach (ClientSession targetSession in tsmembers1)
                                        {
                                            if (targetSession != null && !isFailed)
                                            {
                                                session.Character.SkyTowerLevel = si.Round + 1;
                                                // Idk if the skytower is supposed to be a real timespace
                                                //ServerManager.Instance.TimespaceSessions.Remove(targetSession);
                                            }
                                        }

                                        ScriptedInstance ClientTimeSpace = clients.Character.SkyTower;
                                        Observable.Timer(TimeSpan.FromSeconds(30)).SafeSubscribe(o =>
                                        {
                                            ClientSession[] tsmembers = new ClientSession[40];
                                            ClientTimeSpace._mapInstanceDictionary.SelectMany(s => s.Value?.Sessions).ToList().CopyTo(tsmembers);
                                            foreach (ClientSession targetSession in tsmembers)
                                            {
                                                if (targetSession != null)
                                                {
                                                    if (targetSession.Character.Hp <= 0)
                                                    {
                                                        targetSession.Character.Hp = 1;
                                                        targetSession.Character.Mp = 1;
                                                    }
                                                }
                                            }
                                            ClientTimeSpace._mapInstanceDictionary.Values.ToList().ForEach(m => m.Dispose());
                                        });
                                    }
                                    break;

                                case MapInstanceType.TimeSpaceInstance:
                                    evt.MapInstance.InstanceBag.Clock.StopClock();
                                    evt.MapInstance.Clock.StopClock();
                                    evt.MapInstance.InstanceBag.EndState = (byte)evt.Parameter;
                                    ClientSession client = evt.MapInstance.Sessions.ToList().Where(s => s.Character?.Timespace != null).FirstOrDefault();
                                    if (client != null && client.Character?.Timespace != null && evt.MapInstance.InstanceBag.EndState != 10)
                                    {
#warning Check EndState and monsters to kill
                                        Guid MapInstanceId = ServerManager.GetBaseMapInstanceIdByMapId(client.Character.MapId);
                                        ScriptedInstance si = ServerManager.Instance.TimeSpaces.FirstOrDefault(s => s.Id == client.Character.Timespace.Id);
                                        if (si == null)
                                        {
                                            return;
                                        }
                                        byte penalty = 0;
                                        if (penalty > (client.Character.Level - si.LevelMinimum) * 2)
                                        {
                                            penalty = penalty > 100 ? (byte)100 : penalty;
                                            client.SendPacket(client.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("TS_PENALTY"), penalty), 10));
                                        }
                                        int point = evt.MapInstance.InstanceBag.Point * (100 - penalty) / 100;
                                        string perfection = "";
                                        perfection += evt.MapInstance.InstanceBag.MonstersKilled >= si.MonsterAmount ? 1 : 0;
                                        perfection += evt.MapInstance.InstanceBag.NpcsKilled == 0 ? 1 : 0;
                                        perfection += evt.MapInstance.InstanceBag.RoomsVisited >= si.RoomAmount ? 1 : 0;
                                        foreach (MapInstance mapInstance in client.Character.Timespace._mapInstanceDictionary.Values)
                                        {
                                            var tshighest = ServerManager.Instance.TimespaceLogs.OrderByDescending(s => s.Score).FirstOrDefault(s => s.ScriptedInstanceId == si.ScriptedInstanceId);
                                            var preventNull = tshighest == null ? 0 : tshighest.Score;
                                            mapInstance.Broadcast($"score {(point > preventNull || tshighest == null ? 6 : evt.MapInstance.InstanceBag.EndState)} {point} 27 47 18 {(client.Character.Timespace.IsLoserMode ? 0 : si.DrawItems?.Count ?? 0)} {evt.MapInstance.InstanceBag.MonstersKilled} {si.NpcAmount - evt.MapInstance.InstanceBag.NpcsKilled} {evt.MapInstance.InstanceBag.RoomsVisited} {perfection} 1 1");
                                        }

                                        if (evt.MapInstance.InstanceBag.EndState == 5)
                                        {
                                            if (client.Character.Inventory.Values.FirstOrDefault(s => s.Item.ItemType == ItemType.Special && s.Item.Effect == 140 && s.Item.EffectValue == si.Id) is ItemInstance tsStone)
                                            {
                                                client.Character.Inventory.RemoveItemFromInventory(tsStone.Id);
                                            }
                                            ClientSession[] tsmembers = new ClientSession[40];
                                            client.Character.Timespace._mapInstanceDictionary.SelectMany(s => s.Value?.Sessions).ToList().CopyTo(tsmembers);
                                            foreach (ClientSession targetSession in tsmembers)
                                            {
                                                if (targetSession != null)
                                                {
                                                    targetSession.Character.IncrementQuests(QuestType.TimesSpace, si.QuestTimeSpaceId);
                                                }
                                            }
                                        }

                                        ClientSession[] tsmembers1 = new ClientSession[40];
                                        client.Character.Timespace._mapInstanceDictionary.SelectMany(s => s.Value?.Sessions).ToList().CopyTo(tsmembers1);
                                        bool isFailed = evt.MapInstance.InstanceBag.EndState == 2 || evt.MapInstance.InstanceBag.EndState == 3 || evt.MapInstance.InstanceBag.EndState == 1;
                                        foreach (ClientSession targetSession in tsmembers1)
                                        {
                                            if (targetSession != null)
                                            {
                                                ServerManager.Instance.TimespaceLogs.Add(new CharacterTimespaceLogDTO
                                                {
                                                    CharacterId = targetSession.Character.CharacterId,
                                                    ScriptedInstanceId = si.ScriptedInstanceId,
                                                    Score = client.Character.Timespace.IsLoserMode ? 0 : point,
                                                    Date = DateTime.Now,
                                                    IsFailed = isFailed
                                                });
                                                ServerManager.Instance.TimespaceSessions.Remove(targetSession);
                                            }
                                        }

                                        ScriptedInstance ClientTimeSpace = client.Character.Timespace;
                                        Observable.Timer(TimeSpan.FromSeconds(30)).SafeSubscribe(o =>
                                        {
                                            ClientSession[] tsmembers = new ClientSession[40];
                                            ClientTimeSpace._mapInstanceDictionary.SelectMany(s => s.Value?.Sessions).ToList().CopyTo(tsmembers);
                                            foreach (ClientSession targetSession in tsmembers)
                                            {
                                                if (targetSession != null)
                                                {
                                                    if (targetSession.Character.Hp <= 0)
                                                    {
                                                        targetSession.Character.Hp = 1;
                                                        targetSession.Character.Mp = 1;
                                                    }
                                                }
                                            }
                                            ClientTimeSpace._mapInstanceDictionary.Values.ToList().ForEach(m => m.Dispose());
                                        });
                                    }
                                    break;

                                case MapInstanceType.RaidInstance:
                                    {
                                        evt.MapInstance.InstanceBag.Clock.StopClock(false);
                                        evt.MapInstance.Clock.StopClock(false);
                                        evt.MapInstance.InstanceBag.EndState = (byte)evt.Parameter;

                                        Character owner = evt.MapInstance.Sessions.FirstOrDefault(s => s.Character.Group?.Raid?.InstanceBag.CreatorId == s.Character.CharacterId)?.Character ??
                                                          evt.MapInstance.Sessions.FirstOrDefault(s => s.Character.Group?.Raid != null)?.Character;

                                        Group group = owner?.Group;

                                        if (group?.Raid == null)
                                        {
                                            break;
                                        }

                                        Logger.Log.LogUserEvent("RAID_BEFITE", owner.Name, $"RaidId: {group.GroupId}");

                                        if (evt.MapInstance.InstanceBag.EndState == 1 && evt.MapInstance.Monsters.Any(s => s.IsBoss))
                                        {
                                            foreach (var grSession in group.Sessions.Where(s =>
                                                s?.Character?.MapInstance?.Monsters.Any(e => e.IsBoss) == true))
                                            {
                                                var rewardSender = new RewardSenderHelper();
                                                Observable.Timer(TimeSpan.FromMinutes(0)).SafeSubscribe(s =>
                                                {
                                                    rewardSender.SendRaidRewards(grSession, @group);
                                                });
                                            }

                                            Logger.Log.LogUserEvent("RAID_BEFMON", owner.Name, $"RaidId: {group.GroupId}");

                                            foreach (MapMonster mapMonster in evt.MapInstance.Monsters)
                                            {
                                                if (mapMonster != null)
                                                {
                                                    mapMonster.SetDeathStatement();
                                                    evt.MapInstance.Broadcast(StaticPacketHelper.Out(UserType.Monster, mapMonster.MapMonsterId));
                                                    evt.MapInstance.RemoveMonster(mapMonster);
                                                }
                                            }

                                            Logger.Log.LogUserEvent("RAID_SUCCESS", owner.Name, $"RaidId: {group.GroupId}");

                                            ServerManager.Instance.Broadcast(UserInterfaceHelper.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("RAID_SUCCEED"), group.Raid.Label, owner.Name), 0));

                                            var quest = ServerManager.Instance.BattlePassQuests.Find(s => s.MissionType == BattlePassMissionType.CompleteRaids && s.FirstData == group.Raid.Id);
                                            if (quest != null)
                                            {
                                                session.Character.IncreaseBattlePassQuestObjectives(quest.Id, 1);
                                            }

                                            var quest2 = ServerManager.Instance.BattlePassQuests.Find(s => s.MissionSubType == BattlePassMissionSubType.WinRaid);
                                            if (quest2 != null)
                                            {
                                                session.Character.IncreaseBattlePassQuestObjectives(quest.Id, 1);
                                            }

                                            group.Sessions.GetAllItems().ToList().ForEach(s =>
                                            {
                                                if (s.Account != null && s.Character?.Group?.Raid != null)
                                                {
                                                    s.Character.GeneralLogs?.Add(new GeneralLogDTO
                                                    {
                                                        AccountId = s.Account.AccountId,
                                                        CharacterId = s.Character.CharacterId,
                                                        IpAddress = s.CleanIpAddress,
                                                        LogData = $"{s.Character.Group.Raid.Id}",
                                                        LogType = "InstanceEntry",
                                                        Timestamp = DateTime.Now
                                                    });
                                                }
                                            });
                                        }
                                        TimeSpan dueTime = TimeSpan.FromSeconds(evt.MapInstance.InstanceBag.EndState == 1 ? 1 : 0);

                                        evt.MapInstance.Broadcast(StaticPacketHelper.GenerateRaidBf(evt.MapInstance.InstanceBag.EndState));

                                        Observable.Timer(dueTime).SafeSubscribe(o =>
                                        {
                                            evt.MapInstance.Sessions.Where(s => s.Character != null && s.Character.HasBuff(BCardType.CardType.FrozenDebuff, (byte)AdditionalTypes.FrozenDebuff.EternalIce))
                                                .Select(s => s.Character).ToList().ForEach(c =>
                                                {
                                                    c.RemoveBuff(569);
                                                });

                                            ClientSession[] groupMembers = new ClientSession[group.SessionCount];
                                            group.Sessions.CopyTo(groupMembers);

                                            foreach (ClientSession groupMember in groupMembers.Where(s => s.Character != null))
                                            {
                                                Observable.Timer(TimeSpan.FromMilliseconds(250)).SafeSubscribe(x =>
                                                {
                                                    if (groupMember.Character.Hp < 1)
                                                    {
                                                        groupMember.Character.Hp = 1;
                                                        groupMember.Character.Mp = 1;
                                                    }

                                                    groupMember.SendPacket(groupMember.Character.GenerateRaid(1, true));
                                                    groupMember.SendPacket(groupMember.Character.GenerateRaid(2, true));
                                                    group.LeaveGroup(groupMember);
                                                });
                                            }

                                            ServerManager.Instance.GroupList.RemoveAll(s => s.GroupId == group.GroupId);
                                            ServerManager.Instance.ThreadSafeGroupList.Remove(group.GroupId);

                                            group.Raid.Dispose();
                                        });
                                    }
                                    break;

                                case MapInstanceType.Act4Morcos:
                                case MapInstanceType.Act4Hatus:
                                case MapInstanceType.Act4Calvina:
                                case MapInstanceType.Act4Berios:
                                    client = evt.MapInstance.Sessions.FirstOrDefault(s => s.Character?.Family?.Act4RaidBossMap == evt.MapInstance);
                                    if (client != null)
                                    {
                                        Family fam = client.Character.Family;
                                        if (fam != null)
                                        {
                                            short destX = 38;
                                            short destY = 179;
                                            short rewardVNum = 882;
                                            switch (evt.MapInstance.MapInstanceType)
                                            {
                                                //Morcos is default
                                                case MapInstanceType.Act4Hatus:
                                                    destX = 18;
                                                    destY = 10;
                                                    rewardVNum = 185;
                                                    break;

                                                case MapInstanceType.Act4Calvina:
                                                    destX = 25;
                                                    destY = 7;
                                                    rewardVNum = 942;
                                                    break;

                                                case MapInstanceType.Act4Berios:
                                                    destX = 16;
                                                    destY = 25;
                                                    rewardVNum = 999;
                                                    break;
                                            }
                                            int count = evt.MapInstance.Sessions.Count(s => s?.Character != null);
                                            foreach (ClientSession sess in evt.MapInstance.Sessions)
                                            {
                                                if (evt.MapInstance.Sessions.Count(s => s.CleanIpAddress.Equals(sess?.CleanIpAddress)) > 2)
                                                {
                                                    sess?.SendPacket(sess?.Character?.GenerateSay(Language.Instance.GetMessageFromKey("MAX_PLAYER_ALLOWED"), 10));
                                                    return;
                                                }

                                                if (sess?.Character != null)
                                                {
                                                    var family = (FamilyDTO)sess.Character.Family;
                                                    var rare = ServerManager.RandomNumber<byte>(3, 8);
                                                    sess.Character.GiftAdd(rewardVNum, 1, rare: (byte)(rare + 1), design: 255);
                                                    sess.Character.GetReputation((sess.Character.Level + sess.Character.HeroLevel) * 1000, false);
                                                    if (family != null && family.FamilyGold + 30000 * ServerManager.Instance.Configuration.RateFamGold <= ServerManager.Instance.Configuration.MaxFamilyBankGold)
                                                    {
                                                        sess?.SendPacket(sess?.Character.GenerateSay($"You earned {30000 * ServerManager.Instance.Configuration.RateFamGold} Gold for your familybank.", 12));
                                                        family.FamilyGold += 30000 * ServerManager.Instance.Configuration.RateFamGold;
                                                        DAL.DAOFactory.FamilyDAO.InsertOrUpdate(ref family);
                                                        ServerManager.Instance.FamilyRefresh(family.FamilyId);
                                                    }


                                                    var increasePercentage = 0;
                                                    var rnd = ServerManager.RandomNumber();
                                                    increasePercentage += ServerManager.Instance.Configuration.BonusRaidBoxPercentage;

                                                    increasePercentage += sess.Character.GetBuff(BCardType.CardType.BonusBCards, ((byte)AdditionalTypes.BonusTitleBCards.IncreaseRaidBoxDropChance))[0];


                                                    if (increasePercentage != 0 && rnd <= increasePercentage)
                                                    {
                                                        rare = ServerManager.RandomNumber<byte>(3, 8);
                                                        sess.Character.GiftAdd(rewardVNum, 1, rare: (byte)(rare + 1), design: 255);
                                                        sess.Character.Session?.SendPacket(sess.Character.GenerateSay(Language.Instance.GetMessageFromKey("RECEIVE_RAIDBOX"), 12));
                                                        sess.CurrentMapInstance?.Broadcast(StaticPacketHelper.GenerateEff(UserType.Player, sess.Character.CharacterId, 5004), sess.Character.PositionX, sess.Character.PositionY);

                                                    }

                                                    if (sess.Character.GenerateFamilyXp((20000) / count))
                                                    {
                                                        sess.SendPacket(sess.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("WIN_FXP"), 20000 * ServerManager.Instance.Configuration.RateFxp / count), 10));
                                                    }

                                                    var quest = ServerManager.Instance.BattlePassQuests.Find(s => s.MissionSubType == BattlePassMissionSubType.WinAct4Raid);
                                                    if (quest != null)
                                                    {
                                                        sess.Character.IncreaseBattlePassQuestObjectives(quest.Id, 1);
                                                    }
                                                }
                                            }
                                            evt.MapInstance.Broadcast("dance 2");

                                            Logger.Log.Debug($"[fam.Name]FamilyRaidId: {evt.MapInstance.MapInstanceType}");

                                            CommunicationServiceClient.Instance.SendMessageToCharacter(new SCSCharacterMessage
                                            {
                                                DestinationCharacterId = fam.FamilyId,
                                                SourceCharacterId = client.Character.CharacterId,
                                                SourceWorldId = ServerManager.Instance.WorldId,
                                                Message = UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("FAMILYRAID_SUCCESS"), 0),
                                                Type = MessageType.Family
                                            });


                                            Observable.Timer(TimeSpan.FromSeconds(30)).SafeSubscribe(o =>
                                            {
                                                foreach (ClientSession targetSession in evt.MapInstance.Sessions.ToArray())
                                                {
                                                    if (targetSession != null)
                                                    {
                                                        if (targetSession.Character.Hp <= 0)
                                                        {
                                                            targetSession.Character.Hp = 1;
                                                            targetSession.Character.Mp = 1;
                                                        }

                                                        ServerManager.Instance.ChangeMapInstance(targetSession.Character.CharacterId, fam.Act4Raid.MapInstanceId, destX, destY);

                                                        targetSession.SendPacket("dance");
                                                    }
                                                }
                                                evt.MapInstance.Dispose();
                                            });

                                            fam.InsertFamilyLog(FamilyLogType.RaidWon, raidType: (int)evt.MapInstance.MapInstanceType - 7);
                                        }
                                    }
                                    break;

                                case MapInstanceType.CaligorInstance:

                                    FactionType winningFaction = CaligorRaid.AngelDamage > CaligorRaid.DemonDamage ? FactionType.Angel : FactionType.Demon;

                                    foreach (ClientSession sess in evt.MapInstance.Sessions)
                                    {
                                        if (sess?.Character != null)
                                        {
                                            if (CaligorRaid.RemainingTime > 2400)
                                            {
                                                if (sess.Character.Faction == winningFaction)
                                                {
                                                    sess.Character.GiftAdd(5960, 1);
                                                }
                                                else
                                                {
                                                    sess.Character.GiftAdd(5961, 1);
                                                }
                                            }
                                            else
                                            {
                                                if (sess.Character.Faction == winningFaction)
                                                {
                                                    sess.Character.GiftAdd(5961, 1);
                                                }
                                                else
                                                {
                                                    sess.Character.GiftAdd(5958, 1);
                                                }
                                            }
                                            sess.Character.GiftAdd(5959, 1);
                                            sess.Character.GenerateFamilyXp(2500);
                                            sess.Character.GetReputation((session.Character.Level + session.Character.HeroLevel) * 1000, false);
                                        }
                                    }
                                    evt.MapInstance.Broadcast(UserInterfaceHelper.GenerateCHDM(ServerManager.GetNpcMonster(2305).MaxHP, CaligorRaid.AngelDamage, CaligorRaid.DemonDamage, CaligorRaid.RemainingTime));
                                    break;
                            }
                            break;

                        case EventActionType.CLOCK:
                            evt.MapInstance.InstanceBag.Clock.TotalSecondsAmount = Convert.ToInt32(evt.Parameter);
                            evt.MapInstance.InstanceBag.Clock.SecondsRemaining = Convert.ToInt32(evt.Parameter);
                            break;

                        case EventActionType.MAPCLOCK:
                            evt.MapInstance.Clock.TotalSecondsAmount = Convert.ToInt32(evt.Parameter);
                            evt.MapInstance.Clock.SecondsRemaining = Convert.ToInt32(evt.Parameter);
                            break;

                        case EventActionType.STARTCLOCK:
                            Tuple<List<EventContainer>, List<EventContainer>> eve = (Tuple<List<EventContainer>, List<EventContainer>>)evt.Parameter;
                            evt.MapInstance.InstanceBag.Clock.StopEvents = eve.Item1;
                            evt.MapInstance.InstanceBag.Clock.TimeoutEvents = eve.Item2;
                            evt.MapInstance.InstanceBag.Clock.StartClock();
                            evt.MapInstance.Broadcast(evt.MapInstance.InstanceBag.Clock.GetClock());
                            break;

                        case EventActionType.STARTMAPCLOCK:
                            eve = (Tuple<List<EventContainer>, List<EventContainer>>)evt.Parameter;
                            evt.MapInstance.Clock.StopEvents = eve.Item1;
                            evt.MapInstance.Clock.TimeoutEvents = eve.Item2;
                            evt.MapInstance.Clock.StartClock();
                            evt.MapInstance.Broadcast(evt.MapInstance.Clock.GetClock());
                            break;

                        case EventActionType.STOPCLOCK:
                            evt.MapInstance.InstanceBag.Clock.StopClock();
                            evt.MapInstance.Broadcast(evt.MapInstance.InstanceBag.Clock.GetClock());
                            break;

                        case EventActionType.STOPMAPCLOCK:
                            evt.MapInstance.Clock.StopClock();
                            evt.MapInstance.Broadcast(evt.MapInstance.Clock.GetClock());
                            break;

                        case EventActionType.ADDCLOCKTIME:
                            evt.MapInstance.InstanceBag.Clock.AddTime((int)evt.Parameter);
                            evt.MapInstance.Broadcast(evt.MapInstance.InstanceBag.Clock.GetClock());
                            break;

                        case EventActionType.ADDMAPCLOCKTIME:
                            evt.MapInstance.Clock.AddTime((int)evt.Parameter);
                            evt.MapInstance.Broadcast(evt.MapInstance.Clock.GetClock());
                            break;

                        case EventActionType.TELEPORT:
                            Tuple<short, short, short, short> tp = (Tuple<short, short, short, short>)evt.Parameter;
                            List<Character> characters = evt.MapInstance.GetCharactersInRange(tp.Item1, tp.Item2, 5).ToList();
                            characters.ForEach(s =>
                            {
                                s.PositionX = tp.Item3;
                                s.PositionY = tp.Item4;
                                evt.MapInstance?.Broadcast(s.Session, s.GenerateTp());
                                foreach (Mate mate in s.Mates.Where(m => m.IsTeamMember && m.IsAlive))
                                {
                                    mate.PositionX = tp.Item3;
                                    mate.PositionY = tp.Item4;
                                    evt.MapInstance?.Broadcast(s.Session, mate.GenerateTp());
                                }
                            });
                            break;

                        case EventActionType.SPAWNPORTAL:
                            evt.MapInstance.CreatePortal((Portal)evt.Parameter);
                            break;

                        case EventActionType.REFRESHMAPITEMS:
                            evt.MapInstance.MapClear();
                            break;

                        case EventActionType.STOPMAPWAVES:
                            evt.MapInstance.WaveEvents.Clear();
                            break;

                        case EventActionType.NPCSEFFECTCHANGESTATE:
                            evt.MapInstance.Npcs.ForEach(s => s.EffectActivated = (bool)evt.Parameter);
                            break;

                        case EventActionType.CHANGEPORTALTYPE:
                            Tuple<int, PortalType> param = (Tuple<int, PortalType>)evt.Parameter;
                            Portal portal = evt.MapInstance.Portals.Find(s => s.PortalId == param.Item1);
                            if (portal != null)
                            {
                                portal.IsDisabled = true;
                                evt.MapInstance.Broadcast(portal.GenerateGp());
                                portal.IsDisabled = false;

                                portal.Type = (short)param.Item2;
                                if ((PortalType)portal.Type == PortalType.Closed
                                && (evt.MapInstance.MapInstanceType.Equals(MapInstanceType.Act4Berios)
                                 || evt.MapInstance.MapInstanceType.Equals(MapInstanceType.Act4Calvina)
                                 || evt.MapInstance.MapInstanceType.Equals(MapInstanceType.Act4Hatus)
                                 || evt.MapInstance.MapInstanceType.Equals(MapInstanceType.Act4Morcos)))
                                {
                                    portal.IsDisabled = true;
                                }
                                evt.MapInstance.Broadcast(portal.GenerateGp());
                            }
                            break;

                        case EventActionType.CHANGEDROPRATE:
                            evt.MapInstance.DropRate = (int)evt.Parameter;
                            break;

                        case EventActionType.CHANGEXPRATE:
                            evt.MapInstance.XpRate = (int)evt.Parameter;
                            break;

                        case EventActionType.CLEARMAPMONSTERS:
                            var list = evt.MapInstance.Monsters.ToList()
                                .Where(s => s.Owner?.Character == null && s.Owner?.Mate == null);
                            foreach (var mapMonster in list)
                            {
                                evt.MapInstance.Broadcast(StaticPacketHelper.Out(UserType.Monster, mapMonster.MapMonsterId));
                                mapMonster.SetDeathStatement();
                                evt.MapInstance.RemoveMonster(mapMonster);
                            }
                            break;

                        case EventActionType.DISPOSEMAP:
                            evt.MapInstance.Dispose();
                            break;

                        case EventActionType.SPAWNBUTTON:
                            evt.MapInstance.SpawnButton((MapButton)evt.Parameter);
                            break;

                        case EventActionType.UNSPAWNMONSTERS:
                            evt.MapInstance.DespawnMonster((int)evt.Parameter);
                            break;

                        case EventActionType.SPAWNMONSTER:
                            evt.MapInstance.SummonMonster((MonsterToSummon)evt.Parameter);
                            break;

                        case EventActionType.SPAWNMONSTERS:
                            bool generate = evt.MapInstance.Map.MapId != 2004;
                            evt.MapInstance.SummonMonsters((List<MonsterToSummon>)evt.Parameter, generate);
                            break;

                        case EventActionType.REFRESHRAIDGOAL:
                            ClientSession cl = evt.MapInstance.Sessions.FirstOrDefault();
                            if (cl?.Character != null)
                            {
                                ServerManager.Instance.Broadcast(cl, cl.Character?.Group?.GeneraterRaidmbf(cl), ReceiverType.Group);
                                ServerManager.Instance.Broadcast(cl, UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("NEW_MISSION"), 0), ReceiverType.Group);
                            }
                            break;

                        case EventActionType.SPAWNNPC:
                            evt.MapInstance.SummonNpc((NpcToSummon)evt.Parameter);
                            break;

                        case EventActionType.SPAWNNPCS:
                            evt.MapInstance.SummonNpcs((List<NpcToSummon>)evt.Parameter);
                            break;

                        case EventActionType.DROPITEMS:
                            evt.MapInstance.DropItems((List<Tuple<short, int, short, short>>)evt.Parameter);
                            break;

                        case EventActionType.THROWITEMS:
                            Tuple<int, short, byte, int, int, byte, byte, short> parameters = (Tuple<int, short, byte, int, int, byte, byte, short>)evt.Parameter;
                            if (monster != null)
                            {
                                parameters = new Tuple<int, short, byte, int, int, byte, byte, short>(monster.MapMonsterId, parameters.Item2, parameters.Item3, parameters.Item4, parameters.Item5, parameters.Item6, parameters.Item7, parameters.Rest);
                            }
                            evt.MapInstance.ThrowItems(parameters);
                            break;

                        case EventActionType.SPAWNONLASTENTRY:
                            //Character lastincharacter = evt.MapInstance.Sessions.OrderByDescending(s => s.RegisterTime).FirstOrDefault()?.Character;
                            //List<MonsterToSummon> summonParameters = new List<MonsterToSummon>();
                            //MapCell hornSpawn = new MapCell
                            //{
                            //    X = lastincharacter?.PositionX ?? 154,
                            //    Y = lastincharacter?.PositionY ?? 140
                            //};
                            //BattleEntity hornTarget = lastincharacter?.BattleEntity ?? null;
                            //summonParameters.Add(new MonsterToSummon(Convert.ToInt16(evt.Parameter), hornSpawn, hornTarget, true));
                            //evt.MapInstance.SummonMonsters(summonParameters);
                            break;

                        case EventActionType.REMOVEAFTER:
                            {
                                Observable.Timer(TimeSpan.FromSeconds(Convert.ToInt16(evt.Parameter)))
                                    .SafeSubscribe(o =>
                                    {
                                        if (monster != null)
                                        {
                                            monster.SetDeathStatement();
                                            evt.MapInstance.RemoveMonster(monster);
                                            evt.MapInstance.Broadcast(StaticPacketHelper.Out(UserType.Monster, monster.MapMonsterId));
                                        }
                                    });
                            }
                            break;

                        case EventActionType.REMOVELAURENABUFF:
                            {
                                Observable.Timer(TimeSpan.FromSeconds(1))
                                    .SafeSubscribe(observer =>
                                    {
                                        if (evt.Parameter is BattleEntity battleEntity
                                            && evt.MapInstance?.Monsters != null
                                            && !evt.MapInstance.Monsters.ToList().Any(s => s.MonsterVNum == 2327))
                                        {
                                            battleEntity.RemoveBuff(475);
                                        }
                                    });
                            }
                            break;

                        case EventActionType.REMOVEKIROLLASBUFF:
                            {
                                Observable.Timer(TimeSpan.FromSeconds(1))
                                    .SafeSubscribe(observer =>
                                    {
                                        if (evt.Parameter is BattleEntity battleEntity
                                            && evt.MapInstance?.Monsters != null
                                            && !evt.MapInstance.Monsters.ToList().Any(s => s.MonsterVNum == 3040))
                                        {
                                            battleEntity.RemoveBuff(767);
                                        }
                                    });
                            }
                            break;

                        case EventActionType.SENDINSTANTBATTLEWAVEREWARDS:

                            if (!(evt.Parameter is InstantBattleWaveReward itm))
                            {
                                break;
                            }

                            var sessions = evt.MapInstance?.Sessions;

                            if (sessions == null)
                            {
                                break;
                            }

                            var rewardHelper = new RewardSenderHelper();

                            if (!itm.Items.Any())
                            {
                                return;
                            }

                            if (!_keepAliveClient.IsBazaarOnline())
                            {
                                return;
                            }

                            Observable.Timer(TimeSpan.FromSeconds(0)).SafeSubscribe(async x =>
                            {
                                foreach (var mapsession in sessions)
                                {
                                    await rewardHelper.SendInstantBattleWaveRewards(mapsession, itm);
                                    await Task.Delay(250);
                                }
                            });

                            break;

                        case EventActionType.ENABLEPVP:

                            if (evt.MapInstance == null)
                            {
                                break;
                            }

                            evt.MapInstance.IsPVP = true;
                            evt.MapInstance.Broadcast(UserInterfaceHelper.GenerateMsg("PvP is now Enabled.", 0));

                            break;

                            #endregion
                    }
                }
            }
        }

        public void ScheduleEvent(TimeSpan timeSpan, EventContainer evt) => Observable.Timer(timeSpan).SafeSubscribe(x => RunEvent(evt));

        #endregion
    }
}