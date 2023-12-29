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
    [NRunHandler(NRunType.ArenaRegisterTalent)]
    public class RegisterAotHandler : SpecialHandlerBase, IGenericHandler<NRunPacket>
    {
        public RegisterAotHandler(ClientSession session) : base(session)
        {
        }

        public void ValidateData(NRunPacket packet)
        {
            Execute(packet);
        }

        public void Execute(NRunPacket packet)
        {
            /*
            if (Session.CurrentMapInstance.MapInstanceType == MapInstanceType.TalentArenaMapInstance || Session.CurrentMapInstance.MapInstanceType != MapInstanceType.BaseMapInstance)
            {
                return;
            }


            if (Session.Character.Level < 30)
            {
                Session?.SendPacket(UserInterfaceHelper.GenerateInfo("You need to be at least level 30 to participate"));
                return;
            }

            PenaltyLogDTO penalty = DAOFactory.PenaltyLogDAO.LoadByAccount(Session.Account.AccountId)
                                 .FirstOrDefault(x =>
                                         x.DateEnd > DateTime.Now &&
                                         x.Penalty == PenaltyType.ArenaBan);

            if (penalty != null)
            {
                Session?.SendPacket($"info You are banned from Arena of Talents till {penalty?.DateEnd} by {penalty?.AdminName}!");
                return;
            }

            if (Session?.Character?.MapInstance?.Sessions?.Count(s => s.CleanIpAddress.Equals(Session.CleanIpAddress)) > 1)
            {
                Session?.SendPacket(Session?.Character?.GenerateSay(Language.Instance.GetMessageFromKey("MAX_PLAYER_ALLOWED_ARENA"), 10));
                return;
            }

            ServerManager.Instance.ArenaMembers.Add(new ArenaMember
            {
                ArenaType = EventType.UNRANKEDTALENTARENA,
                Session = Session,
                GroupId = null,
                Time = 0
            });
            
            UnrankedArenaEvent.Matchmaking(Session);
            */
            return;
        }
    }
}
