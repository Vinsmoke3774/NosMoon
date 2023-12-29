using System;
using NosByte.Packets.ClientPackets;
using OpenNos.Core;
using OpenNos.Data;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Extension.CharacterExtensions;
using OpenNos.GameObject.Networking;

namespace OpenNos.Handler.World.Basic
{
    public class ComplimentPacketHandler : IPacketHandler
    {
        private ClientSession Session { get; set; }

        public ComplimentPacketHandler(ClientSession session) => Session = session;

        /// <summary>
        /// compl packet
        /// </summary>
        /// <param name="complimentPacket"></param>
        public void Compliment(ComplimentPacket complimentPacket)
        {
            if (complimentPacket != null)
            {
                if (Session.Character.CharacterId == complimentPacket.CharacterId)
                {
                    return;
                }

                ClientSession sess = ServerManager.Instance.GetSessionByCharacterId(complimentPacket.CharacterId);
                if (sess != null)
                {
                    if (Session.Character.Level >= 30)
                    {
                        GeneralLogDTO dto =
                            Session.Character.GeneralLogs.LastOrDefault(s =>
                                s.LogData == "World" && s.LogType == "Connection");
                        GeneralLogDTO lastcompliment =
                            Session.Character.GeneralLogs.LastOrDefault(s =>
                                s.LogData == "World" && s.LogType == nameof(Compliment));
                        if (dto?.Timestamp.AddMinutes(60) <= DateTime.Now)
                        {
                            if (lastcompliment == null || lastcompliment.Timestamp.AddDays(1) <= DateTime.Now.Date)
                            {
                                sess.Character.Compliment++;
                                Session.SendPacket(Session.Character.GenerateSay(
                                    string.Format(Language.Instance.GetMessageFromKey("COMPLIMENT_GIVEN"),
                                        sess.Character.Name), 12));
                                Session.Character.GeneralLogs.Add(new GeneralLogDTO
                                {
                                    AccountId = Session.Account.AccountId,
                                    CharacterId = Session.Character.CharacterId,
                                    IpAddress = Session.CleanIpAddress,
                                    LogData = "World",
                                    LogType = nameof(Compliment),
                                    Timestamp = DateTime.Now
                                });

                                Session.CurrentMapInstance?.Broadcast(Session,
                                    Session.Character.GenerateSay(
                                        string.Format(Language.Instance.GetMessageFromKey("COMPLIMENT_RECEIVED"),
                                            Session.Character.Name), 12), ReceiverType.OnlySomeone,
                                    characterId: complimentPacket.CharacterId);
                            }
                            else
                            {
                                Session.SendPacket(
                                    Session.Character.GenerateSay(
                                        Language.Instance.GetMessageFromKey("COMPLIMENT_COOLDOWN"), 11));
                            }
                        }
                        else if (dto != null)
                        {
                            Session.SendPacket(Session.Character.GenerateSay(
                                string.Format(Language.Instance.GetMessageFromKey("COMPLIMENT_LOGIN_COOLDOWN"),
                                    (dto.Timestamp.AddMinutes(60) - DateTime.Now).Minutes), 11));
                        }
                    }
                    else
                    {
                        Session.SendPacket(
                            Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("COMPLIMENT_NOT_MINLVL"),
                                11));
                    }
                }
            }
        }
    }
}
