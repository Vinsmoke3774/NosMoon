using NosByte.Packets.ClientPackets;
using NosByte.Shared;
using OpenNos.Core;
using OpenNos.Core.Actions;
using OpenNos.DAL;
using OpenNos.Data;
using OpenNos.Domain;
using OpenNos.GameObject.ActionHandles;
using OpenNos.GameObject.Event.TwoVersusTwoEvent;
using OpenNos.GameObject.Extension.CharacterExtensions;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace OpenNos.GameObject.Npc.NRunHandles.TwoVersusTwo
{
    [NRunHandler(NRunType.TwoVersusTwoRegister)]
    public class TwoVersusTwoRegisterHandler : SpecialHandlerBase, IGenericHandler<NRunPacket>
    {
        public TwoVersusTwoRegisterHandler(ClientSession session) : base(session)
        {
        }

        public void ValidateData(NRunPacket packet)
        {
            Execute(packet);
        }

        public void Execute(NRunPacket packet)
        {
            if (Session.Character.Group != null)
            {
                if (!Session.Character.Group.IsLeader(Session))
                {
                    Session.SendPacket($"info You're not the party master !");
                    return;
                }
            }

            if (ServerManager.Instance.ChannelId != 1)
            {
                Session.SendPacket($"info You can only register on the channel 1!");
                return;
            }

            if (Session.CurrentMapInstance.MapInstanceType != MapInstanceType.BaseMapInstance)
            {
                return;
            }

            if (Session.Character.LastSkillUse.AddSeconds(20) > DateTime.Now || Session.Character.LastDefence.AddSeconds(20) > DateTime.Now)
            {
                Session.SendPacket(UserInterfaceHelper.GenerateInfo(string.Format(Language.Instance.GetMessageFromKey("PLAYER_IN_BATTLE"), Session.Character.Name)));
                return;
            }


            if (Session.Character.LastDefence.AddSeconds(2) > DateTime.Now)
            {
                Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("CANT_LAUNCH_TWOVERSUSTWO_IN_FIGHT"), 10));
                return;
            }

            PenaltyLogDTO penalty = DAOFactory.PenaltyLogDAO.LoadByAccount(Session.Account.AccountId).FirstOrDefault(x => x.DateEnd > DateTime.Now && x.Penalty == PenaltyType.TwoVersusTwoBan);

            if (penalty != null)
            {
                Session.SendPacket($"info You are banned from the 2v2 till {penalty.DateEnd} by {penalty.AdminName}!");
                return;
            }

            //if (Session.Character?.MapInstance?.Sessions?.Count(s => s.CleanIpAddress.Equals(Session.CleanIpAddress)) > 1)
            //{
            //    Session.SendPacket(Session?.Character?.GenerateSay(Language.Instance.GetMessageFromKey("MAX_PLAYER_ALLOWED_TWOVERSUSTWO"), 10));
            //    return;
            //}

            Session.Character.DefaultTimer = 120;

            IObservable<long> observable = Observable.Interval(TimeSpan.FromSeconds(1));

            Session.SendPacket(UserInterfaceHelper.GenerateBSInfo(0, 10, Session.Character.DefaultTimer, 1));

            var alreadyExists = ServerManager.Instance.TwoVersusTwoMembers.FirstOrDefault(s => s.CharacterId == Session.Character.CharacterId);
            if (alreadyExists != null)
                ServerManager.Instance.TwoVersusTwoMembers.Remove(alreadyExists);

            Session.Character.TwoVersusTwoBattle = new();

            var mtm = new TwoVersusTwoMember
            {
                GroupedId = 0,
                IsDead = false,
                EnemyGroupId = 0, // Specific group will be given after
                CharacterId = Session.Character.CharacterId
            };

            ServerManager.Instance.TwoVersusTwoMembers.Add(mtm);

            Session.Character.TwoVersusTwoBattle.IsDead = false;

            void action(List<ClientSession> a)
            {
                Session.Character.DefaultTimer--;

                if (Session.Character.DefaultTimer <= 0)
                {
                    Session.SendPacket(UserInterfaceHelper.GenerateBSInfo(2, 10, 0, 0));
                    Session.Character.ArenaDisposable?.Dispose();
                }

                a = new();
                if (Session.Character.Group == null)
                {
                    var ally = ServerManager.Instance.TwoVersusTwoMembers.FirstOrDefault(s => s.CharacterId != Session.Character.CharacterId);
                    if (ally != null)
                    {
                        var session = ServerManager.Instance.GetSessionByCharacterId(ally.CharacterId);
                        a.Add(session);
                        a.Add(Session);
                    }
                }
                else
                {
                    a.AddRange(Session.Character.Group.Sessions.ToList());
                }

                var searchEnemy = ServerManager.Instance.TwoVersusTwoMembers.FirstOrDefault(s => !a.Any(x => s.CharacterId == x.Character.CharacterId));
                if (searchEnemy != null)
                {
                    var getSession = ServerManager.Instance.GetSessionByCharacterId(searchEnemy.CharacterId);
                    if (getSession.Character.Group != null)
                    {
                        a.AddRange(getSession.Character.Group.Sessions.ToList());
                    }
                    else
                    {
                        var searchAllyOfEnemy = ServerManager.Instance.TwoVersusTwoMembers.FirstOrDefault(s => searchEnemy.CharacterId != s.CharacterId && !a.Any(x => s.CharacterId == x.Character.CharacterId));
                        if (searchAllyOfEnemy != null)
                        {
                            var getSessionOfAllyOfEnemy = ServerManager.Instance.GetSessionByCharacterId(searchAllyOfEnemy.CharacterId);
                            a.Add(getSession);
                            a.Add(getSessionOfAllyOfEnemy);
                        }
                    }
                }

                if (a.Count == 4)
                {
                    foreach (var ss in a)
                    {
                        var objet = ServerManager.Instance.TwoVersusTwoMembers.FirstOrDefault(s => s.CharacterId == ss.Character.CharacterId);
                        ServerManager.Instance.TwoVersusTwoMembers.Remove(objet);
                        ss.Character.ArenaDisposable?.Dispose();
                    }

                    TwoVersusTwoEvent.CheckAll(a);
                }
            }

            Session.Character.ArenaDisposable = Observable.Interval(TimeSpan.FromSeconds(1)).SafeSubscribe(s =>
            {
                action(new());
            });
        }
    }
}