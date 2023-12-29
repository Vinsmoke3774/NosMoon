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
using System.Threading;

namespace OpenNos.GameObject.Event.DEATHMATCH
{
    public class DeathMatch
    {
        private static void RemoveAllPets(ClientSession ses)
        {
            foreach (var mateTeam in ses.Character.Mates?.Where(s => s.IsTeamMember))
            {
                if (mateTeam == null) continue;
                mateTeam.RemoveTeamMember(true);
            }
        }

        private static void InitalizeVariables(List<ClientSession> sessions)
        {
            foreach (var session in sessions)
            {
                session.Character.LastMapId = session.Character.MapId;
                session.Character.LastMapX = session.Character.MapX;
                session.Character.LastMapY = session.Character.MapY;
            }
        }

        private static void EndGame(List<ClientSession> sessions)
        {
            foreach (var session in sessions)
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
                    session.CurrentMapInstance.Broadcast(session.Character.GenerateRevive());
                });
            }
        }

        private static void GenerateRewards(List<ClientSession> sessions)
        {
            var ranking = sessions.Select(s => s.Character.DeathMatchMember).OrderByDescending(s => s.Kills).Take(10);

            var first = ServerManager.Instance.GetSessionByCharacterName(ranking.ToList()[0].Name);
            first.Character.Inventory.AddNewToInventory(2800, 5);
            first.Character.GetReputation(10000);

            var second = ServerManager.Instance.GetSessionByCharacterName(ranking.ToList()[1].Name);
            second.Character.Inventory.AddNewToInventory(2800, 4);
            second.Character.GetReputation(8000);

            var third = ServerManager.Instance.GetSessionByCharacterName(ranking.ToList()[2].Name);
            third.Character.Inventory.AddNewToInventory(2800, 3);
            third.Character.GetReputation(7000);

            var fourth = ServerManager.Instance.GetSessionByCharacterName(ranking.ToList()[3].Name);
            fourth.Character.Inventory.AddNewToInventory(2800, 2);
            fourth.Character.GetReputation(6000);

            var five = ServerManager.Instance.GetSessionByCharacterName(ranking.ToList()[4].Name);
            five.Character.Inventory.AddNewToInventory(2800, 1);
            five.Character.GetReputation(5000);

            var sixth = ServerManager.Instance.GetSessionByCharacterName(ranking.ToList()[5].Name);
            sixth.Character.Inventory.AddNewToInventory(2800, 1);

            var seventh = ServerManager.Instance.GetSessionByCharacterName(ranking.ToList()[6].Name);
            seventh.Character.Inventory.AddNewToInventory(2800, 1);

            var eighth = ServerManager.Instance.GetSessionByCharacterName(ranking.ToList()[7].Name);
            eighth.Character.Inventory.AddNewToInventory(2800, 1);

            var ninth = ServerManager.Instance.GetSessionByCharacterName(ranking.ToList()[8].Name);
            ninth.Character.Inventory.AddNewToInventory(2800, 1);

            var tenth = ServerManager.Instance.GetSessionByCharacterName(ranking.ToList()[9].Name);
            tenth.Character.Inventory.AddNewToInventory(2800, 1);

            foreach (var session in sessions)
            {
                session.SendPacket(session.Character.GenerateFd());
                session.SendPacket(session.Character.GenerateGold());
                session.Character.Hp = (int)session.Character.HPLoad();
                session.Character.Mp = (int)session.Character.MPLoad();
                session.SendPacket(session.Character.GenerateStat());
                session.Character.IsFrozen = false;
                session.Character.DeathMatchMember = new();
            }

            Observable.Timer(TimeSpan.FromSeconds(2)).SafeSubscribe(x =>
            {
                EndGame(sessions);
            });
        }

        public static void TeleportToLobby(List<ClientSession> sessions)
        {
            sessions = sessions.Shuffle();
            InitalizeVariables(sessions);

            foreach (var session in sessions)
            {
                session.Character.DeathMatchMember.Name = session.Character.Name;
                session.Character.DeathMatchMember.Kills = 0;

                if (session.Character.IsVehicled) session.Character.RemoveVehicle();

                RemoveAllPets(session);

                session.CurrentMapInstance.Broadcast(UserInterfaceHelper.GenerateBSInfo(2, 12, 0, 0));
            }

            Dictionary<short, int> possibleMapId = new();

            // Cave of Ghosts
            possibleMapId.Add(0, 100);

            // Shanera Dungeon
            possibleMapId.Add(1, 30);

            var randomNumber = new Random().Next(0, 2);
            var randomMap = possibleMapId[(short)randomNumber];

            MapInstance map = ServerManager.GenerateMapInstance((short)randomMap, MapInstanceType.DeathMatch, new InstanceBag(), isScriptedInstance: true);

            switch (randomMap)
            {
                case 100:
                    sessions[0].Character.MapX = 6;
                    sessions[0].Character.MapY = 82;
                    sessions[0].Character.Direction = 5;

                    sessions[1].Character.MapX = 79;
                    sessions[1].Character.MapY = 80;
                    sessions[1].Character.Direction = 4;

                    sessions[2].Character.MapX = 85;
                    sessions[2].Character.MapY = 44;
                    sessions[2].Character.Direction = 3;

                    sessions[3].Character.MapX = 82;
                    sessions[3].Character.MapY = 6;
                    sessions[3].Character.Direction = 7;

                    sessions[4].Character.MapX = 9;
                    sessions[4].Character.MapY = 10;
                    sessions[4].Character.Direction = 6;

                    sessions[5].Character.MapX = 30;
                    sessions[5].Character.MapY = 49;
                    sessions[5].Character.Direction = 6;

                    sessions[6].Character.MapX = 57;
                    sessions[6].Character.MapY = 59;
                    sessions[6].Character.Direction = 6;

                    sessions[7].Character.MapX = 64;
                    sessions[7].Character.MapY = 26;
                    sessions[7].Character.Direction = 1;

                    sessions[8].Character.MapX = 60;
                    sessions[8].Character.MapY = 59;
                    sessions[8].Character.Direction = 0;

                    sessions[9].Character.MapX = 41;
                    sessions[9].Character.MapY = 34;
                    sessions[9].Character.Direction = 5;
                    break;

                case 30:
                    sessions[0].Character.MapX = 58;
                    sessions[0].Character.MapY = 1;
                    sessions[0].Character.Direction = 2;

                    sessions[1].Character.MapX = 112;
                    sessions[1].Character.MapY = 26;
                    sessions[1].Character.Direction = 3;

                    sessions[2].Character.MapX = 4;
                    sessions[2].Character.MapY = 26;
                    sessions[2].Character.Direction = 1;

                    sessions[3].Character.MapX = 5;
                    sessions[3].Character.MapY = 97;
                    sessions[3].Character.Direction = 1;

                    sessions[4].Character.MapX = 59;
                    sessions[4].Character.MapY = 116;
                    sessions[4].Character.Direction = 0;

                    sessions[5].Character.MapX = 112;
                    sessions[5].Character.MapY = 97;
                    sessions[5].Character.Direction = 3;

                    sessions[6].Character.MapX = 113;
                    sessions[6].Character.MapY = 60;
                    sessions[6].Character.Direction = 3;

                    sessions[7].Character.MapX = 2;
                    sessions[7].Character.MapY = 62;
                    sessions[7].Character.Direction = 62;

                    sessions[8].Character.MapX = 73;
                    sessions[8].Character.MapY = 45;
                    sessions[8].Character.Direction = 1;

                    sessions[9].Character.MapX = 43;
                    sessions[9].Character.MapY = 43;
                    sessions[9].Character.Direction = 3;
                    break;
            }

            foreach (var session in sessions)
            {
                ServerManager.Instance.ChangeMapInstance(session.Character.CharacterId, map.MapInstanceId, session.Character.MapX, session.Character.MapY, dir: session.Character.Direction);

                session.Character.NoAttack = true;
                session.Character.NoMove = true;
                session.SendPacket(session.Character.GenerateCond());

                session.Character.IsInFightZone = true;
                session.CurrentMapInstance.IsPVP = false;

                session.Character.NoAttack = true;
                session.Character.NoMove = true;

                List<BuffType> bufftodisable = new() { BuffType.All };

                session.Character.DisableBuffs(bufftodisable);
                session.Character.Hp = (int)session.Character.HPLoad();
                session.Character.Mp = (int)session.Character.MPLoad();
                session.SendPacket(session.Character.GenerateStat());

                if (session.Character.UseSp)
                {
                    session.Character.SkillsSp.ForEach(s => s.LastUse = DateTime.Now.AddDays(-1));
                }

                session.SendPacket(session.Character.GenerateSki());
                session.SendPackets(session.Character.GenerateQuicklist());

                session.Character.DeathMatchMember.IsDead = false;
            }

            sessions[0].Character.MapInstance.Clock.TotalSecondsAmount = 300;
            sessions[0].Character.MapInstance.Clock.SecondsRemaining = 300;

            sessions[0].Character.MapInstance.Clock.StartClock();
            sessions[0].CurrentMapInstance.Broadcast(sessions[0].Character.MapInstance.Clock.GetClock());

            Observable.Timer(TimeSpan.FromSeconds(30)).SafeSubscribe(s =>
            {
                TeleportInFightZone(sessions);
            });
        }

        private static void TeleportInFightZone(List<ClientSession> sessions)
        {
            foreach (var session in sessions)
            {
                session.Character.IsInFightZone = false;
                session.Character.NoAttack = false;
                session.Character.NoMove = false;
                session.SendPacket(session.Character.GenerateCond());

                // Enable PVP
                session.CurrentMapInstance.IsPVP = true;
            }

            sessions[0].Character.MapInstance.Clock.TotalSecondsAmount = 12000;
            sessions[0].Character.MapInstance.Clock.SecondsRemaining = 12000;

            Observable.Timer(TimeSpan.FromSeconds(12000)).SafeSubscribe(s =>
            {
                sessions[0].Character.MapInstance.Clock.StopClock();
                sessions[0].CurrentMapInstance.Broadcast(sessions[0].Character.MapInstance.Clock.GetClock());
            });

            sessions[0].Character.MapInstance.Clock.StartClock();
            sessions[0].CurrentMapInstance.Broadcast(sessions[0].Character.MapInstance.Clock.GetClock());

            while (!FightZoneAction(sessions))
            {
                Thread.Sleep(1000);
            }
        }

        private static bool FightZoneAction(List<ClientSession> sessions)
        {
            if (sessions[0].Character.MapInstance.Clock.SecondsRemaining <= 0)
            {
                GenerateRewards(sessions);
            }

            if (sessions.FirstOrDefault(s => s.Character.DeathMatchMember.Kills >= 30) == default) return false; // We don't have anything to do there as no one is dead yet

            GenerateRewards(sessions);

            return true;
        }
    }
}
