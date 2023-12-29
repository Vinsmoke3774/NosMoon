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
using OpenNos.GameObject.Networking;
using OpenNos.Handler.SharedMethods;

namespace OpenNos.Handler.World.Battle
{
    public class FalconSkillPacketHandler : IPacketHandler
    {
        private ClientSession Session { get; set; }

        public FalconSkillPacketHandler(ClientSession session) => Session = session;

        /// <summary>
        /// ob_a packet
        /// </summary>
        /// <param name="useIconFalconSkillPacket"></param>
        public void UseIconFalconSkill(UseIconFalconSkillPacket useIconFalconSkillPacket)
        {
            if (Session.Character.BattleEntity.FalconFocusedEntityId > 0)
            {
                HitRequest iconSkillHitRequest = new HitRequest(TargetHitType.SingleTargetHit, Session, ServerManager.GetSkill(1248), 4283);
                var falconFocusedEntity = Session.CurrentMapInstance.BattleEntities.FirstOrDefault(s =>
                    s.MapEntityId == Session.Character.BattleEntity.FalconFocusedEntityId);
                if (falconFocusedEntity != null)
                {
                    Session.SendPacket("ob_ar");
                    switch (falconFocusedEntity.EntityType)
                    {
                        case EntityType.Player:
                            Session.PvpHit(iconSkillHitRequest, falconFocusedEntity.Character.Session);
                            break;

                        case EntityType.Monster:
                            falconFocusedEntity.MapMonster.HitQueue.Enqueue(iconSkillHitRequest);
                            break;

                        case EntityType.Mate:
                            falconFocusedEntity.Mate.HitRequest(iconSkillHitRequest);
                            break;
                    }
                    Session.CurrentMapInstance.Broadcast(Session, $"eff_ob  {(byte)falconFocusedEntity.UserType} {falconFocusedEntity.MapEntityId} 0 4269", ReceiverType.AllExceptMe);
                }
            }
        }
    }
}
