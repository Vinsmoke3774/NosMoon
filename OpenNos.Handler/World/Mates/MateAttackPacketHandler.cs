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
using OpenNos.GameObject.Networking;

namespace OpenNos.Handler.World.Mates
{
    public class MateAttackPacketHandler : IPacketHandler
    {
        private ClientSession Session { get; set; }

        public MateAttackPacketHandler(ClientSession session) => Session = session;

        /// <summary>
        /// suctl packet
        /// </summary>
        /// <param name="suctlPacket"></param>
        public void Attack(SuctlPacket suctlPacket)
        {
            if (suctlPacket == null)
            {
                return;
            }

            PenaltyLogDTO penalty = Session.Account.PenaltyLogs.OrderByDescending(s => s.DateEnd).FirstOrDefault();
            if (Session.Character.IsMuted() && penalty != null)
            {
                if (Session.Character.Gender == GenderType.Female)
                {
                    Session.CurrentMapInstance?.Broadcast(
                        Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("MUTED_FEMALE"), 1));
                    Session.SendPacket(Session.Character.GenerateSay(
                        string.Format(Language.Instance.GetMessageFromKey("MUTE_TIME"),
                            (penalty.DateEnd - DateTime.Now).ToString("hh\\:mm\\:ss")), 11));
                }
                else
                {
                    Session.CurrentMapInstance?.Broadcast(
                        Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("MUTED_MALE"), 1));
                    Session.SendPacket(Session.Character.GenerateSay(
                        string.Format(Language.Instance.GetMessageFromKey("MUTE_TIME"),
                            (penalty.DateEnd - DateTime.Now).ToString("hh\\:mm\\:ss")), 11));
                }

                return;
            }

            Mate attacker = Session.Character.Mates.Find(x => x.MateTransportId == suctlPacket.MateTransportId);

            if (attacker != null && !attacker.HasBuff(BCardType.CardType.SpecialAttack, (byte)AdditionalTypes.SpecialAttack.NoAttack))
            {

                if (attacker.IsEgg)
                {
                    return;
                }

                IEnumerable<NpcMonsterSkill> mateSkills = attacker.Skills;

                if (mateSkills != null)
                {
                    NpcMonsterSkill skill = null;

                    List<NpcMonsterSkill> PossibleSkills = mateSkills.Where(s => (DateTime.Now - s.LastSkillUse).TotalMilliseconds >= 1000 * s.Skill.Cooldown || s.Rate == 0).ToList();

                    foreach (NpcMonsterSkill ski in PossibleSkills.OrderBy(rnd => ServerManager.RandomNumber()))
                    {
                        if (ski.Rate == 0)
                        {
                            skill = ski;
                        }
                        else if (ServerManager.RandomNumber() < ski.Rate)
                        {
                            skill = ski;
                            break;
                        }
                    }

                    switch (suctlPacket.TargetType)
                    {
                        case UserType.Monster:
                            if (attacker.Hp > 0)
                            {
                                MapMonster target = Session.CurrentMapInstance?.GetMonsterById(suctlPacket.TargetId);
                                if (target != null)
                                {
                                    if (attacker.BattleEntity.CanAttackEntity(target.BattleEntity))
                                    {
                                        attacker.TargetHit(target.BattleEntity, skill);
                                    }
                                }
                            }

                            return;

                        case UserType.Npc:
                            if (attacker.Hp > 0)
                            {
                                Mate target = Session.CurrentMapInstance?.GetMate(suctlPacket.TargetId);
                                if (target != null)
                                {
                                    if (attacker.Owner.BattleEntity.CanAttackEntity(target.BattleEntity))
                                    {
                                        attacker.TargetHit(target.BattleEntity, skill);
                                    }
                                    else
                                    {
                                        Session.SendPacket(StaticPacketHelper.Cancel(2, target.CharacterId));
                                    }
                                }
                            }
                            return;

                        case UserType.Player:
                            if (attacker.Hp > 0)
                            {
                                Character target = Session.CurrentMapInstance?.GetSessionByCharacterId(suctlPacket.TargetId)?.Character;
                                if (target != null)
                                {
                                    if (attacker.Owner.BattleEntity.CanAttackEntity(target.BattleEntity))
                                    {
                                        attacker.TargetHit(target.BattleEntity, skill);
                                    }
                                    else
                                    {
                                        Session.SendPacket(StaticPacketHelper.Cancel(2, target.CharacterId));
                                    }
                                }
                            }

                            return;

                        case UserType.Object:
                            return;
                    }
                }
            }
        }
    }
}
