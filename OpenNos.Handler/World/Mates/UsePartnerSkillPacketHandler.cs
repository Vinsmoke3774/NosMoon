using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NosByte.Packets.ClientPackets;
using OpenNos.Core;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Helpers;
using OpenNos.Handler.SharedMethods;

namespace OpenNos.Handler.World.Mates
{
    public class UsePartnerSkillPacketHandler : IPacketHandler
    {
        private ClientSession Session { get; set; }

        public UsePartnerSkillPacketHandler(ClientSession session) => Session = session;


        /// <summary>
        /// u_ps packet
        /// </summary>
        /// <param name="usePartnerSkillPacket"></param>
        public void UseSkill(UsePartnerSkillPacket usePartnerSkillPacket)
        {
            #region Invalid packet

            if (usePartnerSkillPacket == null)
            {
                return;
            }

            #endregion

            #region Mate not found (or invalid)

            Mate mate = Session.Character.Mates.Find(x => x.MateTransportId == usePartnerSkillPacket.TransportId
                                                                     && x.MateType == MateType.Partner && x.IsTeamMember);
            if (mate == null)
            {
                return;
            }

            #endregion

            #region Not using PSP

            if (mate.Sp == null || !mate.IsUsingSp)
            {
                return;
            }

            #endregion

            #region Skill not found

            PartnerSkill partnerSkill = mate.Sp.GetSkill(usePartnerSkillPacket.CastId);

            if (partnerSkill == null)
            {
                return;
            }

            #endregion

            #region Convert PartnerSkill to Skill

            Skill skill = PartnerSkillHelper.ConvertToNormalSkill(partnerSkill);

            #endregion

            #region Battle entities

            BattleEntity battleEntityAttacker = mate.BattleEntity;
            BattleEntity battleEntityDefender = null;

            switch (usePartnerSkillPacket.TargetType)
            {
                case UserType.Player:
                    {
                        Character target = Session.Character.MapInstance?.GetCharacterById(usePartnerSkillPacket.TargetId);
                        battleEntityDefender = target?.BattleEntity;
                    }
                    break;

                case UserType.Npc:
                    {
                        Mate target = Session.Character.MapInstance?.GetMate(usePartnerSkillPacket.TargetId);
                        battleEntityDefender = target?.BattleEntity;
                    }
                    break;

                case UserType.Monster:
                    {
                        MapMonster target = Session.Character.MapInstance?.GetMonsterById(usePartnerSkillPacket.TargetId);
                        battleEntityDefender = target?.BattleEntity;
                    }
                    break;
            }

            #endregion

            #region Attack

            battleEntityAttacker.PartnerSkillTargetHit(battleEntityDefender, skill);

            #endregion
        }
    }
}
