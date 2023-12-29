using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NosByte.Packets.ClientPackets;
using OpenNos.Core;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Extension.CharacterExtensions;
using OpenNos.GameObject.Helpers;

namespace OpenNos.Handler.World.Mates
{
    public class PsOpPacketHandler : IPacketHandler
    {
        private ClientSession Session { get; set; }

        public PsOpPacketHandler(ClientSession session) => Session = session;


        /// <summary>
        /// ps_op packet
        /// </summary>
        /// <param name="partnerSkillOpenPacket"></param>
        public void PartnerSkillOpen(PartnerSkillOpenPacket partnerSkillOpenPacket)
        {
            if (partnerSkillOpenPacket == null
                || partnerSkillOpenPacket.CastId < 0
                || partnerSkillOpenPacket.CastId > 2)
            {
                return;
            }

            Mate mate = Session?.Character?.Mates?.ToList().FirstOrDefault(s => s.IsTeamMember && s.MateType == MateType.Partner && s.PetId == partnerSkillOpenPacket.PetId);

            if (mate?.Sp == null || mate.IsUsingSp)
            {
                Session.SendPacket(Session.Character.GenerateSay("Your partner must not be transformed to learn a new skill.", 11));
                return;
            }

            if (!mate.Sp.CanLearnSkill())
            {
                return;
            }

            PartnerSkill partnerSkill = mate.Sp.GetSkill(partnerSkillOpenPacket.CastId);

            if (partnerSkill != null)
            {
                return;
            }

            if (partnerSkillOpenPacket.JustDoIt)
            {
                if (mate.Sp.AddSkill(mate, partnerSkillOpenPacket.CastId))
                {
                    Session.SendPacket(UserInterfaceHelper.GenerateModal(Language.Instance.GetMessageFromKey("PSP_SKILL_LEARNED"), 1));
                    mate.Sp.ResetXp();
                }

                Session.SendPacket(mate.GenerateScPacket());
            }
            else
            {
                if (Session.Account.Authority >= AuthorityType.DEV)
                {
                    if (mate.Sp.AddSkill(mate, partnerSkillOpenPacket.CastId))
                    {
                        Session.SendPacket(UserInterfaceHelper.GenerateModal(Language.Instance.GetMessageFromKey("PSP_SKILL_LEARNED"), 1));
                        mate.Sp.FullXp();
                    }

                    Session.SendPacket(mate.GenerateScPacket());
                    return;
                }
                Session.SendPacket($"pdelay 3000 12 #ps_op^{partnerSkillOpenPacket.PetId}^{partnerSkillOpenPacket.CastId}^1");
                Session.CurrentMapInstance?.Broadcast(UserInterfaceHelper.GenerateGuri(2, 2, mate.MateTransportId), mate.PositionX, mate.PositionY);
            }
        }
    }
}
