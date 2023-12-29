using NosByte.Packets.ClientPackets;
using OpenNos.Core;
using OpenNos.Core.Actions;
using OpenNos.DAL;
using OpenNos.Data;
using OpenNos.Domain;
using OpenNos.GameObject.ActionHandles;
using OpenNos.GameObject.Event.ARENA;
using OpenNos.GameObject.Extension.CharacterExtensions;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;
using System;
using System.Linq;

namespace OpenNos.GameObject.Npc.NRunHandles.TalentArena
{
    [NRunHandler(NRunType.ArenaRankedRegister)]
    public class RegisterAotRankedHandler : SpecialHandlerBase, IGenericHandler<NRunPacket>
    {
        public RegisterAotRankedHandler(ClientSession session) : base(session)
        {
        }

        public void ValidateData(NRunPacket packet)
        {
            Execute(packet);
        }

        public void Execute(NRunPacket packet)
        {
            /*
            if (ServerManager.Instance.ChannelId != 1)
            {
                Session.SendPacket($"info You can only register on the channel 1!");
                return;
            }

            if (Session.CurrentMapInstance.MapInstanceType == MapInstanceType.TalentArenaMapInstance || Session.CurrentMapInstance.MapInstanceType != MapInstanceType.BaseMapInstance)
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

            if (Session.Character.Level < 80)
            {
                Session?.SendPacket(UserInterfaceHelper.GenerateInfo(Language.Instance.GetMessageFromKey("NOT_REQUIRED_LEVEL_FOR_AOT_RANKED")));
                return;
            }

            PenaltyLogDTO penalty = DAOFactory.PenaltyLogDAO.LoadByAccount(Session.Account.AccountId).FirstOrDefault(x => x.DateEnd > DateTime.Now && x.Penalty == PenaltyType.ArenaBan);

            if (penalty != null)
            {
                Session?.SendPacket($"info You are banned from Arena of Talents till {penalty?.DateEnd} by {penalty?.AdminName}!");
                return;
            }

            if (Session.Character?.MapInstance?.Sessions?.Count(s => s.CleanIpAddress.Equals(Session.CleanIpAddress)) > 1)
            {
                Session?.SendPacket(Session?.Character?.GenerateSay(Language.Instance.GetMessageFromKey("MAX_PLAYER_ALLOWED_ARENA"), 10));
                return;
            }

            if (Session.Character.LastDefence.AddSeconds(2) > DateTime.Now)
            {
                Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("CANT_LAUNCH_ARENA_IN_FIGHT"), 10));
                return;
            }

            ServerManager.Instance.ArenaMembers.Add(new ArenaMember
            {
                ArenaType = EventType.TALENTARENA,
                Session = Session,
                GroupId = null,
                Time = 0,
                IsDead = false
            });

            TalentArenaEvent.Matchmaking(Session);
            */
            return;
        }
    }
}
