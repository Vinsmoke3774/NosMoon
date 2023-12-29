using NosByte.Packets.ClientPackets;
using NosByte.Shared;
using OpenNos.Core.Actions;
using OpenNos.DAL;
using OpenNos.Data;
using OpenNos.Domain;
using OpenNos.GameObject.ActionHandles;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace OpenNos.GameObject.Npc.NRunHandles.TwoVersusTwo
{
    [NRunHandler(NRunType.DeathMatchRegister)]
    public class DeathMatchRegisterHandler : SpecialHandlerBase, IGenericHandler<NRunPacket>
    {
        public DeathMatchRegisterHandler(ClientSession session) : base(session)
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
                Session.Character.Group.LeaveGroup(Session);
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

            PenaltyLogDTO penalty = DAOFactory.PenaltyLogDAO.LoadByAccount(Session.Account.AccountId).FirstOrDefault(x => x.DateEnd > DateTime.Now && x.Penalty == PenaltyType.TwoVersusTwoBan);

            if (penalty != null)
            {
                Session.SendPacket($"info You are banned from the 2v2 till {penalty.DateEnd} by {penalty.AdminName}!");
                return;
            }

            /*
            if (Session.Character?.MapInstance?.Sessions?.Count(s => s.CleanIpAddress.Equals(Session.CleanIpAddress)) > 1)
            {
                Session.SendPacket(Session?.Character?.GenerateSay(Language.Instance.GetMessageFromKey("MAX_PLAYER_ALLOWED_TWOVERSUSTWO"), 10));
                return;
            }
            */

            Session.Character.DefaultTimer = 120;

            IObservable<long> observable = Observable.Interval(TimeSpan.FromSeconds(1));

            Session.SendPacket(UserInterfaceHelper.GenerateBSInfo(0, 10, Session.Character.DefaultTimer, 1));

            var mtm = new DeathMatchMember
            {
                IsDead = false,
                IsWaiting = true,
                Session = Session,
            };

            ServerManager.Instance.DeathMatchMembers.Add(mtm);

            Session.Character.DeathMatchMember.IsDead = false;

            void action(List<ClientSession> a)
            {
                Session.Character.DefaultTimer--;

                if (Session.Character.DefaultTimer <= 0)
                {
                    Session.SendPacket(UserInterfaceHelper.GenerateBSInfo(2, 10, 0, 0));
                    Session.Character.ArenaDisposable?.Dispose();
                }

                a = new();
                var players = ServerManager.Instance.DeathMatchMembers.Where(s => s.IsWaiting).Take(10);
                a.AddRange(players.Select(s => s.Session));

                if (a.Count == 10)
                {
                    foreach (var ss in a)
                    {
                        var objet = ServerManager.Instance.DeathMatchMembers.FirstOrDefault(s => s.Session.Character.CharacterId == ss.Character.CharacterId);
                        ServerManager.Instance.DeathMatchMembers.ToList()[ServerManager.Instance.DeathMatchMembers.ToList().IndexOf(objet)].IsWaiting = true;
                        ServerManager.Instance.DeathMatchMembers.Remove(objet);
                        ss.Character.ArenaDisposable?.Dispose();
                    }

                    Event.DEATHMATCH.DeathMatch.TeleportToLobby(a);
                }
                else
                {
                    foreach (var ss in a)
                    {
                        var objet = ServerManager.Instance.DeathMatchMembers.FirstOrDefault(s => s.Session.Character.CharacterId == ss.Character.CharacterId);
                        ServerManager.Instance.DeathMatchMembers.ToList()[ServerManager.Instance.DeathMatchMembers.ToList().IndexOf(objet)].IsWaiting = true;
                    }
                }
            }

            Session.Character.ArenaDisposable = Observable.Interval(TimeSpan.FromSeconds(1)).SafeSubscribe(s =>
            {
                action(new());
            });
        }
    }
}