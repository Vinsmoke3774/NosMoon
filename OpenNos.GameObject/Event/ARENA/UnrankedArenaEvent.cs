using OpenNos.Core;
using OpenNos.Domain;
using OpenNos.GameObject.Extension.CharacterExtensions;
using OpenNos.GameObject.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using NosByte.Shared;
using OpenNos.GameObject.Helpers;
using System.Threading;

namespace OpenNos.GameObject.Event.ARENA
{
    internal static class UnrankedArenaEvent
    {
        private static bool _isRunning = false;

        internal static void Run()
        {
            _isRunning = true;
        }

        internal static void Matchmaking(ClientSession session)
        {
            if (!_isRunning)
            {
                session.SendPacket(UserInterfaceHelper.GenerateMsg("Talent arena isn't opened", 0));
                return;
            }

            session.Character.Group?.LeaveGroup(session);

            var getSessionRankFromRP = CompetitiveRank.GetRankFromRp(session.Character.RP);
            IObservable<long> observable = Observable.Interval(TimeSpan.FromSeconds(1));

            void action(ArenaMember member)
            {
                session.Character.DefaultTimer--;

                // Timer
                session.SendPacket(UserInterfaceHelper.GenerateBSInfo(0, 2, session.Character.DefaultTimer, 1));
                if (session.Character.DefaultTimer == 119) session.SendPacket(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("SEARCH_RIVAL_ARENA_TEAM"), 10));

                if (session.Character.LastDefence.AddSeconds(2) > DateTime.Now)
                {
                    session.SendPacket(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("ARENA_LAUNCH_CANCELED_DUE_TO_FIGHT"), 10));
                    session.Character.ArenaDisposable.Dispose();
                    return;
                }

                if (!_isRunning)
                {
                    session.SendPacket(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("ARENA_MATCHMAKING_CANCELED_CAUSE_ARENA_CLOSED"), 10));
                    session.Character.ArenaDisposable.Dispose();
                    return;
                }

                if (session.Character.DefaultTimer < 0)
                {
                    session.SendPacket(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("ARENA_OPPONENT_NOT_FOUND"), 10));
                    return;
                }

                if (member == null) return;

                InitalizeVariables(member, session);
                TeleportToLobby(session, member.Session);
            }

            session.Character.ArenaDisposable = observable.SafeSubscribe(x =>
            {
                var opponent = ServerManager.Instance.ArenaMembers.FirstOrDefault(s => s.Session.Character.CharacterId != session.Character.CharacterId && s.ArenaType == EventType.UNRANKEDTALENTARENA && ((Enumerable.Range(30, 59).Contains(s.Session.Character.Level) && Enumerable.Range(30, 59).Contains(session.Character.Level)) || (Enumerable.Range(60, 79).Contains(s.Session.Character.Level) && Enumerable.Range(60, 79).Contains(session.Character.Level)) || (Enumerable.Range(80, 99).Contains(s.Session.Character.Level) && Enumerable.Range(80, 99).Contains(session.Character.Level))));
                action(opponent);
            });

            if (session.Character.Level >= 88)
            {
                Observable.Timer(TimeSpan.FromMinutes(2)).SafeSubscribe(x =>
                {
                    session.Character.ArenaDisposable.Dispose();
                    session.Character.DefaultTimer = 120;

                    session.Character.ArenaDisposable = observable.SafeSubscribe(x =>
                    {
                        var closestSession = ServerManager.Instance.ArenaMembers.OrderBy(item => Math.Abs(session.Character.HeroLevel - item.Session.Character.HeroLevel)).First();
                        var opponent = ServerManager.Instance.ArenaMembers.FirstOrDefault(s => s.Session.Character.CharacterId != session.Character.CharacterId && s.ArenaType == EventType.UNRANKEDTALENTARENA && s.Session.Character.HeroLevel == closestSession.Session.Character.HeroLevel);
                        if (opponent != null) session.Character.ArenaDisposable.Dispose();
                        action(opponent);
                    });
                });
            }
            else
            {
                session.Character.ArenaDisposable.Dispose();
            }
        }

        private static void InitalizeVariables(ArenaMember member, ClientSession session)
        {
            session.Character.TalentArenaBattle.Round = 0;
            member.Session.Character.TalentArenaBattle.Round = 0;
            session.Character.TalentArenaBattle.RoundWin = 0;
            member.Session.Character.TalentArenaBattle.RoundWin = 0;
            session.Character.TalentArenaBattle.ArenaTeamType = ArenaTeamType.Erenia;
            member.Session.Character.TalentArenaBattle.ArenaTeamType = ArenaTeamType.Zenas;
            session.Character.TalentArenaBattle.Opponent = member.Session;
            member.Session.Character.TalentArenaBattle.Opponent = session;
            session.Character.LastMapId = session.Character.MapId;
            session.Character.LastMapX = session.Character.MapX;
            session.Character.LastMapY = session.Character.MapY;
            member.Session.Character.LastMapId = member.Session.Character.MapId;
            member.Session.Character.LastMapX = member.Session.Character.MapX;
            member.Session.Character.LastMapY = member.Session.Character.MapY;
        }

        private static void EndGame(ClientSession session)
        {
            List<BuffType> bufftodisable = new() { BuffType.All };

            session.Character.DisableBuffs(bufftodisable);
            session.Character.Hp = (int)session.Character.HPLoad();
            session.Character.Mp = (int)session.Character.MPLoad();
            session.Character.IsInArenaLobby = false;
            session.Character.IsInFightZone = false;
            session.SendPacket(session.Character.GenerateStat());

            if (session.Character.UseSp)
            {
                session.Character.SkillsSp.ForEach(s => s.LastUse = DateTime.Now.AddDays(-1));
            }

            session.SendPacket(session.Character.GenerateSki());
            session.SendPackets(session.Character.GenerateQuicklist());

            var mapInstance = ServerManager.GetMapInstanceByMapId(session.Character.LastMapId);
            ServerManager.Instance.ChangeMapInstance(session.Character.CharacterId, mapInstance.MapInstanceId, session.Character.LastMapX, session.Character.LastMapY);

            Observable.Timer(TimeSpan.FromMilliseconds(500)).SafeSubscribe(x =>
            {
                session.SendPacket(session.Character.GenerateTaM(1));
                session.CurrentMapInstance.Broadcast(session.Character.GenerateRevive());
            });
        }

        private static void GenerateWin(ClientSession session)
        {
            session.Character.TalentWin++;
            session.Character.GiftAdd(1, 1);

            session.SendPacket(UserInterfaceHelper.GenerateMsg("You won the game, you will be teleported in 2 seconds", 0));

            var quest = ServerManager.Instance.BattlePassQuests.Find(s => s.MissionSubType == BattlePassMissionSubType.WinAoTGames);
            if (quest != null)
            {
                session.Character.IncreaseBattlePassQuestObjectives(quest.Id, 1);
            }

            Observable.Timer(TimeSpan.FromSeconds(2)).SafeSubscribe(x =>
            {
                EndGame(session);
            });
        }

        private static void GenerateLose(ClientSession session)
        {
            session.Character.TalentLose++;

            session.SendPacket(UserInterfaceHelper.GenerateMsg("Opponent won the game, you will be teleported in 2 seconds", 0));

            Observable.Timer(TimeSpan.FromSeconds(2)).SafeSubscribe(x =>
            {
                EndGame(session);
            });
        }

        private static void TeleportToDeadLobby(ClientSession session, ClientSession opponent, bool isSuddenDeath = false)
        {
            var msg2 = UserInterfaceHelper.GenerateMsg("You have 30 seconds to prepare before being teleported into the fight zone !", 0);

            // teleport to fight zone
            session.CurrentMapInstance.InstanceBag.Clock.TotalSecondsAmount = 30;
            session.CurrentMapInstance.InstanceBag.Clock.SecondsRemaining = 300;
            session.CurrentMapInstance.InstanceBag.Clock.StartClock();

            Observable.Timer(TimeSpan.FromSeconds(session.CurrentMapInstance.InstanceBag.Clock.TotalSecondsAmount)).SafeSubscribe(s => TeleportInFightZone(session, opponent));

            List<BuffType> bufftodisable = new() { BuffType.All };

            opponent.Character.DisableBuffs(bufftodisable);
            opponent.Character.Hp = (int)opponent.Character.HPLoad();
            opponent.Character.Mp = (int)opponent.Character.MPLoad();

            session.Character.DisableBuffs(bufftodisable);
            session.Character.Hp = (int)session.Character.HPLoad();
            session.Character.Mp = (int)session.Character.MPLoad();

            if (session.Character.TalentArenaBattle.IsDead)
            {
                session.Character.PositionX = 120;
                session.Character.PositionY = 39;
                session.CurrentMapInstance.Broadcast(session, session.Character.GenerateTp());
                session.Character.TalentArenaBattle.IsDead = false;
                Observable.Timer(TimeSpan.FromSeconds(1)).SafeSubscribe(x =>
                {
                    session.CurrentMapInstance.Broadcast(session.Character.GenerateRevive());
                });
            }
            else if (opponent.Character.TalentArenaBattle.IsDead)
            {
                opponent.Character.PositionX = 19;
                opponent.Character.PositionY = 40;
                opponent.CurrentMapInstance.Broadcast(opponent, opponent.Character.GenerateTp());
                opponent.Character.TalentArenaBattle.IsDead = false;
                Observable.Timer(TimeSpan.FromSeconds(1)).SafeSubscribe(x =>
                {
                    opponent.CurrentMapInstance.Broadcast(opponent.Character.GenerateRevive());
                });
            }


            session.CurrentMapInstance.Broadcast(msg2);

            session.SendPacket(session.Character.GenerateStat());
            session.SendPacket(session.Character.GenerateSki());
            session.SendPackets(session.Character.GenerateQuicklist());

            opponent.SendPacket(opponent.Character.GenerateStat());
            opponent.SendPacket(opponent.Character.GenerateSki());
            opponent.SendPackets(opponent.Character.GenerateQuicklist());

            session.CurrentMapInstance.IsPVP = false;
            session.CurrentMapInstance.Broadcast(session.Character.GenerateTaM(0));
            session.CurrentMapInstance.Broadcast(session.Character.GenerateTaM(3));

            if (isSuddenDeath)
            {
                Observable.Timer(TimeSpan.FromSeconds(30)).SafeSubscribe(s =>
                {
                    session.CurrentMapInstance.Broadcast(UserInterfaceHelper.GenerateMsg("Sudden death, next player to win the round will win the match !", 0));
                });
            }

            SetLobbyValues(session, opponent, true, false);
        }

        private static void TeleportToLobby(ClientSession session, ClientSession opponent)
        {
            if (session == null || opponent == null) return;

            if (session.Character.IsVehicled) session.Character.RemoveVehicle();

            if (opponent.Character.IsVehicled) opponent.Character.RemoveVehicle();

            session.Character.ArenaDisposable.Dispose();
            opponent.Character.ArenaDisposable.Dispose();
            session.CurrentMapInstance.Broadcast(UserInterfaceHelper.GenerateBSInfo(2, 2, 0, 0));

            MapInstance map = ServerManager.GenerateMapInstance(2015, MapInstanceType.TalentArenaMapInstance, new InstanceBag());

            ServerManager.Instance.ChangeMapInstance(session.Character.CharacterId, map.MapInstanceId, 120, 39);
            ServerManager.Instance.ChangeMapInstance(opponent.Character.CharacterId, map.MapInstanceId, 19, 40);

            map.InstanceBag.Clock.TotalSecondsAmount = 60;
            map.InstanceBag.Clock.SecondsRemaining = 600;
            map.InstanceBag.Clock.StartClock();

            Observable.Timer(TimeSpan.FromSeconds(map.InstanceBag.Clock.TotalSecondsAmount)).SafeSubscribe(s => TeleportInFightZone(session, opponent));

            SetLobbyValues(session, opponent, true, false);
            session.Character.TalentArenaBattle.IsDead = false;
            opponent.Character.TalentArenaBattle.IsDead = false;
            session.CurrentMapInstance.IsPVP = false;

            List<BuffType> bufftodisable = new() { BuffType.All };

            session.Character.DisableBuffs(bufftodisable);
            session.Character.Hp = (int)session.Character.HPLoad();
            session.Character.Mp = (int)session.Character.MPLoad();
            session.SendPacket(session.Character.GenerateStat());

            opponent.Character.DisableBuffs(bufftodisable);
            opponent.Character.Hp = (int)opponent.Character.HPLoad();
            opponent.Character.Mp = (int)opponent.Character.MPLoad();
            opponent.SendPacket(opponent.Character.GenerateStat());

            if (session.Character.UseSp)
            {
                session.Character.SkillsSp.ForEach(s => s.LastUse = DateTime.Now.AddDays(-1));
            }

            session.SendPacket(session.Character.GenerateSki());
            session.SendPackets(session.Character.GenerateQuicklist());

            if (opponent.Character.UseSp)
            {
                opponent.Character.SkillsSp.ForEach(s => s.LastUse = DateTime.Now.AddDays(-1));
            }

            opponent.SendPacket(opponent.Character.GenerateSki());
            opponent.SendPackets(opponent.Character.GenerateQuicklist());

            session.SendPacket(UserInterfaceHelper.GenerateSay("You are in team Erenia", 1));
            opponent.SendPacket(UserInterfaceHelper.GenerateSay("You are in team Zenas", 1));

            switch (session.Character.TalentArenaBattle.Round)
            {
                case 0:
                    var msg = UserInterfaceHelper.GenerateMsg("You have 60 seconds to prepare before being teleported into the fight zone !", 0);
                    session.CurrentMapInstance.Broadcast(msg);
                    break;

                default:
                    var msg2 = UserInterfaceHelper.GenerateMsg("You have 30 seconds to prepare before being teleported into the fight zone !", 0);
                    session.CurrentMapInstance.Broadcast(msg2);

                    // teleport to fight zone
                    session.CurrentMapInstance.InstanceBag.Clock.TotalSecondsAmount = 30;
                    session.CurrentMapInstance.InstanceBag.Clock.SecondsRemaining = 300;
                    session.CurrentMapInstance.InstanceBag.Clock.StartClock();

                    Observable.Timer(TimeSpan.FromSeconds(session.CurrentMapInstance.InstanceBag.Clock.TotalSecondsAmount)).SafeSubscribe(s => TeleportInFightZone(session, opponent));
                    break;
            }

            session.SendPacket(session.Character.GenerateTaP(opponent.Character, 0, session.Character.TalentArenaBattle.ArenaTeamType, false));
            opponent.SendPacket(opponent.Character.GenerateTaP(session.Character, 0, opponent.Character.TalentArenaBattle.ArenaTeamType, false));
            session.SendPacket(session.Character.GenerateTaPs(opponent.Character));
            opponent.SendPacket(session.Character.GenerateTaPs(session.Character));
            session.CurrentMapInstance.Broadcast(session.Character.GenerateTaM(0));
            session.CurrentMapInstance.Broadcast(session.Character.GenerateTaM(3));
        }

        private static void SetLobbyValues(ClientSession session, ClientSession opponent, bool lobbyValue, bool fightZoneValue)
        {
            session.Character.IsInArenaLobby = lobbyValue;
            opponent.Character.IsInArenaLobby = lobbyValue;
            session.Character.IsInFightZone = fightZoneValue;
            opponent.Character.IsInFightZone = fightZoneValue;
        }

        private static void TeleportInFightZone(ClientSession session, ClientSession opponent)
        {
            SetLobbyValues(session, opponent, false, true);

            // Messages
            if (!session.Character.TalentArenaBattle.TieBreaker)
            {
                session.CurrentMapInstance.Broadcast(UserInterfaceHelper.GenerateMsg("The fight is starting, good luck !", 0));
            }
            else
            {
                session.CurrentMapInstance.Broadcast(UserInterfaceHelper.GenerateMsg("TiedBreaker: The next player to win two rounds in a row wins the game !", 0));
            }

            // Teleport into the fight zone
            session.Character.PositionX = 87;
            session.Character.PositionY = 39;
            session.CurrentMapInstance.Broadcast(session, session.Character.GenerateTp());
            opponent.Character.PositionX = 56;
            opponent.Character.PositionY = 40;
            opponent.CurrentMapInstance.Broadcast(opponent, opponent.Character.GenerateTp());

            // Enable PVP
            session.CurrentMapInstance.IsPVP = true;

            while (!FightZoneAction(session, opponent))
            {
                Thread.Sleep(1000);
            }
        }

        private static bool FightZoneAction(ClientSession session, ClientSession opponent)
        {
            if (!session.Character.TalentArenaBattle.IsDead && !opponent.Character.TalentArenaBattle.IsDead) return false; // We don't have anything to do there as no one is dead yet

            if (session.Character.TalentArenaBattle.TieBreaker)
            {
                switch (session.Character.TalentArenaBattle.Round)
                {
                    case 0:
                        if (opponent.Character.TalentArenaBattle.IsDead)
                        {
                            session.Character.TalentArenaBattle.RoundWin++;
                        }
                        else if (session.Character.TalentArenaBattle.IsDead)
                        {
                            opponent.Character.TalentArenaBattle.RoundWin++;
                        }

                        session.Character.TalentArenaBattle.Round++;
                        TeleportToDeadLobby(session, opponent);
                        break;

                    case 1:
                        if (opponent.Character.TalentArenaBattle.IsDead)
                        {
                            session.Character.TalentArenaBattle.RoundWin++;
                        }
                        else if (session.Character.TalentArenaBattle.IsDead)
                        {
                            opponent.Character.TalentArenaBattle.RoundWin++;
                        }

                        if (session.Character.TalentArenaBattle.RoundWin == 4)
                        {
                            GenerateWin(session);
                            GenerateLose(opponent);
                            session.Character.TalentArenaBattle.Round = 0;
                        }
                        else if (opponent.Character.TalentArenaBattle.RoundWin == 4)
                        {
                            GenerateWin(opponent);
                            GenerateLose(session);
                            session.Character.TalentArenaBattle.Round = 0;
                        }

                        session.Character.TalentArenaBattle.Round++;
                        TeleportToDeadLobby(session, opponent, true);
                        break;

                    case 2:
                        if (opponent.Character.TalentArenaBattle.IsDead)
                        {
                            GenerateWin(session);
                            GenerateLose(opponent);
                            session.Character.TalentArenaBattle.Round = 0;
                        }
                        else if (session.Character.TalentArenaBattle.IsDead)
                        {
                            GenerateWin(opponent);
                            GenerateLose(session);
                            session.Character.TalentArenaBattle.Round = 0;
                        }
                        break;
                }
            }
            else
            {
                switch (session.Character.TalentArenaBattle.Round)
                {
                    case 0:
                    case 1:
                        if (opponent.Character.TalentArenaBattle.IsDead)
                        {
                            session.Character.TalentArenaBattle.RoundWin++;
                        }
                        else if (session.Character.TalentArenaBattle.IsDead)
                        {
                            opponent.Character.TalentArenaBattle.RoundWin++;
                        }

                        session.Character.TalentArenaBattle.Round++;
                        TeleportToDeadLobby(session, opponent);
                        break;

                    case 2:
                        if (opponent.Character.TalentArenaBattle.IsDead)
                        {
                            session.Character.TalentArenaBattle.RoundWin++;
                        }
                        else if (session.Character.TalentArenaBattle.IsDead)
                        {
                            opponent.Character.TalentArenaBattle.RoundWin++;
                        }

                        if (session.Character.TalentArenaBattle.RoundWin == 3)
                        {
                            GenerateWin(session);
                            GenerateLose(opponent);
                            session.Character.TalentArenaBattle.Round = 0;
                        }
                        else if (opponent.Character.TalentArenaBattle.RoundWin == 3)
                        {
                            GenerateWin(opponent);
                            GenerateLose(session);
                            session.Character.TalentArenaBattle.Round = 0;
                        }
                        session.Character.TalentArenaBattle.Round++;
                        TeleportToDeadLobby(session, opponent);
                        break;

                    case 3:
                        if (opponent.Character.TalentArenaBattle.IsDead)
                        {
                            session.Character.TalentArenaBattle.RoundWin++;
                        }
                        else if (session.Character.TalentArenaBattle.IsDead)
                        {
                            opponent.Character.TalentArenaBattle.RoundWin++;
                        }

                        if (session.Character.TalentArenaBattle.RoundWin == 4)
                        {
                            GenerateWin(session);
                            GenerateLose(opponent);
                            session.Character.TalentArenaBattle.Round = 0;
                        }
                        else if (opponent.Character.TalentArenaBattle.RoundWin == 4)
                        {
                            GenerateWin(opponent);
                            GenerateLose(session);
                            session.Character.TalentArenaBattle.Round = 0;
                        }

                        if (session.Character.TalentArenaBattle.RoundWin == opponent.Character.TalentArenaBattle.RoundWin && session.Character.TalentArenaBattle.RoundWin == 2)
                        {
                            session.Character.TalentArenaBattle.Round = 0;
                            session.Character.TalentArenaBattle.TieBreaker = true;
                        }

                        TeleportToDeadLobby(session, opponent);
                        break;
                    case 4:
                        break;
                }
            }

            return true;
        }
    }
}
