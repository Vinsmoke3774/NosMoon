using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NosByte.Packets.ClientPackets;
using OpenNos.Core;
using OpenNos.Data;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Extension.CharacterExtensions;
using OpenNos.GameObject.Helpers;

namespace OpenNos.Handler.World.Mates
{
    public class MateSpecialSkillPacketHandler : IPacketHandler
    {
        private ClientSession Session { get; set; }

        public MateSpecialSkillPacketHandler(ClientSession session) => Session = session;

        /// <summary>
        /// u_pet packet
        /// </summary>
        /// <param name="upetPacket"></param>
        public void SpecialSkill(UpetPacket upetPacket)
        {
            if (upetPacket == null)
            {
                return;
            }

            PenaltyLogDTO penalty = Session.Account.PenaltyLogs.OrderByDescending(s => s.DateEnd).FirstOrDefault();
            if (Session.Character.IsMuted() && penalty != null)
            {
                if (Session.Character.Gender == GenderType.Female)
                {
                    Session.CurrentMapInstance?.Broadcast(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("MUTED_FEMALE"), 1));
                    Session.SendPacket(Session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("MUTE_TIME"), (penalty.DateEnd - DateTime.Now).ToString("hh\\:mm\\:ss")), 11));
                }
                else
                {
                    Session.CurrentMapInstance?.Broadcast(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("MUTED_MALE"), 1));
                    Session.SendPacket(Session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("MUTE_TIME"), (penalty.DateEnd - DateTime.Now).ToString("hh\\:mm\\:ss")), 11));
                }

                return;
            }

            Mate attacker = Session.Character.Mates.ToList().FirstOrDefault(x => x.MateTransportId == upetPacket.MateTransportId);
            if (attacker == null)
            {
                return;
            }

            if (attacker.IsEgg)
            {
                return;
            }

            NpcMonsterSkill mateSkill = null;
            if (attacker.Monster.Skills.Any())
            {
                mateSkill = attacker.Monster.Skills.FirstOrDefault(x => x.Rate == 0);
            }

            if (mateSkill == null)
            {
                mateSkill = new NpcMonsterSkill
                {
                    SkillVNum = 200
                };
            }

            if (attacker.IsSitting)
            {
                return;
            }

            switch (upetPacket.TargetType)
            {
                case UserType.Monster:
                    if (attacker.Hp > 0)
                    {
                        MapMonster target = Session?.CurrentMapInstance?.GetMonsterById(upetPacket.TargetId);
                        attacker.TargetHit(target.BattleEntity, mateSkill);
                    }

                    return;

                case UserType.Npc:
                    return;

                case UserType.Player:
                    if (attacker.Hp > 0)
                    {
                        Character target = Session?.CurrentMapInstance?.GetSessionByCharacterId(upetPacket.TargetId).Character;
                        attacker.TargetHit(target.BattleEntity, mateSkill);
                    }
                    return;

                case UserType.Object:
                    return;

                default:
                    return;
            }
        }
    }
}
