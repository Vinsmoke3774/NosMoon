using NosByte.Shared;
using OpenNos.Core.Extensions;
using OpenNos.Domain;
using OpenNos.GameObject.Extension.CharacterExtensions;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;
using OpenNos.GameObject.TwoVersusTwo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;

namespace OpenNos.GameObject.Event.TwoVersusTwoEvent
{
    public class TwoVersusTwoEvent
    {

        #region Methods

        public static void CheckAll(List<ClientSession> ses)
        {
            foreach (var ss in ses)
            {
                ss.Character.LastMapId = ss.Character.MapId;
                ss.Character.LastMapX = ss.Character.MapX;
                ss.Character.LastMapY = ss.Character.MapY;
            }

            Dictionary<short, int> possibleMapId = new();

            // Aster Temple
            possibleMapId.Add(0, 199);

            // Cosy Garden
            possibleMapId.Add(1, 2566);

            var randomNumber = new Random().Next(0, possibleMapId.Count);
            var randomMap = possibleMapId[(short)randomNumber];

            var map = ServerManager.GenerateMapInstance((short)randomMap, MapInstanceType.TwoVersusTwo, new InstanceBag(), isScriptedInstance: true);
            ServerManager.Instance.ChangeMapInstance(ses[0].Character.CharacterId, map.MapInstanceId, 12, 24, dir: 1);
            ServerManager.Instance.ChangeMapInstance(ses[1].Character.CharacterId, map.MapInstanceId, 30, 16, dir: 1);
            ServerManager.Instance.ChangeMapInstance(ses[2].Character.CharacterId, map.MapInstanceId, 12, 16, dir: 3);
            ServerManager.Instance.ChangeMapInstance(ses[3].Character.CharacterId, map.MapInstanceId, 30, 24, dir: 3);

            foreach (var ss in ses)
            {
                RemoveAllPets(ss);
                ss.SendPacket(UserInterfaceHelper.GenerateBSInfo(2, 7, 0, 0));
            }
            new TwoVersusTwoThread().RunEvent(ses);
        }

        public static void RemoveAllPets(ClientSession ses)
        {
            foreach (var mateTeam in ses.Character.Mates?.Where(s => s.IsTeamMember))
            {
                if (mateTeam == null) continue;
                mateTeam.RemoveTeamMember(true);
            }
        }

        #endregion

        #region Classes

        public class TwoVersusTwoThread
        {
            #region Methods

            public void CreateGroup(List<ClientSession> session)
            {
                var group = new Group
                {
                    GroupType = GroupType.TwoVersusTwo1
                };
                ServerManager.Instance.AddGroup(group);
                var group2 = new Group
                {
                    GroupType = GroupType.TwoVersusTwo2
                };
                ServerManager.Instance.AddGroup(group2);
                List<ClientSession> firstTeam = new();
                List<ClientSession> secondTeam = new();

                if (session.All(s => s.Character.Group == null))
                {
                    session = session.Shuffle();
                }

                for (int i = 0; i < 4; i++)
                {
                    if (TwoVersusTwoBattleManager.AreNotInMap(session[i]))
                    {
                        break;
                    }

                    session[i].Character.Group?.LeaveGroup(session[i]);

                    if (i == 0 || i == 1)
                    {
                        switch (i)
                        {
                            case 0 when session[i].Character.MapId == 199:
                                session[i].Character.PositionX = 5;
                                session[i].Character.PositionY = 29;
                                break;

                            case 1 when session[i].Character.MapId == 199:
                                session[i].Character.PositionX = 5;
                                session[i].Character.PositionY = 11;
                                break;

                            case 0 when session[i].Character.MapId == 2566:
                                session[i].Character.PositionX = 6;
                                session[i].Character.PositionY = 27;
                                break;

                            case 1 when session[i].Character.MapId == 2566:
                                session[i].Character.PositionX = 6;
                                session[i].Character.PositionY = 11;
                                break;
                        }

                        session[i].CurrentMapInstance.Broadcast(session[i], session[i].Character.GenerateTp());
                        firstTeam.Add(session[i]);
                        group.JoinGroup(session[i], true);
                    }
                    else if (i == 2 || i == 3)
                    {
                        switch (i)
                        {
                            case 2 when session[i].Character.MapId == 199:
                                session[i].Character.PositionX = 43;
                                session[i].Character.PositionY = 26;
                                break;

                            case 3 when session[i].Character.MapId == 199:
                                session[i].Character.PositionX = 43;
                                session[i].Character.PositionY = 17;
                                break;

                            case 2 when session[i].Character.MapId == 2566:
                                session[i].Character.PositionX = 27;
                                session[i].Character.PositionY = 11;
                                break;

                            case 3 when session[i].Character.MapId == 2566:
                                session[i].Character.PositionX = 27;
                                session[i].Character.PositionY = 26;
                                break;
                        }

                        session[i].CurrentMapInstance.Broadcast(session[i], session[i].Character.GenerateTp());
                        secondTeam.Add(session[i]);
                        group2.JoinGroup(session[i], true);
                    }
                    ServerManager.Instance.UpdateGroup(session[i].Character.CharacterId);
                }

                var blueTeam = new TwoVersusTwoBattleTeam(firstTeam, TwoVersusTwoTeamType.Blue, null);
                var redTeam = new TwoVersusTwoBattleTeam(secondTeam, TwoVersusTwoTeamType.Red, blueTeam);
                blueTeam.SecondTeam = redTeam;
                ServerManager.Instance.TwoVersusTwoBattleMembers.Add(blueTeam);
                ServerManager.Instance.TwoVersusTwoBattleMembers.Add(redTeam);
            }

            public void RunEvent(List<ClientSession> session)
            {
                CreateGroup(session);
                foreach (ClientSession sess in session)
                {
                    sess.Character.DisableBuffs(BuffType.All);
                    sess.Character.Hp = (int)sess.Character.HPLoad();
                    sess.Character.Mp = (int)sess.Character.MPLoad();
                    sess.SendPacket(sess.Character.GenerateStat());

                    sess.Character.MapInstance.Clock.TotalSecondsAmount = 300;
                    sess.Character.MapInstance.Clock.SecondsRemaining = 300;
                    sess.Character.MapInstance.Clock.StartClock();
                    sess.SendPacket(sess.Character.MapInstance.Clock.GetClock());

                    sess.Character.NoMove = true;
                    sess.Character.NoAttack = true;
                    sess.SendPacket(sess.Character.GenerateCond());
                }

                Observable.Timer(TimeSpan.FromSeconds(30)).SafeSubscribe(o =>
                {
                    session.First().CurrentMapInstance.IsPVP = true;

                    foreach (var ss in session)
                    {
                        ss.Character.NoMove = false;
                        ss.Character.NoAttack = false;
                        ss.SendPacket(ss.Character.GenerateCond());

                        ss.Character.MapInstance.Clock.TotalSecondsAmount = 6000;
                        ss.Character.MapInstance.Clock.SecondsRemaining = 6000;
                        ss.Character.MapInstance.Clock.StartClock();
                        ss.SendPacket(ss.Character.MapInstance.Clock.GetClock());
                    }
                });

                Observable.Timer(TimeSpan.FromSeconds(30)).SafeSubscribe(o =>
                {
                    while (!EndGame(session))
                    {
                        Thread.Sleep(TimeSpan.FromSeconds(1));
                    }
                });
            }

            private void Teleport(List<ClientSession> session)
            {
                Observable.Timer(TimeSpan.FromSeconds(3)).SafeSubscribe(s =>
                {
                    foreach (var ss in session)
                    {
                        ss.Character.MapId = ss.Character.LastMapId;
                        ss.Character.MapX = ss.Character.LastMapX;
                        ss.Character.MapY = ss.Character.LastMapY;
                        ss.Character.NoMove = false;
                        ss.Character.NoAttack = false;
                        ss.Character.IsFrozen = false;
                        ss.Character.Group?.LeaveGroup(ss);
                        ss.SendPacket(ss.Character.GenerateCond());
                        var sss = ServerManager.Instance.TwoVersusTwoBattleMembers.Find(s => s.Session.Contains(ss));
                        ServerManager.Instance.TwoVersusTwoBattleMembers.Remove(sss);
                        ServerManager.Instance.ChangeMap(ss.Character.CharacterId, ss.Character.MapId, ss.Character.MapX, ss.Character.MapY);
                    }
                });
            }

            private bool EndGame(List<ClientSession> sessions)
            {
                List<ClientSession> blue = new();
                List<ClientSession> red = new();

                foreach (var session in sessions)
                {
                    var ss = ServerManager.Instance.TwoVersusTwoBattleMembers.Find(s => s.Session.Contains(session));

                    if (ss == null) continue;


                    if (ss.TeamEntity == TwoVersusTwoTeamType.Blue)
                    {
                        blue.Add(session);
                    }

                    if (ss.TeamEntity == TwoVersusTwoTeamType.Red)
                    {
                        red.Add(session);
                    }
                }

                foreach (var dead in red.Where(s => s.Character.TwoVersusTwoBattle.IsDead))
                {
                    if (dead.Character.RedDead.All(s => s.Character.CharacterId != dead.Character.CharacterId))
                    {
                        foreach (var ss in red)
                        {
                            ss.Character.RedDead.Add(dead);
                        }
                    }
                }

                foreach (var dead in blue.Where(s => s.Character.TwoVersusTwoBattle.IsDead))
                {
                    if (dead.Character.BlueDead.All(s => s.Character.CharacterId != dead.Character.CharacterId))
                    {
                        foreach (var ss in blue)
                        {
                            ss.Character.BlueDead.Add(dead);
                        }
                    }
                }


                if (red.First().Character.RedDead.Count() >= 2)
                {
                    foreach (var ss in red)
                    {
                        ss.Character.Inventory.AddNewToInventory(2801, 1);
                        ss.Character.RedDead.Clear();
                        ss.Character.BlueDead.Clear();
                    }

                    foreach (var ss in blue)
                    {
                        ss.Character.Inventory.AddNewToInventory(20002, 1);
                        ss.Character.Inventory.AddNewToInventory(2800, 1);
                        ss.Character.RedDead.Clear();
                        ss.Character.BlueDead.Clear();
                    }

                    List<ClientSession> sss = new();
                    sss.AddRange(red);
                    sss.AddRange(blue);

                    Teleport(sss);

                    red.First().CurrentMapInstance.Broadcast(red.First().Character.GenerateSay("Blue team won, you'll be teleported in 3 seconds !", 10));
                    return true;
                }

                if (blue.First().Character.BlueDead.Count() >= 2)
                {
                    foreach (var ss in blue)
                    {
                        ss.Character.Inventory.AddNewToInventory(2801, 1);
                        ss.Character.RedDead.Clear();
                        ss.Character.BlueDead.Clear();
                    }

                    foreach (var ss in red)
                    {
                        ss.Character.Inventory.AddNewToInventory(20002, 1);
                        ss.Character.Inventory.AddNewToInventory(2800, 1);
                        ss.Character.RedDead.Clear();
                        ss.Character.BlueDead.Clear();
                    }

                    List<ClientSession> sss = new();
                    sss.AddRange(red);
                    sss.AddRange(blue);

                    Teleport(sss);

                    blue.First().CurrentMapInstance.Broadcast(blue.First().Character.GenerateSay("Red team won, you'll be teleported in 3 seconds !", 10));
                    return true;
                }

                if (red.First().Character.MapInstance.Clock.SecondsRemaining <= 0)
                {
                    if (red.First().Character.RedDead.Count() > blue.First().Character.BlueDead.Count())
                    {
                        foreach (var ss in red)
                        {
                            ss.Character.Inventory.AddNewToInventory(2801, 1);
                            ss.Character.RedDead.Clear();
                            ss.Character.BlueDead.Clear();
                        }

                        foreach (var ss in blue)
                        {
                            ss.Character.Inventory.AddNewToInventory(2800, 1);
                            ss.Character.RedDead.Clear();
                            ss.Character.BlueDead.Clear();
                        }

                        List<ClientSession> sss = new();
                        sss.AddRange(red);
                        sss.AddRange(blue);

                        Teleport(sss);

                        blue.First().CurrentMapInstance.Broadcast(blue.First().Character.GenerateSay("Blue team won, you'll be teleported in 3 seconds !", 10));
                        return true;
                    }
                    
                    if (red.First().Character.RedDead.Count() > blue.First().Character.BlueDead.Count())
                    {
                        foreach (var ss in blue)
                        {
                            ss.Character.Inventory.AddNewToInventory(2801, 1);
                            ss.Character.RedDead.Clear();
                            ss.Character.BlueDead.Clear();
                        }

                        foreach (var ss in red)
                        {
                            ss.Character.Inventory.AddNewToInventory(2800, 1);
                            ss.Character.RedDead.Clear();
                            ss.Character.BlueDead.Clear();
                        }

                        List<ClientSession> sss = new();
                        sss.AddRange(red);
                        sss.AddRange(blue);

                        Teleport(sss);

                        blue.First().CurrentMapInstance.Broadcast(blue.First().Character.GenerateSay("Red team won, you'll be teleported in 3 seconds !", 10));
                        return true;
                    }
                }

                return false;
            }
            #endregion
        }

        #endregion
    }
}