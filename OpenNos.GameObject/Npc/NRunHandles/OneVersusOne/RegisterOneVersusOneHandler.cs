using NosByte.Packets.ClientPackets;
using OpenNos.Core;
using OpenNos.Core.Actions;
using OpenNos.DAL;
using OpenNos.Data;
using OpenNos.Domain;
using OpenNos.GameObject.ActionHandles;
using OpenNos.GameObject.Event.ONEVERSUSONE;
using OpenNos.GameObject.Extension.CharacterExtensions;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;
using System;
using System.Linq;

namespace OpenNos.GameObject.Npc.NRunHandles.TalentArena
{
    [NRunHandler(NRunType.OneVersusOneRegister)]
    public class RegisterOneVersusOneHandler : SpecialHandlerBase, IGenericHandler<NRunPacket>
    {
        public RegisterOneVersusOneHandler(ClientSession session) : base(session)
        {
        }

        public void ValidateData(NRunPacket packet)
        {
            Execute(packet);
        }

        public void Execute(NRunPacket packet)
        {
            if (ServerManager.Instance.ChannelId != 1)
            {
                Session.SendPacket($"info You can only register on the channel 1!");
                return;
            }

            if (Session.CurrentMapInstance.MapInstanceType != MapInstanceType.BaseMapInstance)
            {
                return;
            }

            if (Session.Character.Group != null)
            {
                Session?.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("NOT_ALLOWED_IN_GROUP"), 0));
                return;
            }

            if (Session.Character.LastSkillUse.AddSeconds(20) > DateTime.Now || Session.Character.LastDefence.AddSeconds(20) > DateTime.Now)
            {
                Session.SendPacket(UserInterfaceHelper.GenerateInfo(string.Format(Language.Instance.GetMessageFromKey("PLAYER_IN_BATTLE"), Session.Character.Name)));
                return;
            }


            if (Session.Character.LastDefence.AddSeconds(2) > DateTime.Now)
            {
                Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("CANT_LAUNCH_ONEVERSUSONE_IN_FIGHT"), 10));
                return;
            }

            PenaltyLogDTO penalty = DAOFactory.PenaltyLogDAO.LoadByAccount(Session.Account.AccountId).FirstOrDefault(x => x.DateEnd > DateTime.Now && x.Penalty == PenaltyType.OneVersusOneBan);

            if (penalty != null)
            {
                Session?.SendPacket($"info You are banned from OneVersusOne till {penalty?.DateEnd} by {penalty?.AdminName}!");
                return;
            }

            /*
            if (Session.Character?.MapInstance?.Sessions?.Count(s => s.CleanIpAddress.Equals(Session.CleanIpAddress)) > 1)
            {
                Session?.SendPacket(Session?.Character?.GenerateSay(Language.Instance.GetMessageFromKey("MAX_PLAYER_ALLOWED_ONEVERSUSONE"), 10));
                return;
            }
            */

            Session.Character.DefaultTimer = 120;

            ServerManager.Instance.OneVersusOneMembers.Add(new OneVersusOneMember
            {
                OneVersusOneType = EventType.ONEVERSUSONE,
                Session = Session,
                Kills = 0,
                IsDead = false
            });

            OneVersusOneEvent.Matchmaking(Session);
        }
    }
}
