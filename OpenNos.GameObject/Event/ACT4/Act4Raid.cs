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
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using NosByte.Scheduler;
using NosByte.Shared;
using OpenNos.Core.Scheduler;
using OpenNos.Core.Logger;

namespace OpenNos.GameObject.Event
{
    public static class Act4Raid
    {
        #region Properties

        public static ConcurrentBag<MapMonster> Guardians { get; set; }

        #endregion

        #region Methods

        public static void GenerateRaid(MapInstanceType raidType, FactionType faction)
        {
            Guardians = new ConcurrentBag<MapMonster>();
            MapInstance bitoren = ServerManager.GetMapInstance(ServerManager.GetBaseMapInstanceIdByMapId(134));
            bitoren.CreatePortal(new Portal
            {
                SourceMapId = 134,
                SourceX = 140,
                SourceY = 100,
                DestinationMapId = 0,
                DestinationX = 1,
                DestinationY = 1,
                Type = (short)(9 + faction)
            });

            #region Guardian Spawning

            var guardianVnum = (short) (678 + (byte) faction);

            Guardians.Add(new MapMonster
            {
                MonsterVNum = guardianVnum,
                MapX = 147,
                MapY = 88,
                MapId = 134,
                Position = 2,
                IsMoving = false,
                MapMonsterId = bitoren.GetNextMonsterId(),
                ShouldRespawn = false,
                IsHostile = true
            });
            Guardians.Add(new MapMonster
            {
                MonsterVNum = guardianVnum,
                MapX = 149,
                MapY = 94,
                MapId = 134,
                Position = 2,
                IsMoving = false,
                MapMonsterId = bitoren.GetNextMonsterId(),
                ShouldRespawn = false,
                IsHostile = true
            });
            Guardians.Add(new MapMonster
            {
                MonsterVNum = guardianVnum,
                MapX = 147,
                MapY = 101,
                MapId = 134,
                Position = 2,
                IsMoving = false,
                MapMonsterId = bitoren.GetNextMonsterId(),
                ShouldRespawn = false,
                IsHostile = true
            });
            Guardians.Add(new MapMonster
            {
                MonsterVNum = guardianVnum,
                MapX = 139,
                MapY = 105,
                MapId = 134,
                Position = 2,
                IsMoving = false,
                MapMonsterId = bitoren.GetNextMonsterId(),
                ShouldRespawn = false,
                IsHostile = true
            });
            Guardians.Add(new MapMonster
            {
                MonsterVNum = guardianVnum,
                MapX = 132,
                MapY = 101,
                MapId = 134,
                Position = 2,
                IsMoving = false,
                MapMonsterId = bitoren.GetNextMonsterId(),
                ShouldRespawn = false,
                IsHostile = true
            });
            Guardians.Add(new MapMonster
            {
                MonsterVNum = guardianVnum,
                MapX = 129,
                MapY = 94,
                MapId = 134,
                Position = 2,
                IsMoving = false,
                MapMonsterId = bitoren.GetNextMonsterId(),
                ShouldRespawn = false,
                IsHostile = true
            });
            Guardians.Add(new MapMonster
            {
                MonsterVNum = guardianVnum,
                MapX = 132,
                MapY = 88,
                MapId = 134,
                Position = 2,
                IsMoving = false,
                MapMonsterId = bitoren.GetNextMonsterId(),
                ShouldRespawn = false,
                IsHostile = true
            });

            #endregion

            foreach (MapMonster monster in Guardians)
            {
                monster.Initialize(bitoren);
                bitoren.AddMonster(monster);
                bitoren.Broadcast(monster.GenerateIn());
            }

            ServerManager.Instance.Broadcast(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("ACT4_BITOREN_RAID_OPEN"), 0));
            new Act4RaidThread().Run(raidType, faction);
        }

        #endregion
    }

    public class Act4RaidThread
    {
        #region Members

        private short _bossMapId = 136;

        private bool _bossMove;

        private short _bossVNum = 563;

        private short _bossX = 55;

        private short _bossY = 11;

        private short _destPortalX = 55;

        private short _destPortalY = 80;

        private FactionType _faction;

        private short _mapId = 135;

        private int _raidTime = 3600;
        private MapInstanceType _raidType;

        private short _sourcePortalX = 146;

        private short _sourcePortalY = 43;

        private IDisposable _raidInterval;

        #endregion

        #region Methods

        public void Run(MapInstanceType raidType, FactionType faction)
        {
            _raidType = raidType;
            _faction = faction;
            switch (raidType)
            {
                // Morcos is default
                case MapInstanceType.Act4Hatus:
                    _mapId = 137;
                    _bossMapId = 138;
                    _bossVNum = 577;
                    _bossX = 36;
                    _bossY = 18;
                    _sourcePortalX = 37;
                    _sourcePortalY = 156;
                    _destPortalX = 36;
                    _destPortalY = 58;
                    _bossMove = false;
                    break;

                case MapInstanceType.Act4Calvina:
                    _mapId = 139;
                    _bossMapId = 140;
                    _bossVNum = 629;
                    _bossX = 26;
                    _bossY = 26;
                    _sourcePortalX = 194;
                    _sourcePortalY = 17;
                    _destPortalX = 9;
                    _destPortalY = 41;
                    _bossMove = true;
                    break;

                case MapInstanceType.Act4Berios:
                    _mapId = 141;
                    _bossMapId = 142;
                    _bossVNum = 624;
                    _bossX = 29;
                    _bossY = 29;
                    _sourcePortalX = 188;
                    _sourcePortalY = 96;
                    _destPortalX = 29;
                    _destPortalY = 54;
                    _bossMove = true;
                    break;
            }

#if DEBUG
            _raidTime = 3600;
#endif

            //Run once to load everything in place
            RefreshRaid(_raidTime);

            ServerManager.Instance.Act4RaidStart = DateTime.Now;

            _raidInterval = Observable.Interval(TimeSpan.FromSeconds(60)).SafeSubscribe(s =>
            {
                _raidTime -= 120;
                RefreshRaid(_raidTime);
            });

            Observable.Timer(TimeSpan.FromMinutes(30)).SafeSubscribe(s => { EndRaid(); });
        }

        public void OpenCaligor()
        {
            EventHelper.GenerateEvent(EventType.CALIGOR);
        }

        private void EndRaid()
        {
            foreach (Family fam in ServerManager.Instance.FamilyList.GetAllItems())
            {
                if (fam.Act4Raid != null)
                {
                    fam.Act4Raid?.StopLife();
                    EventHelper.Instance.RunEvent(new EventContainer(fam.Act4Raid, EventActionType.DISPOSEMAP, null));
                    fam.Act4Raid = null;
                }
                if (fam.Act4RaidBossMap != null)
                {
                    fam.Act4RaidBossMap?.StopLife();
                    EventHelper.Instance.RunEvent(new EventContainer(fam.Act4RaidBossMap, EventActionType.DISPOSEMAP, null));
                    fam.Act4RaidBossMap = null;
                }
            }
            MapInstance bitoren = ServerManager.GetMapInstance(ServerManager.GetBaseMapInstanceIdByMapId(134));

            foreach (Portal portal in bitoren.Portals.ToList().Where(s => s.Type.Equals(10) || s.Type.Equals(11)))
            {
                portal.IsDisabled = true;
                bitoren.Broadcast(portal.GenerateGp());
                bitoren.Portals.Remove(portal);
            }

            bitoren.Portals.RemoveAll(s => s.Type.Equals(10));
            bitoren.Portals.RemoveAll(s => s.Type.Equals(11));
            switch (_faction)
            {
                case FactionType.Angel:
                    ServerManager.Instance.Act4AngelStat.Mode = 0;
                    ServerManager.Instance.Act4AngelStat.IsMorcos = false;
                    ServerManager.Instance.Act4AngelStat.IsHatus = false;
                    ServerManager.Instance.Act4AngelStat.IsCalvina = false;
                    ServerManager.Instance.Act4AngelStat.IsBerios = false;
                    break;

                case FactionType.Demon:
                    ServerManager.Instance.Act4DemonStat.Mode = 0;
                    ServerManager.Instance.Act4DemonStat.IsMorcos = false;
                    ServerManager.Instance.Act4DemonStat.IsHatus = false;
                    ServerManager.Instance.Act4DemonStat.IsCalvina = false;
                    ServerManager.Instance.Act4DemonStat.IsBerios = false;
                    break;
            }

            ServerManager.Instance.StartedEvents.Remove(EventType.Act4Raid);

            foreach (MapMonster monster in Act4Raid.Guardians)
            {
                bitoren.Broadcast(StaticPacketHelper.Out(UserType.Monster, monster.MapMonsterId));
                bitoren.RemoveMonster(monster);
            }

            Act4Raid.Guardians.Clear();
            _raidInterval?.Dispose();
        }

        private void OpenRaid(Family fam)
        {
            try
            {
                fam.Act4RaidBossMap.OnCharacterDiscoveringMapEvents.Add(new Tuple<EventContainer, List<long>>(new EventContainer(fam.Act4RaidBossMap, EventActionType.STARTACT4RAIDWAVES, new List<long>()), new List<long>()));
                List<EventContainer> onDeathEvents = new List<EventContainer>
                {
                new EventContainer(fam.Act4RaidBossMap, EventActionType.THROWITEMS, new Tuple<int, short, byte, int, int, short>(_bossVNum, 1046, 10, 20000, 20001, 0)),
                new EventContainer(fam.Act4RaidBossMap, EventActionType.THROWITEMS, new Tuple<int, short, byte, int, int, short>(_bossVNum, 1244, 10, 5, 6, 0))
                };
                if (_raidType.Equals(MapInstanceType.Act4Berios))
                {
                    onDeathEvents.Add(new EventContainer(fam.Act4RaidBossMap, EventActionType.THROWITEMS, new Tuple<int, short, byte, int, int, short>(_bossVNum, 2395, 3, 1, 2, 0)));
                    onDeathEvents.Add(new EventContainer(fam.Act4RaidBossMap, EventActionType.THROWITEMS, new Tuple<int, short, byte, int, int, short>(_bossVNum, 2396, 5, 1, 2, 0)));
                    onDeathEvents.Add(new EventContainer(fam.Act4RaidBossMap, EventActionType.THROWITEMS, new Tuple<int, short, byte, int, int, short>(_bossVNum, 2397, 10, 1, 2, 0)));

                    fam.Act4RaidBossMap.OnCharacterDiscoveringMapEvents.Add(new Tuple<EventContainer, List<long>>(new EventContainer(fam.Act4RaidBossMap, EventActionType.SPAWNMONSTER, new MonsterToSummon(621, fam.Act4RaidBossMap.Map.GetRandomPosition(), null, move: true, hasDelay: 30)), new List<long>()));
                    fam.Act4RaidBossMap.OnCharacterDiscoveringMapEvents.Add(new Tuple<EventContainer, List<long>>(new EventContainer(fam.Act4RaidBossMap, EventActionType.SPAWNMONSTER, new MonsterToSummon(622, fam.Act4RaidBossMap.Map.GetRandomPosition(), null, move: true, hasDelay: 205)), new List<long>()));
                    fam.Act4RaidBossMap.OnCharacterDiscoveringMapEvents.Add(new Tuple<EventContainer, List<long>>(new EventContainer(fam.Act4RaidBossMap, EventActionType.SPAWNMONSTER, new MonsterToSummon(623, fam.Act4RaidBossMap.Map.GetRandomPosition(), null, move: true, hasDelay: 380)), new List<long>()));
                }
                onDeathEvents.Add(new EventContainer(fam.Act4RaidBossMap, EventActionType.SCRIPTEND, (byte)1));
                onDeathEvents.Add(new EventContainer(fam.Act4Raid, EventActionType.CHANGEPORTALTYPE, new Tuple<int, PortalType>(fam.Act4Raid.Portals.Find(s => s.SourceX == _sourcePortalX && s.SourceY == _sourcePortalY && !s.IsDisabled).PortalId, PortalType.Closed)));
                MonsterToSummon bossMob = new MonsterToSummon(_bossVNum, new MapCell { X = _bossX, Y = _bossY }, null, _bossMove)
                {
                    DeathEvents = onDeathEvents
                };
                EventHelper.Instance.RunEvent(new EventContainer(fam.Act4RaidBossMap, EventActionType.SPAWNMONSTER, bossMob));
                EventHelper.Instance.RunEvent(new EventContainer(fam.Act4Raid, EventActionType.SENDPACKET, UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("ACT4RAID_OPEN"), 0)));
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
                EndRaid();
            }
        }

        private void RefreshRaid(int remaining)
        {
            var famList = ServerManager.Instance.FamilyList.GetAllItems();
            foreach (var fam in famList)
            {
                fam.Act4Raid ??= ServerManager.GenerateMapInstance(_mapId, _raidType, new InstanceBag(), true);
                fam.Act4RaidBossMap ??= ServerManager.GenerateMapInstance(_bossMapId, _raidType, new InstanceBag());
                if (remaining <= 3600 && !fam.Act4Raid.Portals.Any(s => s.DestinationMapInstanceId.Equals(fam?.Act4RaidBossMap?.MapInstanceId)))
                {
                    fam.Act4Raid.CreatePortal(new Portal
                    {
                        DestinationMapInstanceId = fam.Act4RaidBossMap.MapInstanceId,
                        DestinationX = _destPortalX,
                        DestinationY = _destPortalY,
                        SourceX = _sourcePortalX,
                        SourceY = _sourcePortalY,
                    });
                    OpenRaid(fam);
                }

                if (fam?.Act4RaidBossMap?.Monsters?.Find(s => s.MonsterVNum == _bossVNum && s.CurrentHp / s.MaxHp < 0.5) == null || fam?.Act4Raid?.Portals?.Find(s => s.SourceX == _sourcePortalX && s.SourceY == _sourcePortalY && !s.IsDisabled) == null)
                {
                    continue;
                }

                EventHelper.Instance.RunEvent(new EventContainer(fam.Act4Raid, EventActionType.CHANGEPORTALTYPE, new Tuple<int, PortalType>(fam.Act4Raid.Portals.Find(s => s.SourceX == _sourcePortalX && s.SourceY == _sourcePortalY && !s.IsDisabled).PortalId, PortalType.Closed)));
                fam.Act4Raid.Broadcast(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("PORTAL_CLOSED"), 0));
                fam.Act4RaidBossMap.Broadcast(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("PORTAL_CLOSED"), 0));
            }
        }

        #endregion
    }
}