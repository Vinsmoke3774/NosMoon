using OpenNos.Core;
using OpenNos.Domain;
using OpenNos.GameObject.Extension.CharacterExtensions;
using OpenNos.GameObject.Networking;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using NosByte.Shared;
using OpenNos.GameObject.Helpers;
using System.Threading;
using System.Linq;
using OpenNos.Data;

namespace OpenNos.GameObject.Event.ONEVERSUSONE
{
    internal static class OneVersusOneEvent
    {
        internal static void Matchmaking(ClientSession session)
        {
            IObservable<long> observable = Observable.Interval(TimeSpan.FromSeconds(1));

            session.SendPacket(UserInterfaceHelper.GenerateBSInfo(0, 9, session.Character.DefaultTimer, 1));

            void action(OneVersusOneMember member)
            {
                session.Character.DefaultTimer--;

                // Timer
                if (session.Character.DefaultTimer <= 0)
                {
                    session.SendPacket(UserInterfaceHelper.GenerateBSInfo(2, 9, 0, 0));
                    session.Character.ArenaDisposable?.Dispose();
                }

                if (session.Character.DefaultTimer == 119) session.SendPacket(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("SEARCH_RIVAL_ONE_VERSUS_ONE"), 10));

                if (session.Character.LastDefence.AddSeconds(2) > DateTime.Now)
                {
                    session.SendPacket(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("ONE_VERSUS_ONE_LAUNCH_CANCELED_DUE_TO_FIGHT"), 10));
                    session.Character.ArenaDisposable.Dispose();
                    return;
                }

                if (session.Character.DefaultTimer < 0)
                {
                    session.SendPacket(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("ONE_VERSUS_ONE_OPPONENT_NOT_FOUND"), 10));
                    return;
                }

                if (member == null) return;

                InitalizeVariables(member, session);
                TeleportToLobby(session, member.Session);

                ServerManager.Instance.OneVersusOneMembers.Remove(member);
                ServerManager.Instance.OneVersusOneMembers.Remove(ServerManager.Instance.OneVersusOneMembers.FirstOrDefault(s => s.Session.Character.CharacterId == session.Character.CharacterId));
            }

            session.Character.ArenaDisposable = observable.SafeSubscribe(x =>
            {
                var opponent = ServerManager.Instance.OneVersusOneMembers.FirstOrDefault(s => s.Session.Character.CharacterId != session.Character.CharacterId);
                action(opponent);
            });
        }

        private static void RemoveAllPets(ClientSession ses)
        {
            foreach (var mateTeam in ses.Character.Mates?.Where(s => s.IsTeamMember))
            {
                if (mateTeam == null) continue;
                mateTeam.RemoveTeamMember(true);
            }
        }

        private static void InitalizeVariables(OneVersusOneMember member, ClientSession session)
        {
            session.Character.OneVersusOneBattle.Enemy = member.Session;
            member.Session.Character.OneVersusOneBattle.Enemy = session;
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
            session.Character.Inventory.AddNewToInventory(20002, 1);
            session.Character.Inventory.AddNewToInventory(2800, 1);
            session.Character.GetReputation(1000);
            session.SendPacket(session.Character.GenerateFd());
            session.Character.OneVersusOneBattle = new();

            Observable.Timer(TimeSpan.FromSeconds(2)).SafeSubscribe(x =>
            {
                EndGame(session);
            });
        }

        private static void GenerateLose(ClientSession session)
        {
            session.Character.Inventory.AddNewToInventory(2801, 1);
            session.Character.OneVersusOneBattle = new();

            Observable.Timer(TimeSpan.FromSeconds(2)).SafeSubscribe(x =>
            {
                EndGame(session);
            });
        }

        private static void TeleportToLobby(ClientSession session, ClientSession opponent)
        {
            if (session == null || opponent == null) return;

            if (session.Character.IsVehicled) session.Character.RemoveVehicle();

            if (opponent.Character.IsVehicled) opponent.Character.RemoveVehicle();

            RemoveAllPets(session);
            RemoveAllPets(opponent);

            session.Character.ArenaDisposable.Dispose();
            opponent.Character.ArenaDisposable.Dispose();
            session.CurrentMapInstance.Broadcast(UserInterfaceHelper.GenerateBSInfo(2, 12, 0, 0));

            Dictionary<short, Tuple<int, int, int, int, int>> possibleMapId = new();

            // Death Valley
            possibleMapId.Add(0, Tuple.Create(5100, 14, 27, 0, 1));
            possibleMapId.Add(1, Tuple.Create(5100, 14, 1, 2, 0));

            // Spiral Abyss
            possibleMapId.Add(2, Tuple.Create(5005, 32, 17, 3, 3));
            possibleMapId.Add(3, Tuple.Create(5005, 2, 17, 1, 2));

            // Wizard Tower Interior
            possibleMapId.Add(4, Tuple.Create(72, 21, 41, 0, 5));
            possibleMapId.Add(5, Tuple.Create(72, 21, 3, 2, 4));

            // Wizard Tower Exterior
            possibleMapId.Add(6, Tuple.Create(75, 2, 11, 1, 7));
            possibleMapId.Add(7, Tuple.Create(75, 21, 11, 3, 6));

            // Bridge Cave
            possibleMapId.Add(8, Tuple.Create(91, 5, 9, 1, 9));
            possibleMapId.Add(9, Tuple.Create(91, 42, 9, 3, 8));

            // Green Zone
            possibleMapId.Add(10, Tuple.Create(95, 25, 29, 4, 11));
            possibleMapId.Add(11, Tuple.Create(95, 12, 7, 6, 10));

            var randomNumber = new Random().Next(0, 11);
            var randomMap = possibleMapId[(short)randomNumber];

            MapInstance map = ServerManager.GenerateMapInstance((short)randomMap.Item1, MapInstanceType.OneVersusOne, new InstanceBag(), isScriptedInstance: true);

            short otherIndex = (short)randomMap.Item5;
            var otherPos = possibleMapId[otherIndex];
            ServerManager.Instance.ChangeMapInstance(session.Character.CharacterId, map.MapInstanceId, randomMap.Item2, randomMap.Item3, dir: randomMap.Item4);
            ServerManager.Instance.ChangeMapInstance(opponent.Character.CharacterId, map.MapInstanceId, otherPos.Item2, otherPos.Item3, dir: otherPos.Item4);

            session.Character.MapInstance.Clock.TotalSecondsAmount = 300;
            session.Character.MapInstance.Clock.SecondsRemaining = 300;

            Observable.Timer(TimeSpan.FromSeconds(30)).SafeSubscribe(s => 
            {
                TeleportInFightZone(session, opponent);
                session.Character.MapInstance.Clock.StopClock();
                session.SendPacket(session.Character.MapInstance.Clock.GetClock());
                opponent.SendPacket(session.Character.MapInstance.Clock.GetClock());
            });

            session.Character.MapInstance.Clock.StartClock();
            session.SendPacket(session.Character.MapInstance.Clock.GetClock());
            opponent.SendPacket(session.Character.MapInstance.Clock.GetClock());

            SetLobbyValues(session, opponent, true, false);
            session.CurrentMapInstance.IsPVP = false;

            session.Character.NoAttack = true;
            session.Character.NoMove = true;
            opponent.Character.NoAttack = true;
            opponent.Character.NoMove = true;

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

            session.Character.OneVersusOneBattle.IsDead = false;
            opponent.Character.OneVersusOneBattle.IsDead = false;
        }

        private static void SetLobbyValues(ClientSession session, ClientSession opponent, bool lobbyValue, bool fightZoneValue)
        {
            session.Character.IsInFightZone = fightZoneValue;
            opponent.Character.IsInFightZone = fightZoneValue;
        }

        private static void TeleportInFightZone(ClientSession session, ClientSession opponent)
        {
            SetLobbyValues(session, opponent, false, true);
            session.Character.NoAttack = false;
            session.Character.NoMove = false;
            opponent.Character.NoAttack = false;
            opponent.Character.NoMove = false;
            opponent.SendPacket(opponent.Character.GenerateCond());
            session.SendPacket(session.Character.GenerateCond());

            // Enable PVP
            session.CurrentMapInstance.IsPVP = true;

            session.Character.MapInstance.Clock.TotalSecondsAmount = 6000;
            session.Character.MapInstance.Clock.SecondsRemaining = 6000;

            Observable.Timer(TimeSpan.FromSeconds(600)).SafeSubscribe(s =>
            {
                session.Character.MapInstance.Clock.StopClock();
                session.SendPacket(session.Character.MapInstance.Clock.GetClock());
                opponent.SendPacket(session.Character.MapInstance.Clock.GetClock());
            });

            session.Character.MapInstance.Clock.StartClock();
            session.SendPacket(session.Character.MapInstance.Clock.GetClock());
            opponent.SendPacket(session.Character.MapInstance.Clock.GetClock());

            while (!FightZoneAction(session, opponent))
            {
                Thread.Sleep(1000);
            }
        }

        private static bool FightZoneAction(ClientSession session, ClientSession opponent)
        {
            if (!session.IsDisposing)
            {
                if (session.CurrentMapInstance.Sessions.Count() == 1)
                {
                    PenaltyLogDTO log = new PenaltyLogDTO
                    {
                        AccountId = opponent.Account.AccountId,
                        Reason = "Leaving 1V1",
                        Penalty = PenaltyType.OneVersusOneBan,
                        DateStart = DateTime.Now,
                        DateEnd = DateTime.Now.AddMinutes(10),
                        AdminName = "NosMoon System",
                    };
                    Character.InsertOrUpdatePenalty(log);

                    session.Character.LastMapId = session.Character.MapId;
                    session.Character.LastMapX = session.Character.MapX;
                    session.Character.LastMapY = session.Character.MapY;

                    var mapInstance = ServerManager.GetMapInstanceByMapId(session.Character.LastMapId);
                    ServerManager.Instance.ChangeMapInstance(session.Character.CharacterId, mapInstance.MapInstanceId, session.Character.LastMapX, session.Character.LastMapY);

                    return true;
                }
            }
            else if (!opponent.IsDisposing)
            {
                if (opponent.CurrentMapInstance.Sessions.Count() == 1)
                {
                    PenaltyLogDTO log = new PenaltyLogDTO
                    {
                        AccountId = session.Account.AccountId,
                        Reason = "Leaving 1V1",
                        Penalty = PenaltyType.OneVersusOneBan,
                        DateStart = DateTime.Now,
                        DateEnd = DateTime.Now.AddMinutes(10),
                        AdminName = "NosMoon System",
                    };
                    Character.InsertOrUpdatePenalty(log);

                    opponent.Character.LastMapId = opponent.Character.MapId;
                    opponent.Character.LastMapX = opponent.Character.MapX;
                    opponent.Character.LastMapY = opponent.Character.MapY;

                    var mapInstance = ServerManager.GetMapInstanceByMapId(opponent.Character.LastMapId);
                    ServerManager.Instance.ChangeMapInstance(opponent.Character.CharacterId, mapInstance.MapInstanceId, opponent.Character.LastMapX, opponent.Character.LastMapY);

                    return true;
                }
            }

            if (session.Character.MapInstance.Clock.SecondsRemaining <= 0)
            {
                if (session.Character.Hp > opponent.Character.Hp)
                {
                    GenerateLose(opponent);
                    GenerateWin(session);
                }
                else if (session.Character.Hp < opponent.Character.Hp)
                {
                    GenerateLose(session);
                    GenerateWin(opponent);
                }

                return true;
            }

            if (!session.Character.OneVersusOneBattle.IsDead && !opponent.Character.OneVersusOneBattle.IsDead) return false; // We don't have anything to do there as no one is dead yet

            if (session.Character.OneVersusOneBattle.IsDead)
            {
                GenerateLose(session);
                GenerateWin(opponent);
            }

            if (opponent.Character.OneVersusOneBattle.IsDead)
            {
                GenerateLose(opponent);
                GenerateWin(session);
            }

            return true;
        }
    }
}
