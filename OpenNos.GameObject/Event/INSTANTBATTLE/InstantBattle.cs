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
using OpenNos.Domain;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;
using OpenNos.Master.Library.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using log4net.Layout;
using NosByte.Shared;
using NosByte.Shared.ApiModels;
using OpenNos.Core.Extensions;
using OpenNos.GameObject.HttpClients;

namespace OpenNos.GameObject.Event
{
    public static class InstantBattle
    {
        private static readonly List<Tuple<MapInstance, byte>> Maps = new List<Tuple<MapInstance, byte>>();

        private static readonly KeepAliveClient KeepAliveClient = KeepAliveClient.Instance;

        #region Methods

        private static void CreateInstantBattleMaps(List<ClientSession> sessions, byte instanceLevel)
        {
            if (sessions == null || sessions.Count == 0)
            {
                return;
            }


            sessions = sessions.Shuffle();
            var currentPlaceInList = 0;
            MapInstance map = ServerManager.GenerateMapInstance(2004, MapInstanceType.NormalInstance, new InstanceBag());
            Maps.Add(new Tuple<MapInstance, byte>(map, instanceLevel));

            foreach (var session in sessions)
            {
                if (session?.Character == null)
                {
                    continue;
                }

                if (currentPlaceInList % 50 == 0)
                {
                    map = ServerManager.GenerateMapInstance(2004, MapInstanceType.NormalInstance, new InstanceBag());
                    Maps.Add(new Tuple<MapInstance, byte>(map, instanceLevel));
                }

                UserHttpClient.Instance.SetCharacterEventState(new UserDataModel
                {
                    CharacterId = session.Character.CharacterId,
                    EventType = EventType.INSTANTBATTLE,
                    LevelSum = (short)(session.Character.Level + session.Character.HeroLevel),
                    Name = session.Character.Name
                });
                ServerManager.Instance.TeleportOnRandomPlaceInMap(session, map.MapInstanceId);
                currentPlaceInList++;
            }
        }

        public static void GenerateInstantBattle()
        {
            ServerManager.Instance.Broadcast(UserInterfaceHelper.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("INSTANTBATTLE_MINUTES"), 5), 0));
            ServerManager.Instance.Broadcast(UserInterfaceHelper.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("INSTANTBATTLE_MINUTES"), 5), 1));

            Observable.Timer(TimeSpan.FromMinutes(4)).SafeSubscribe(x =>
            {
                ServerManager.Instance.Broadcast(UserInterfaceHelper.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("INSTANTBATTLE_MINUTES"), 1), 0));
                ServerManager.Instance.Broadcast(UserInterfaceHelper.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("INSTANTBATTLE_MINUTES"), 1), 1));
            });

            Observable.Timer(TimeSpan.FromSeconds(270)).SafeSubscribe(x =>
            {
                ServerManager.Instance.Broadcast(UserInterfaceHelper.GenerateMsg(
                    string.Format(Language.Instance.GetMessageFromKey("INSTANTBATTLE_SECONDS"), 30), 0));
                ServerManager.Instance.Broadcast(UserInterfaceHelper.GenerateMsg(
                    string.Format(Language.Instance.GetMessageFromKey("INSTANTBATTLE_SECONDS"), 30), 1));
            });


            Observable.Timer(TimeSpan.FromSeconds(290)).SafeSubscribe(x =>
            {
                ServerManager.Instance.Broadcast(UserInterfaceHelper.GenerateMsg(
                    string.Format(Language.Instance.GetMessageFromKey("INSTANTBATTLE_SECONDS"), 10), 0));
                ServerManager.Instance.Broadcast(UserInterfaceHelper.GenerateMsg(
                    string.Format(Language.Instance.GetMessageFromKey("INSTANTBATTLE_SECONDS"), 10), 1));
            });


            Observable.Timer(TimeSpan.FromSeconds(300)).SafeSubscribe(x =>
            {
                ServerManager.Instance.Broadcast(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("INSTANTBATTLE_STARTED"), 1));
                ServerManager.Instance.Sessions.Where(s => s.Character?.MapInstance.MapInstanceType == MapInstanceType.BaseMapInstance).ToList().ForEach(s => s.SendPacket($"qnaml 1 #guri^506 {Language.Instance.GetMessageFromKey("INSTANTBATTLE_QUESTION")}"));
                ServerManager.Instance.EventInWaiting = true;
            });

            Observable.Timer(TimeSpan.FromSeconds(330)).SafeSubscribe(x =>
            {
                ServerManager.Instance.Broadcast(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("INSTANTBATTLE_STARTED"), 1));
                ServerManager.Instance.Sessions.Where(s => s.Character?.IsWaitingForEvent == false).ToList().ForEach(s => s.SendPacket("esf"));
                ServerManager.Instance.EventInWaiting = false;
                List<ClientSession> sessions = ServerManager.Instance.Sessions.Where(s => s.Character?.IsWaitingForEvent == true && s.Character.MapInstance.MapInstanceType == MapInstanceType.BaseMapInstance).ToList();

                Dictionary<byte, byte> levelDictionary = new Dictionary<byte, byte>
            {
                {1, 39},
                {40, 49},
                {50, 59},
                {60, 69},
                {70, 79},
                {80, 99}
            };

                var noChampionSessions = sessions.Where(s => s.Character.HeroLevel <= 0).ToList();

                foreach (var kvp in levelDictionary)
                {
                    var toAddSessions = noChampionSessions.Where(s => s.Character.Level >= kvp.Key && s.Character.Level <= kvp.Value && !s.Character.IsMuted()).ToList();
                    CreateInstantBattleMaps(toAddSessions, kvp.Key);
                }

                CreateInstantBattleMaps(sessions?.Where(s => s?.Character?.HeroLevel > 0 && s?.Character?.HeroLevel < 30 && !s.Character.IsMuted()).ToList(), 100);
                CreateInstantBattleMaps(sessions?.Where(s => s?.Character?.HeroLevel > 29 && !s.Character.IsMuted()).ToList(), 130);

                ServerManager.Instance.Sessions.Where(s => s.Character != null).ToList().ForEach(s => s.Character.IsWaitingForEvent = false);
                ServerManager.Instance.StartedEvents.Remove(EventType.INSTANTBATTLE);
                LargeHeapCompactor.CompactHeap();
                foreach (var mapInstance in from mapInstance in Maps let task = new InstantBattleTask() select mapInstance)
                {
                    Observable.Timer(TimeSpan.FromMinutes(0)).SafeSubscribe(x => InstantBattleTask.Run(mapInstance));
                }
            });
        }

        #endregion

        #region Classes

        public class InstantBattleTask
        {
            #region Methods

            public static void Run(Tuple<MapInstance, byte> mapInstance)
            {
                long maxGold = ServerManager.Instance.Configuration.MaxGold;
                Observable.Timer(TimeSpan.FromSeconds(10)).SafeSubscribe(x =>
                {
                    if (!mapInstance.Item1.Sessions.Any())
                    {
                        mapInstance.Item1.Sessions.Where(s => s.Character != null).ToList().ForEach(s =>
                        {
                            s.Character.RemoveBuffByBCardTypeSubType(new List<KeyValuePair<byte, byte>>()
                        {
                            new KeyValuePair<byte, byte>((byte)BCardType.CardType.SpecialActions, (byte)AdditionalTypes.SpecialActions.Hide),
                            new KeyValuePair<byte, byte>((byte)BCardType.CardType.FalconSkill, (byte)AdditionalTypes.FalconSkill.Hide),
                            new KeyValuePair<byte, byte>((byte)BCardType.CardType.FalconSkill, (byte)AdditionalTypes.FalconSkill.Ambush)
                        });
                            ServerManager.Instance.ChangeMap(s.Character.CharacterId, s.Character.MapId, s.Character.MapX, s.Character.MapY);
                        });
                    }

                    IDisposable interval = null;
                    Observable.Timer(TimeSpan.FromMinutes(12)).SafeSubscribe(x =>
                    {
                        interval = Observable.Interval(TimeSpan.FromSeconds(1)).SafeSubscribe(x =>
                        {
                            if (mapInstance.Item1.Monsters.Any(s => s.CurrentHp > 0))
                            {
                                return;
                            }
                            EventHelper.Instance.ScheduleEvent(TimeSpan.FromMinutes(0), new EventContainer(mapInstance.Item1, EventActionType.SPAWNPORTAL, new Portal { SourceX = 47, SourceY = 33, DestinationMapId = 1 }));
                            mapInstance.Item1.Broadcast(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("INSTANTBATTLE_SUCCEEDED"), 0));

                            if (!KeepAliveClient.IsBazaarOnline()) // Tbh here I'm just checking if the bazaar server is online so I can send rewards to only 1 char
                            {
                                return;
                            }

                            foreach (ClientSession cli in mapInstance.Item1.Sessions.Where(s => s.Character != null).ToList())
                            {
                                var rewardSender = new RewardSenderHelper();
                                rewardSender.SendInstantBattleEndRewards(cli, mapInstance);

                                var quest = ServerManager.Instance.BattlePassQuests.Find(s => s.MissionSubType == BattlePassMissionSubType.WinInstantCombat);
                                if (quest != null)
                                {
                                    cli.Character.IncreaseBattlePassQuestObjectives(quest.Id, 1);
                                }
                            }

                            interval?.Dispose();
                        });
                    });

                    Observable.Timer(TimeSpan.FromMinutes(15)).SafeSubscribe(x =>
                    {
                        mapInstance?.Item1?.StopLife();
                        EventHelper.Instance.ScheduleEvent(TimeSpan.FromMinutes(0), new EventContainer(mapInstance.Item1, EventActionType.DISPOSEMAP, null));
                    });

                    Observable.Timer(TimeSpan.FromMinutes(20)).SafeSubscribe(x =>
                    {
                        UserHttpClient.Instance.DeleteInstantBattleEvents();
                    });
                    EventHelper.Instance.ScheduleEvent(TimeSpan.FromMinutes(3), new EventContainer(mapInstance.Item1, EventActionType.SENDPACKET, UserInterfaceHelper.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("INSTANTBATTLE_MINUTES_REMAINING"), 12), 0)));
                    EventHelper.Instance.ScheduleEvent(TimeSpan.FromMinutes(5), new EventContainer(mapInstance.Item1, EventActionType.SENDPACKET, UserInterfaceHelper.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("INSTANTBATTLE_MINUTES_REMAINING"), 10), 0)));
                    EventHelper.Instance.ScheduleEvent(TimeSpan.FromMinutes(10), new EventContainer(mapInstance.Item1, EventActionType.SENDPACKET, UserInterfaceHelper.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("INSTANTBATTLE_MINUTES_REMAINING"), 5), 0)));
                    EventHelper.Instance.ScheduleEvent(TimeSpan.FromMinutes(11), new EventContainer(mapInstance.Item1, EventActionType.SENDPACKET, UserInterfaceHelper.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("INSTANTBATTLE_MINUTES_REMAINING"), 4), 0)));
                    EventHelper.Instance.ScheduleEvent(TimeSpan.FromMinutes(12), new EventContainer(mapInstance.Item1, EventActionType.SENDPACKET, UserInterfaceHelper.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("INSTANTBATTLE_MINUTES_REMAINING"), 3), 0)));
                    EventHelper.Instance.ScheduleEvent(TimeSpan.FromMinutes(13), new EventContainer(mapInstance.Item1, EventActionType.SENDPACKET, UserInterfaceHelper.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("INSTANTBATTLE_MINUTES_REMAINING"), 2), 0)));
                    EventHelper.Instance.ScheduleEvent(TimeSpan.FromMinutes(14), new EventContainer(mapInstance.Item1, EventActionType.SENDPACKET, UserInterfaceHelper.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("INSTANTBATTLE_MINUTES_REMAINING"), 1), 0)));
                    EventHelper.Instance.ScheduleEvent(TimeSpan.FromMinutes(14.5), new EventContainer(mapInstance.Item1, EventActionType.SENDPACKET, UserInterfaceHelper.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("INSTANTBATTLE_SECONDS_REMAINING"), 30), 0)));
                    EventHelper.Instance.ScheduleEvent(TimeSpan.FromMinutes(14.5), new EventContainer(mapInstance.Item1, EventActionType.SENDPACKET, UserInterfaceHelper.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("INSTANTBATTLE_SECONDS_REMAINING"), 30), 0)));
                    EventHelper.Instance.ScheduleEvent(TimeSpan.FromMinutes(0), new EventContainer(mapInstance.Item1, EventActionType.SENDPACKET, UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("INSTANTBATTLE_MONSTERS_INCOMING"), 0)));
                    EventHelper.Instance.ScheduleEvent(TimeSpan.FromSeconds(10), new EventContainer(mapInstance.Item1, EventActionType.SENDPACKET, UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("INSTANTBATTLE_MONSTERS_HERE"), 0)));

                    for (int wave = 0; wave < 4; wave++)
                    {
                        EventHelper.Instance.ScheduleEvent(TimeSpan.FromSeconds(130 + (wave * 160)), new EventContainer(mapInstance.Item1, EventActionType.SENDPACKET, UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("INSTANTBATTLE_MONSTERS_WAVE"), 0)));
                        EventHelper.Instance.ScheduleEvent(TimeSpan.FromSeconds(160 + (wave * 160)), new EventContainer(mapInstance.Item1, EventActionType.SENDPACKET, UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("INSTANTBATTLE_MONSTERS_INCOMING"), 0)));
                        EventHelper.Instance.ScheduleEvent(TimeSpan.FromSeconds(170 + (wave * 160)), new EventContainer(mapInstance.Item1, EventActionType.SENDPACKET, UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("INSTANTBATTLE_MONSTERS_HERE"), 0)));
                        var currentWave = wave;

                        Observable.Timer(TimeSpan.FromSeconds(10 + (wave * 160))).SafeSubscribe(async x =>
                        {
                            var currentMap = mapInstance.Item1;
                            foreach (var monsterGroup in getInstantBattleMonster(mapInstance.Item1.Map, mapInstance.Item2, currentWave).GroupBy(s => s.VNum))
                            {
                                currentMap.SummonMonsters(monsterGroup.ToList());
                                await Task.Delay(1000);
                            }
                        });

                        EventHelper.Instance.ScheduleEvent(TimeSpan.FromSeconds(140 + (wave * 160)), new EventContainer(mapInstance.Item1, EventActionType.SENDINSTANTBATTLEWAVEREWARDS, getInstantBattleDrop(mapInstance.Item1.Map, mapInstance.Item2, wave)));
                    }

                    Observable.Timer(TimeSpan.FromSeconds(650)).SafeSubscribe(async x =>
                    {
                        var currentMap = mapInstance.Item1;
                        foreach (var monsterGroup in getInstantBattleMonster(mapInstance.Item1.Map, mapInstance.Item2, 4).GroupBy(s => s.VNum))
                        {
                            currentMap.SummonMonsters(monsterGroup.ToList());
                            await Task.Delay(1000);
                        }
                    });
                });
            }

            private static IEnumerable<Tuple<short, int, short, short>> generateDrop(Map map, short vnum, int amountofdrop, int amount)
            {
                List<Tuple<short, int, short, short>> dropParameters = new List<Tuple<short, int, short, short>>();
                for (int i = 0; i < amountofdrop; i++)
                {
                    MapCell cell = map.GetRandomPosition();
                    dropParameters.Add(new Tuple<short, int, short, short>(vnum, amount, cell.X, cell.Y));
                }
                return dropParameters;
            }

            private static InstantBattleWaveReward getInstantBattleDrop(Map map, short instantbattletype, int wave)
            {
                var reward = new InstantBattleWaveReward
                {
                    RewardLevel = (byte)(wave + 1)
                };

                switch (instantbattletype)
                {
                    case 1:
                        reward.Gold = (wave + 1) * 5000;
                        switch (wave)
                        {
                            case 0:
                                reward.Items.Add(new InstantBattleItem { VNum = 2027, Amount = 5 });
                                reward.Items.Add(new InstantBattleItem { VNum = 2018, Amount = 5 });
                                reward.Items.Add(new InstantBattleItem { VNum = 180, Amount = 5 });
                                break;

                            case 1:
                                reward.Items.Add(new InstantBattleItem { VNum = 1002, Amount = 3 });
                                reward.Items.Add(new InstantBattleItem { VNum = 1005, Amount = 3 });
                                reward.Items.Add(new InstantBattleItem { VNum = 181, Amount = 1 });
                                break;

                            case 2:
                                reward.Items.Add(new InstantBattleItem { VNum = 1002, Amount = 5 });
                                reward.Items.Add(new InstantBattleItem { VNum = 1005, Amount = 5 });
                                break;

                            case 3:
                                reward.Items.Add(new InstantBattleItem { VNum = 1003, Amount = 5 });
                                reward.Items.Add(new InstantBattleItem { VNum = 1006, Amount = 5 });
                                break;
                        }
                        break;

                    case 40:
                        reward.Gold = (wave + 1) * 6000;
                        switch (wave)
                        {
                            case 0:
                                reward.Items.Add(new InstantBattleItem { VNum = 1008, Amount = 3 });
                                reward.Items.Add(new InstantBattleItem { VNum = 180, Amount = 1 });
                                break;

                            case 1:
                                reward.Items.Add(new InstantBattleItem { VNum = 1008, Amount = 3 });
                                reward.Items.Add(new InstantBattleItem { VNum = 181, Amount = 1 });
                                break;

                            case 2:
                                reward.Items.Add(new InstantBattleItem { VNum = 1009, Amount = 3 });
                                reward.Items.Add(new InstantBattleItem { VNum = 1246, Amount = 1 });
                                reward.Items.Add(new InstantBattleItem { VNum = 1247, Amount = 1 });
                                break;

                            case 3:
                                reward.Items.Add(new InstantBattleItem { VNum = 1009, Amount = 3 });
                                reward.Items.Add(new InstantBattleItem { VNum = 1248, Amount = 1 });
                                break;
                        }

                        break;

                    case 50:
                        reward.Gold = (wave + 1) * 7000;
                        switch (wave)
                        {
                            case 0:
                                reward.Items.Add(new InstantBattleItem { VNum = 1008, Amount = 3 });
                                reward.Items.Add(new InstantBattleItem { VNum = 180, Amount = 1 });
                                break;

                            case 1:
                                reward.Items.Add(new InstantBattleItem { VNum = 1008, Amount = 3 });
                                reward.Items.Add(new InstantBattleItem { VNum = 181, Amount = 1 });
                                break;

                            case 2:
                                reward.Items.Add(new InstantBattleItem { VNum = 1009, Amount = 3 });
                                reward.Items.Add(new InstantBattleItem { VNum = 1246, Amount = 1 });
                                reward.Items.Add(new InstantBattleItem { VNum = 1247, Amount = 1 });
                                break;

                            case 3:
                                reward.Items.Add(new InstantBattleItem { VNum = 1009, Amount = 3 });
                                reward.Items.Add(new InstantBattleItem { VNum = 1248, Amount = 1 });
                                break;
                        }
                        break;

                    case 60:
                        reward.Gold = (wave + 1) * 8000;
                        switch (wave)
                        {
                            case 0:
                                reward.Items.Add(new InstantBattleItem { VNum = 1010, Amount = 4 });
                                reward.Items.Add(new InstantBattleItem { VNum = 1246, Amount = 1 });
                                break;

                            case 1:
                                reward.Items.Add(new InstantBattleItem { VNum = 1010, Amount = 3 });
                                reward.Items.Add(new InstantBattleItem { VNum = 1247, Amount = 1 });
                                break;

                            case 2:
                                reward.Items.Add(new InstantBattleItem { VNum = 1010, Amount = 13 });
                                reward.Items.Add(new InstantBattleItem { VNum = 1246, Amount = 1 });
                                reward.Items.Add(new InstantBattleItem { VNum = 1247, Amount = 1 });
                                break;

                            case 3:
                                reward.Items.Add(new InstantBattleItem { VNum = 1011, Amount = 5 });
                                reward.Items.Add(new InstantBattleItem { VNum = 1029, Amount = 1 });
                                reward.Items.Add(new InstantBattleItem { VNum = 1248, Amount = 1 });
                                break;
                        }
                        break;

                    case 70:
                        reward.Gold = (wave + 1) * 9000;
                        switch (wave)
                        {
                            case 0:
                                reward.Items.Add(new InstantBattleItem { VNum = 1010, Amount = 3 });
                                reward.Items.Add(new InstantBattleItem { VNum = 1246, Amount = 1 });
                                break;

                            case 1:
                                reward.Items.Add(new InstantBattleItem { VNum = 1010, Amount = 4 });
                                reward.Items.Add(new InstantBattleItem { VNum = 1247, Amount = 1 });
                                break;

                            case 2:
                                reward.Items.Add(new InstantBattleItem { VNum = 1010, Amount = 5 });
                                reward.Items.Add(new InstantBattleItem { VNum = 1246, Amount = 1 });
                                reward.Items.Add(new InstantBattleItem { VNum = 1247, Amount = 1 });
                                break;

                            case 3:
                                reward.Items.Add(new InstantBattleItem { VNum = 1011, Amount = 5 });
                                reward.Items.Add(new InstantBattleItem { VNum = 1248, Amount = 1 });
                                reward.Items.Add(new InstantBattleItem { VNum = 1029, Amount = 1 });
                                break;
                        }
                        break;

                    case 80:
                        switch (wave)
                        {
                            case 0:
                                reward.Gold = 20000;
                                reward.Items.Add(new InstantBattleItem { VNum = 1247, Amount = 5 });
                                reward.Items.Add(new InstantBattleItem { VNum = 1243, Amount = 5 });
                                break;

                            case 1:
                                reward.Gold = 50000;
                                reward.Items.Add(new InstantBattleItem { VNum = 1246, Amount = 2 });
                                reward.Items.Add(new InstantBattleItem { VNum = 1242, Amount = 5 });
                                break;

                            case 2:
                                reward.Gold = 75000;
                                reward.Items.Add(new InstantBattleItem { VNum = 1030, Amount = 15 });
                                reward.Items.Add(new InstantBattleItem { VNum = 1013, Amount = 5 });
                                reward.Items.Add(new InstantBattleItem { VNum = 1247, Amount = 10 });
                                break;

                            case 3:
                                reward.Gold = 150000;
                                reward.Items.Add(new InstantBattleItem { VNum = 2282, Amount = 30 });
                                reward.Items.Add(new InstantBattleItem { VNum = 1249, Amount = 1 });
                                reward.Items.Add(new InstantBattleItem { VNum = 1246, Amount = 10 });
                                break;
                        }
                        break;
                    case 100:
                        switch (wave)
                        {
                            case 0:
                                reward.Gold = 50000;
                                reward.Items.Add(new InstantBattleItem { VNum = 1363, Amount = 3 });
                                reward.Items.Add(new InstantBattleItem { VNum = 2284, Amount = 10 });
                                break;
                            case 1:
                                reward.Gold = 100000;
                                reward.Items.Add(new InstantBattleItem { VNum = 1364, Amount = 2 });
                                reward.Items.Add(new InstantBattleItem { VNum = 2285, Amount = 10 });
                                break;
                            case 2:
                                reward.Gold = 200000;
                                reward.Items.Add(new InstantBattleItem { VNum = 1030, Amount = 15 });
                                reward.Items.Add(new InstantBattleItem { VNum = 1013, Amount = 20 });
                                reward.Items.Add(new InstantBattleItem { VNum = 1900, Amount = 1 });
                                break;
                            case 3:
                                reward.Gold = 500000;
                                reward.Items.Add(new InstantBattleItem { VNum = 2282, Amount = 50 });
                                reward.Items.Add(new InstantBattleItem { VNum = 1249, Amount = 1 });
                                reward.Items.Add(new InstantBattleItem { VNum = 1898, Amount = 1 });
                                break;
                        }
                        break;
                }
                return reward;
            }

            private static List<MonsterToSummon> getInstantBattleMonster(Map map, short instantbattletype, int wave)
            {
                List<MonsterToSummon> SummonParameters = new List<MonsterToSummon>();

                switch (instantbattletype)
                {
                    case 1:
                        switch (wave)
                        {
                            case 0:
                                SummonParameters.AddRange(map.GenerateMonsters(1, 8, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(58, 7, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(105, 8, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(107, 7, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(108, 4, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(111, 7, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(136, 7, true, new List<EventContainer>()));
                                break;

                            case 1:
                                SummonParameters.AddRange(map.GenerateMonsters(194, 7, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(114, 7, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(99, 7, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(39, 8, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(2, 8, true, new List<EventContainer>()));
                                break;

                            case 2:
                                SummonParameters.AddRange(map.GenerateMonsters(140, 7, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(100, 7, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(81, 7, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(12, 7, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(4, 8, true, new List<EventContainer>()));
                                break;

                            case 3:
                                SummonParameters.AddRange(map.GenerateMonsters(115, 7, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(112, 7, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(110, 7, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(14, 7, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(5, 8, true, new List<EventContainer>()));
                                break;

                            case 4:
                                SummonParameters.AddRange(map.GenerateMonsters(979, 1, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(167, 7, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(137, 5, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(22, 7, false, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(17, 4, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(16, 8, true, new List<EventContainer>()));
                                break;
                        }
                        break;

                    case 40:
                        switch (wave)
                        {
                            case 0:
                                SummonParameters.AddRange(map.GenerateMonsters(120, 15, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(151, 15, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(149, 15, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(139, 15, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(73, 16, true, new List<EventContainer>()));
                                break;

                            case 1:
                                SummonParameters.AddRange(map.GenerateMonsters(152, 15, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(147, 15, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(104, 15, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(62, 15, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(8, 16, true, new List<EventContainer>()));
                                break;

                            case 2:
                                SummonParameters.AddRange(map.GenerateMonsters(153, 15, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(132, 15, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(86, 15, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(76, 15, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(68, 16, true, new List<EventContainer>()));
                                break;

                            case 3:
                                SummonParameters.AddRange(map.GenerateMonsters(134, 15, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(91, 15, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(133, 15, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(70, 15, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(89, 16, true, new List<EventContainer>()));
                                break;

                            case 4:
                                SummonParameters.AddRange(map.GenerateMonsters(154, 15, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(200, 15, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(77, 8, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(217, 15, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(724, 1, true, new List<EventContainer>()));
                                break;
                        }
                        break;

                    case 50:
                        switch (wave)
                        {
                            case 0:
                                SummonParameters.AddRange(map.GenerateMonsters(134, 15, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(91, 15, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(89, 15, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(77, 15, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(71, 16, true, new List<EventContainer>()));
                                break;

                            case 1:
                                SummonParameters.AddRange(map.GenerateMonsters(217, 15, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(200, 15, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(154, 15, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(92, 15, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(79, 16, true, new List<EventContainer>()));
                                break;

                            case 2:
                                SummonParameters.AddRange(map.GenerateMonsters(235, 15, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(226, 15, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(214, 15, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(204, 15, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(201, 15, true, new List<EventContainer>()));
                                break;

                            case 3:
                                SummonParameters.AddRange(map.GenerateMonsters(249, 15, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(236, 15, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(227, 15, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(218, 15, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(202, 15, true, new List<EventContainer>()));
                                break;

                            case 4:
                                SummonParameters.AddRange(map.GenerateMonsters(583, 1, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(400, 13, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(255, 8, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(253, 13, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(251, 10, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(205, 14, true, new List<EventContainer>()));
                                break;
                        }
                        break;

                    case 60:
                        switch (wave)
                        {
                            case 0:
                                SummonParameters.AddRange(map.GenerateMonsters(242, 12, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(234, 12, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(215, 12, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(207, 12, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(202, 13, true, new List<EventContainer>()));
                                break;

                            case 1:
                                SummonParameters.AddRange(map.GenerateMonsters(402, 12, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(253, 12, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(237, 12, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(216, 12, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(205, 13, true, new List<EventContainer>()));
                                break;

                            case 2:
                                SummonParameters.AddRange(map.GenerateMonsters(402, 12, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(243, 12, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(228, 12, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(255, 12, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(205, 13, true, new List<EventContainer>()));
                                break;

                            case 3:
                                SummonParameters.AddRange(map.GenerateMonsters(268, 12, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(255, 12, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(254, 12, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(174, 12, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(172, 13, true, new List<EventContainer>()));
                                break;

                            case 4:
                                SummonParameters.AddRange(map.GenerateMonsters(725, 1, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(407, 12, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(272, 12, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(261, 12, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(256, 12, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(275, 13, true, new List<EventContainer>()));
                                break;
                        }
                        break;

                    case 70:
                        switch (wave)
                        {
                            case 0:
                                SummonParameters.AddRange(map.GenerateMonsters(402, 15, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(253, 15, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(237, 15, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(216, 15, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(205, 15, true, new List<EventContainer>()));
                                break;

                            case 1:
                                SummonParameters.AddRange(map.GenerateMonsters(402, 15, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(243, 15, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(228, 15, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(225, 15, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(205, 15, true, new List<EventContainer>()));
                                break;

                            case 2:
                                SummonParameters.AddRange(map.GenerateMonsters(255, 15, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(254, 15, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(251, 15, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(174, 15, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(172, 15, true, new List<EventContainer>()));
                                break;

                            case 3:
                                SummonParameters.AddRange(map.GenerateMonsters(407, 15, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(272, 15, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(261, 15, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(257, 15, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(256, 15, true, new List<EventContainer>()));
                                break;

                            case 4:
                                SummonParameters.AddRange(map.GenerateMonsters(748, 1, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(444, 13, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(439, 13, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(275, 13, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(274, 13, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(273, 13, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(163, 13, true, new List<EventContainer>()));
                                break;
                        }
                        break;

                    case 80:
                        switch (wave)
                        {
                            case 0:
                                SummonParameters.AddRange(map.GenerateMonsters(1007, 15, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(1003, 15, false, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(1002, 15, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(1001, 15, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(1000, 16, true, new List<EventContainer>()));
                                break;

                            case 1:
                                SummonParameters.AddRange(map.GenerateMonsters(1199, 15, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(1198, 15, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(1197, 15, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(1196, 15, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(1123, 16, true, new List<EventContainer>()));
                                break;

                            case 2:
                                SummonParameters.AddRange(map.GenerateMonsters(1305, 15, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(1304, 15, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(1303, 15, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(1302, 15, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(1194, 16, true, new List<EventContainer>()));
                                break;

                            case 3:
                                SummonParameters.AddRange(map.GenerateMonsters(1902, 15, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(1901, 15, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(1900, 15, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(1045, 15, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(1043, 15, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(1042, 16, true, new List<EventContainer>()));
                                break;

                            case 4:
                                SummonParameters.AddRange(map.GenerateMonsters(637, 1, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(1903, 13, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(1053, 13, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(1051, 13, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(1049, 13, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(1048, 13, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(1047, 13, true, new List<EventContainer>()));
                                break;
                        }
                        break;
                    case 100:
                        switch (wave)
                        {
                            case 0:
                                // Draco
                                SummonParameters.AddRange(map.GenerateMonsters(2034, 1, true, new List<EventContainer>()));
                                break;
                            case 1:
                                // Kertos
                                SummonParameters.AddRange(map.GenerateMonsters(1046, 1, true, new List<EventContainer>()));
                                break;
                            case 2:
                                // Valakus
                                SummonParameters.AddRange(map.GenerateMonsters(1044, 1, true, new List<EventContainer>()));
                                break;
                            case 3:
                                // Grenigas
                                SummonParameters.AddRange(map.GenerateMonsters(1058, 1, true, new List<EventContainer>()));
                                break;
                            case 4:
                                // Zenas
                                SummonParameters.AddRange(map.GenerateMonsters(2504, 1, true, new List<EventContainer>()));

                                break;
                        }
                        break;
                    case 130:
                        switch (wave)
                        {
                            case 0:
                                SummonParameters.AddRange(map.GenerateMonsters(3003, 20, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(3000, 20, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(3001, 20, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(3019, 20, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(3002, 20, true, new List<EventContainer>()));

                                // Carno avatar
                                SummonParameters.AddRange(map.GenerateMonsters(3039, 1, true, new List<EventContainer>()));
                                // Kiro avatar
                                SummonParameters.AddRange(map.GenerateMonsters(3040, 1, true, new List<EventContainer>()));
                                break;
                            case 1:
                                SummonParameters.AddRange(map.GenerateMonsters(3016, 20, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(3103, 20, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(3104, 20, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(3024, 20, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(3026, 20, true, new List<EventContainer>()));

                                // Zenas
                                SummonParameters.AddRange(map.GenerateMonsters(2504, 1, true, new List<EventContainer>()));
                                break;
                            case 2:
                                SummonParameters.AddRange(map.GenerateMonsters(3025, 20, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(3013, 20, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(3014, 20, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(3015, 20, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(3105, 20, true, new List<EventContainer>()));

                                // Kirollas
                                SummonParameters.AddRange(map.GenerateMonsters(3027, 1, true, new List<EventContainer>()));
                                break;
                            case 3:
                                SummonParameters.AddRange(map.GenerateMonsters(3009, 20, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(3010, 20, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(3008, 20, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(3024, 20, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(3021, 20, true, new List<EventContainer>()));

                                // Carno
                                SummonParameters.AddRange(map.GenerateMonsters(3028, 1, true, new List<EventContainer>()));
                                break;
                            case 4:
                                SummonParameters.AddRange(map.GenerateMonsters(3055, 20, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(3110, 20, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(3111, 20, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(3112, 20, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(3113, 20, true, new List<EventContainer>()));
                                SummonParameters.AddRange(map.GenerateMonsters(3114, 20, true, new List<EventContainer>()));

                                // Belial
                                SummonParameters.AddRange(map.GenerateMonsters(3029, 1, true, new List<EventContainer>()));
                                break;
                        }
                        break;
                }
                return SummonParameters;
            }

            #endregion
        }

        #endregion
    }
}