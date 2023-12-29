using NosByte.Shared;
using OpenNos.Core.Extensions;
using OpenNos.Domain;
using OpenNos.GameObject.Extension.CharacterExtensions;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace OpenNos.GameObject.Event.KINGOFTHEHILL
{
    public class KingOfTheHill
    {
        public static void Split()
        {
            int count = ServerManager.Instance.KingOfTheHillMembers.Count / 6;

            if (count > 1)
            {
                var members = ServerManager.Instance.KingOfTheHillMembers.ToList().Take(6);
                DetermineGroup(members.ToList());
                Console.WriteLine("member:" + members);
                foreach (var member in members)
                {
                    Console.WriteLine("member removed: " + members);
                    ServerManager.Instance.KingOfTheHillMembers.Remove(member);
                }
            }
            else
            {
                DetermineGroup(ServerManager.Instance.KingOfTheHillMembers.ToList());
                Console.WriteLine("member list: " + ServerManager.Instance.KingOfTheHillMembers.ToList());
            }
        }

        public static void DetermineGroup(List<KingOfTheHillMember> kingOfTheHillMembers) 
        {
            if (kingOfTheHillMembers.Count < 4)
            {
                foreach (var ss in kingOfTheHillMembers.Select(s => s.Session))
                    ss.SendPacket(UserInterfaceHelper.GenerateMsg("Not enough players (6 are required)", 0));
                return;
            }

            var map = ServerManager.GenerateMapInstance(248, MapInstanceType.KingOfTheHill, new(), isScriptedInstance: true);

            kingOfTheHillMembers = kingOfTheHillMembers.Shuffle();

            kingOfTheHillMembers[0].Session.Character.kingOfTheHill.IsKing = true;
            kingOfTheHillMembers[1].Session.Character.kingOfTheHill.IsQueen = true;

            foreach (var member in kingOfTheHillMembers)
            {
                member.Session.Character.kingOfTheHill.MapInstanceId = map.MapInstanceId;
                member.Session.SendPacket(UserInterfaceHelper.GenerateBSInfo(2, 27, 0, 0));
                member.Session.Character.kingOfTheHill.Points = 0;
                var nosTeamCount = kingOfTheHillMembers.Count(s => s.Session.Character.kingOfTheHill.TeamType == KingOfTheHillTeamType.NOS);
                var moonTeamCount = kingOfTheHillMembers.Count(s => s.Session.Character.kingOfTheHill.TeamType == KingOfTheHillTeamType.MOON);
                KingOfTheHillTeamType type = nosTeamCount > moonTeamCount ? KingOfTheHillTeamType.MOON : KingOfTheHillTeamType.NOS;

                if (!member.Session.Character.kingOfTheHill.IsQueen && !member.Session.Character.kingOfTheHill.IsKing)
                {
                    member.Session.Character.kingOfTheHill.TeamType = type;
                }
                else
                {
                    member.Session.Character.kingOfTheHill.TeamType = KingOfTheHillTeamType.NONE;
                }

                if (member.Session.Character.kingOfTheHill.IsKing)
                {
                    ServerManager.Instance.ChangeMapInstance(member.Session.Character.CharacterId, map.MapInstanceId, 150, 90, dir: 2);
                    member.Session.Character.AddBuff(new Buff(802, 99), null); // TODO : Worldie add real buff
                }
                else if (member.Session.Character.kingOfTheHill.IsQueen)
                {
                    ServerManager.Instance.ChangeMapInstance(member.Session.Character.CharacterId, map.MapInstanceId, 139, 90, dir: 2);
                    member.Session.Character.AddBuff(new Buff(802, 99), null); // TODO : Worldie add real buff
                }
                else
                {
                    if (member.Session.Character.kingOfTheHill.TeamType == KingOfTheHillTeamType.NOS)
                        ServerManager.Instance.ChangeMapInstance(member.Session.Character.CharacterId, map.MapInstanceId, 126, new Random().Next(132, 110), dir: 0);
                    else
                        ServerManager.Instance.ChangeMapInstance(member.Session.Character.CharacterId, map.MapInstanceId, 156, new Random().Next(132, 120), dir: 0);
                }

                if (member.Session.Character.kingOfTheHill.IsKing || member.Session.Character.kingOfTheHill.IsQueen)
                {
                    member.Session.Character.Size = 20;
                    member.Session.CurrentMapInstance.Broadcast(member.Session.Character.GenerateScal());
                }

                member.Session.Character.NoMove = true;
                member.Session.Character.NoAttack = true;
                member.Session.SendPacket(member.Session.Character.GenerateCond());
            }

            var member2 = kingOfTheHillMembers[0];
            member2.Session.Character.MapInstance.Clock.TotalSecondsAmount = 600;
            member2.Session.Character.MapInstance.Clock.SecondsRemaining = 600;
            member2.Session.Character.MapInstance.Clock.StartClock();
            member2.Session.CurrentMapInstance.Broadcast(member2.Session.Character.MapInstance.Clock.GetClock());

            Observable.Timer(TimeSpan.FromMinutes(1)).SafeSubscribe(s =>
            {
                StartEvent(kingOfTheHillMembers);
            });

            Say(kingOfTheHillMembers);
        }

        public static void StartEvent(List<KingOfTheHillMember> kingOfTheHillMembers)
        {
            foreach (var session in kingOfTheHillMembers.Select(s => s.Session))
            {
                session.Character.NoMove = false;
                session.Character.NoAttack = false;
                session.SendPacket(session.Character.GenerateCond());
            }

            var member2 = kingOfTheHillMembers[0];
            member2.Session.CurrentMapInstance.IsPVP = true;
            member2.Session.Character.MapInstance.Clock.TotalSecondsAmount = 12000;
            member2.Session.Character.MapInstance.Clock.SecondsRemaining = 12000;
            member2.Session.Character.MapInstance.Clock.StartClock();
            member2.Session.CurrentMapInstance.Broadcast(member2.Session.Character.MapInstance.Clock.GetClock());

            var disposable = Observable.Timer(TimeSpan.FromSeconds(1)).SafeSubscribe(s =>
            {
                CheckKingAndQueenZone(kingOfTheHillMembers);
            });
        }

        public static void CheckKingAndQueenZone(List<KingOfTheHillMember> kingOfTheHillMembers)
        {
            var king = kingOfTheHillMembers[0].Session.CurrentMapInstance.Sessions.First(s => s.Character.kingOfTheHill.IsKing && s.Character.kingOfTheHill.MapInstanceId == kingOfTheHillMembers[0].Session.CurrentMapInstance.MapInstanceId);
            var queen = kingOfTheHillMembers[0].Session.CurrentMapInstance.Sessions.First(s => s.Character.kingOfTheHill.IsQueen && s.Character.kingOfTheHill.MapInstanceId == kingOfTheHillMembers[0].Session.CurrentMapInstance.MapInstanceId);

            if (king == null || queen == null)
                return;

            List<Tuple<int, int>> square = new() { Tuple.Create(166, 187), Tuple.Create(123, 187), Tuple.Create(164, 233), Tuple.Create(124, 231) };

            if (!IsPointInSquare(square, Tuple.Create((int)king.Character.PositionX, (int)king.Character.PositionY)))
            {
                IDisposable obs1 = null;
                obs1 = Observable.Interval(TimeSpan.FromSeconds(1)).SafeSubscribe(s =>
                {
                    if (king?.Character == null)
                    {
                        obs1?.Dispose();
                        return;
                    }

                    if (!king.Character.IsFrozen)
                    {
                        obs1.Dispose();
                        return;
                    }

                    king.CurrentMapInstance?.Broadcast(king.Character?.GenerateEff(35));
                });

                king.Character.NoMove = true;
                king.Character.NoAttack = true;
                king.SendPacket(king.Character.GenerateCond());
                king.Character.IsFrozen = true;
                king.Character.kingOfTheHill.IsKing = false;

                var newKing = kingOfTheHillMembers.Where(s => s.Session.Character.Name != king.Character.Name).OrderBy(s => new Random().Next());
                if (newKing == null) return;
                var member = newKing.First();
                member.Session.Character.kingOfTheHill.IsKing = true;
                member.Session.Character.Size = 20;
                member.Session.CurrentMapInstance.Broadcast(member.Session.Character.GenerateScal());
                member.Session.Character.PositionX = 150;//
                member.Session.Character.PositionY = 90;
                member.Session.CurrentMapInstance.Broadcast(member.Session.Character.GenerateTp());
                // Worldie add buff for new king
                member.Session.CurrentMapInstance.Broadcast(UserInterfaceHelper.GenerateMsg($"{king.Character.Name} abandoned the hill, {member.Session.Character.Name} has now been appointed King !", 0));

                Observable.Timer(TimeSpan.FromSeconds(5)).SafeSubscribe(s =>
                {
                    king.Character.NoMove = false;
                    king.Character.NoAttack = false;
                    king.SendPacket(king.Character.GenerateCond());
                    king.Character.IsFrozen = false;

                    var countTeamMoon = king.CurrentMapInstance.Sessions.Count(s => s.Character.kingOfTheHill.TeamType == KingOfTheHillTeamType.MOON);
                    var countTeamNos = king.CurrentMapInstance.Sessions.Count(s => s.Character.kingOfTheHill.TeamType == KingOfTheHillTeamType.NOS);
                    var teamType = countTeamMoon > countTeamNos ? KingOfTheHillTeamType.NOS : (KingOfTheHillTeamType)new Random().Next(0, 2);
                    king.Character.kingOfTheHill.TeamType = teamType;

                    if (teamType == KingOfTheHillTeamType.NOS)
                    {
                        king.Character.PositionX = 54;
                        king.Character.PositionY = (short)new Random().Next(139, 251);
                    }
                    else
                    {
                        king.Character.PositionX = 239;
                        king.Character.PositionY = (short)new Random().Next(147, 228);
                    }

                    king.Character.Size = 10;
                    king.CurrentMapInstance.Broadcast(king.Character.GenerateScal());
                    king.CurrentMapInstance.Broadcast(king.Character.GenerateTp());
                });
            }
            else
            {
                king.Character.kingOfTheHill.Points += 1;
            }

            if (!IsPointInSquare(square, Tuple.Create((int)queen.Character.PositionX, (int)queen.Character.PositionY)))
            {
                IDisposable obs1 = null;
                obs1 = Observable.Interval(TimeSpan.FromSeconds(1)).SafeSubscribe(s =>
                {
                    if (queen?.Character == null)
                    {
                        obs1?.Dispose();
                        return;
                    }

                    if (!queen.Character.IsFrozen)
                    {
                        obs1.Dispose();
                        return;
                    }

                    queen.CurrentMapInstance?.Broadcast(queen.Character?.GenerateEff(35));
                });

                queen.Character.NoMove = true;
                queen.Character.NoAttack = true;
                queen.SendPacket(queen.Character.GenerateCond());
                queen.Character.IsFrozen = true;
                queen.Character.kingOfTheHill.IsQueen = false;

                var newQueen = kingOfTheHillMembers.Where(s => s.Session.Character.Name != queen.Character.Name).OrderBy(s => new Random().Next());
                if (newQueen == null) return;
                var member = newQueen.First();
                member.Session.Character.kingOfTheHill.IsQueen = true;
                member.Session.Character.Size = 20;
                member.Session.CurrentMapInstance.Broadcast(member.Session.Character.GenerateScal());
                member.Session.Character.PositionX = 139;
                member.Session.Character.PositionY = 90;
                member.Session.CurrentMapInstance.Broadcast(member.Session.Character.GenerateTp());
                // Worldie add buff for new king
                member.Session.CurrentMapInstance.Broadcast(UserInterfaceHelper.GenerateMsg($"{queen.Character.Name} abandoned the hill, {member.Session.Character.Name} has now been appointed King !", 0));

                Observable.Timer(TimeSpan.FromSeconds(5)).SafeSubscribe(s =>
                {
                    queen.Character.NoMove = false;
                    queen.Character.NoAttack = false;
                    queen.SendPacket(queen.Character.GenerateCond());
                    queen.Character.IsFrozen = false;

                    var countTeamMoon = queen.CurrentMapInstance.Sessions.Count(s => s.Character.kingOfTheHill.TeamType == KingOfTheHillTeamType.MOON);
                    var countTeamNos = queen.CurrentMapInstance.Sessions.Count(s => s.Character.kingOfTheHill.TeamType == KingOfTheHillTeamType.NOS);
                    var teamType = countTeamMoon > countTeamNos ? KingOfTheHillTeamType.NOS : (KingOfTheHillTeamType)new Random().Next(0, 2);
                    queen.Character.kingOfTheHill.TeamType = teamType;

                    if (teamType == KingOfTheHillTeamType.NOS)
                    {
                        queen.Character.PositionX = 54;
                        queen.Character.PositionY = (short)new Random().Next(139, 251);
                    }
                    else
                    {
                        queen.Character.PositionX = 239;
                        queen.Character.PositionY = (short)new Random().Next(147, 228);
                    }

                    queen.Character.Size = 10;
                    queen.SendPacket(queen.Character.GenerateScal());
                    queen.CurrentMapInstance.Broadcast(queen.Character.GenerateTp());
                });
            }
            else
            {
                queen.Character.kingOfTheHill.Points += 1;
            }
        }

        private static bool IsPointInSquare(List<Tuple<int, int>> square, Tuple<int, int> playerPos)
        {

            float a = CalculExteriorProduct(square[0], square[1], playerPos);
            float b = CalculExteriorProduct(square[1], square[2], playerPos);
            float c = CalculExteriorProduct(square[2], square[3], playerPos);
            float d = CalculExteriorProduct(square[3], square[0], playerPos);
            return a > 0 && b > 0 && c > 0 && d > 0;
        }

        private static float CalculExteriorProduct(Tuple<int, int> a, Tuple<int, int> b, Tuple<int, int> player)
        {
            var vecab = (a.Item1 - b.Item1, a.Item2 - b.Item2);
            var vecpa = (a.Item1 - player.Item1, a.Item2 - player.Item2);
            return vecab.Item1 * vecpa.Item2 - vecpa.Item1 * vecab.Item2;
        }

        private static void Say(List<KingOfTheHillMember> kingOfTheHillMembers)
        {
            var king = kingOfTheHillMembers[0].Session.CurrentMapInstance.Sessions.First(s => s.Character.kingOfTheHill.IsKing && s.Character.kingOfTheHill.MapInstanceId == kingOfTheHillMembers[0].Session.CurrentMapInstance.MapInstanceId);
            var queen = kingOfTheHillMembers[0].Session.CurrentMapInstance.Sessions.First(s => s.Character.kingOfTheHill.IsQueen && s.Character.kingOfTheHill.MapInstanceId == kingOfTheHillMembers[0].Session.CurrentMapInstance.MapInstanceId);

            if (king == null || queen == null)
                return;

            king.CurrentMapInstance.Broadcast(UserInterfaceHelper.GenerateMsg($"{king.Character.Name} & {queen.Character.Name} has been appointed King and Queen.", 0));
        }

        private static void EndGame()
        {
            ServerManager.Instance.KingOfTheHillTimer = 300;
            ServerManager.Instance.KingOfTheHillMembers.Clear();
        }
    }
}
