using NosByte.Packets.ClientPackets;
using OpenNos.Core;
using OpenNos.Data;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Extension.CharacterExtensions;
using OpenNos.GameObject.Networking;
using OpenNos.GameObject.RainbowBattle;
using System;

namespace OpenNos.Handler.World.Basic
{
    public class LeaveRbbPacketHandler : IPacketHandler
    {
        private ClientSession Session { get; set; }

        public LeaveRbbPacketHandler(ClientSession session) => Session = session;

        public void Fb(FbPacket e)
        {
            if (e == null)
            {
                return;
            }

            var rbb = ServerManager.Instance.RainbowBattleMembers.Find(s => s.Session.Contains(Session));

            if (rbb == null)
            {
                return;
            }

            PenaltyLogDTO log = new PenaltyLogDTO
            {
                AccountId = Session.Account.AccountId,
                Reason = "Leaving Rainbow Battle",
                Penalty = PenaltyType.RainbowBan,
                DateStart = DateTime.Now,
                DateEnd = DateTime.Now.AddMinutes(10),
                AdminName = "NosMoon System",
            };

            Character.InsertOrUpdatePenalty(log);
            rbb.Session.Remove(Session);
            RainbowBattleManager.SendFbs(Session.CurrentMapInstance);
            Session.Character.Group?.LeaveGroup(Session);
            ServerManager.Instance.UpdateGroup(Session.Character.CharacterId);
            ServerManager.Instance.ChangeMap(Session.Character.CharacterId, Session.Character.MapId, Session.Character.MapX, Session.Character.MapY);
            Session.SendPacket(Session.Character.GenerateRaid(2, true));
        }
    }
}
