using NosByte.Packets.ClientPackets;
using OpenNos.Core;
using OpenNos.Core.Extensions;
using OpenNos.Data;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Extension.CharacterExtensions;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;
using System;
using System.Linq;
using System.Reactive.Linq;

namespace OpenNos.Handler.World.Basic
{
    public class SayPacketHandler : IPacketHandler
    {
        private ClientSession Session { get; }

        public SayPacketHandler(ClientSession session) => Session = session;

        /// <summary>
        /// say packet
        /// </summary>
        /// <param name="sayPacket"></param>
        public void Say(SayPacket sayPacket)
        {
            Session.Character.SayRequests++;
            if (string.IsNullOrEmpty(sayPacket.Message))
            {
                return;
            }

            if ((Session.CurrentMapInstance.MapInstanceType == MapInstanceType.TalentArenaMapInstance ||
                Session.CurrentMapInstance.MapInstanceType == MapInstanceType.RainbowBattleInstance ||
                Session.CurrentMapInstance.MapInstanceType == MapInstanceType.CaligorInstance) && Session.Account.Authority < AuthorityType.GS)
            {
                Session.SendPacket(Session.Character.GenerateSay("You cannot talk on this map !", 0));
                return;
            }

            if (Session.Character.SayRequests > 30)
            {
                PenaltyLogDTO log = new PenaltyLogDTO
                {
                    AccountId = Session.Account.AccountId,
                    Reason = "Auto Ban SayRequests Infinite PL",
                    Penalty = PenaltyType.Banned,
                    DateStart = DateTime.Now,
                    DateEnd = DateTime.Now.AddYears(1),
                    AdminName = "NosMoon System"
                };
                Character.InsertOrUpdatePenalty(log);
                Session?.Disconnect();
                return;
            }
            Observable.Timer(TimeSpan.FromSeconds(5)).Subscribe(x =>
            {
                if (Session?.Character?.SayRequests > 0)
                {
                    Session.Character.SayRequests = 0;
                }
            });

            var penalty = Session.Account.PenaltyLogs.OrderByDescending(s => s.DateEnd).FirstOrDefault();
            var message = sayPacket.Message;

            if (Session.Character.IsMuted() && penalty != null)
            {
                if (Session.Character.Gender == GenderType.Female)
                {
                    Session.CurrentMapInstance?.Broadcast(Session, Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("MUTED_FEMALE"), 1));
                    Session.SendPacket(Session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("MUTE_TIME"), (penalty.DateEnd - DateTime.Now).ToString(@"hh\:mm\:ss")), 11));
                    Session.SendPacket(Session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("MUTE_TIME"), (penalty.DateEnd - DateTime.Now).ToString(@"hh\:mm\:ss")), 12));
                }
                else
                {
                    Session.CurrentMapInstance?.Broadcast(Session, Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("MUTED_MALE"), 1));
                    Session.SendPacket(Session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("MUTE_TIME"), (penalty.DateEnd - DateTime.Now).ToString(@"hh\:mm\:ss")), 11));
                    Session.SendPacket(Session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("MUTE_TIME"), (penalty.DateEnd - DateTime.Now).ToString(@"hh\:mm\:ss")), 12));
                }
            }
            else
            {
                var filter = new ProfanityFilter();
                message = filter.CensorString(message);

                if (Session.CurrentMapInstance.MapInstanceType == MapInstanceType.TimeSpaceInstance)
                {
                    if (message.Contains("!"))
                    {
                        string newMessage = message.Remove(0, message.IndexOf("!") + 1);
                        if (!string.IsNullOrEmpty(message))
                        {
                            foreach (ClientSession targetSession in ServerManager.Instance.TimespaceSessions)
                            {
                                targetSession?.SendPacket($"say 1 -1 7 {Session.Character.Name}> {message}");
                            }
                        }
                    }
                }

                byte type = CharacterHelper.AuthorityChatColor(Session.Character.Authority);

                var rbb = ServerManager.Instance.RainbowBattleMembers.Find(s => s.Session.Contains(Session));

                if (Session.Character.Authority >= AuthorityType.TGS)
                {
                    type = CharacterHelper.AuthorityChatColor(Session.Character.Authority);
                    if (Session.CurrentMapInstance.MapInstanceType != MapInstanceType.TalentArenaMapInstance)
                    {
                        Session.CurrentMapInstance?.Broadcast(Session, Session.Character.GenerateSay(message.Trim(), 1), ReceiverType.AllExceptMe);
                    }
                    message = $"[{Session.Character.Authority} {Session.Character.Name}]: " + message;
                }

                if (Session.CurrentMapInstance.MapInstanceType == MapInstanceType.RainbowBattleInstance && rbb != null)
                {
                    foreach (var ses in rbb.Session)
                    {
                        if (ses == Session)
                        {
                            continue;
                        }
                        ses.SendPacket(Session.Character.GenerateSay(message.Trim(), type, Session.Account.Authority >= AuthorityType.TGS));
                    }
                }
                else if (ServerManager.Instance.ChannelId == 51 && Session.Account.Authority < AuthorityType.TMOD)
                {
                    Session.CurrentMapInstance?.Broadcast(Session, Session.Character.GenerateSay(message.Trim(), type, false), ReceiverType.AllExceptMeAct4);
                }
                else
                {
                    Session.CurrentMapInstance?.Broadcast(Session, Session.Character.GenerateSay(message.Trim(), type, Session.Character.Authority >= AuthorityType.TGS), ReceiverType.AllExceptMe);
                }
            }
        }
    }
}