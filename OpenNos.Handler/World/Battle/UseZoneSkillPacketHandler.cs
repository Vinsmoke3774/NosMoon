using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NosByte.Packets.ClientPackets;
using OpenNos.Core;
using OpenNos.GameObject;
using OpenNos.GameObject.Helpers;
using OpenNos.Handler.SharedMethods;

namespace OpenNos.Handler.World.Battle
{
    public class UseZoneSkillPacketHandler : IPacketHandler
    {
        private ClientSession Session { get; set; }

        public UseZoneSkillPacketHandler(ClientSession session) => Session = session;

        /// <summary>
        /// u_as packet
        /// </summary>
        /// <param name="useAoeSkillPacket"></param>
        public void UseZonesSkill(UseAOESkillPacket useAoeSkillPacket)
        {
            Session.Character.Direction = Session.Character.BeforeDirection;

            bool isMuted = Session.Character.MuteMessage();
            if (isMuted || Session.Character.IsVehicled)
            {
                Session.SendPacket(StaticPacketHelper.Cancel());
            }
            else
            {
                if (Session.Character.LastTransform.AddSeconds(3) > DateTime.Now)
                {
                    Session.SendPacket(StaticPacketHelper.Cancel());
                    Session.SendPacket(
                        UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("CANT_ATTACK"), 0));
                    return;
                }

                if (Session.Character.CanFight && Session.Character.Hp > 0)
                {
                    Session.ZoneHit(useAoeSkillPacket.CastId, useAoeSkillPacket.MapX, useAoeSkillPacket.MapY);
                }
            }
        }
    }
}
