using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NosByte.Packets.ClientPackets;
using OpenNos.Core;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Battle;
using OpenNos.GameObject.Helpers;

namespace OpenNos.Handler.World.Battle
{
    public class MultiTargetListPacketHandler : IPacketHandler
    {
        private ClientSession Session { get; set; }

        public MultiTargetListPacketHandler(ClientSession session) => Session = session;

        /// <summary>
        /// mtlist packet
        /// </summary>
        /// <param name="multiTargetListPacket"></param>
        public void MultiTargetListHit(MultiTargetListPacket multiTargetListPacket)
        {
            if (multiTargetListPacket?.Targets == null || Session?.Character?.MapInstance == null)
            {
                return;
            }

            if (Session.Character.IsVehicled || Session.Character.MuteMessage())
            {
                Session.SendPacket(StaticPacketHelper.Cancel());
                return;
            }

            if ((DateTime.Now - Session.Character.LastTransform).TotalSeconds < 3)
            {
                Session.SendPacket(StaticPacketHelper.Cancel());
                Session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("CANT_ATTACK"), 0));
                return;
            }

            if (multiTargetListPacket.TargetsAmount <= 0
                || multiTargetListPacket.TargetsAmount != multiTargetListPacket.Targets.Count)
            {
                return;
            }

            Session.Character.MTListTargetQueue.Clear();

            foreach (MultiTargetListSubPacket subPacket in multiTargetListPacket.Targets)
            {
                Session.Character.MTListTargetQueue.Push(new MTListHitTarget(subPacket.TargetType, subPacket.TargetId, TargetHitType.AOETargetHit));
            }
        }
    }
}
