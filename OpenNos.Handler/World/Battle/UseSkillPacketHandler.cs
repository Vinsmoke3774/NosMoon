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
using OpenNos.Handler.SharedMethods;

namespace OpenNos.Handler.World.Battle
{
    public class UseSkillPacketHandler : IPacketHandler
    {
        private ClientSession Session { get; set; }

        public UseSkillPacketHandler(ClientSession session) => Session = session;

        #region Properties

        public MapInstance MapInstance => Session.Character?.MapInstance ?? Mate?.Owner?.MapInstance ?? MapMonster?.MapInstance ?? MapNpc?.MapInstance;

        public MapMonster MapMonster { get; set; }

        public MapNpc MapNpc { get; set; }

        public Mate Mate { get; set; }

        #endregion

        /// <summary>
        /// u_s packet
        /// </summary>
        /// <param name="useSkillPacket"></param>
        public void UseSkill(UseSkillPacket useSkillPacket)
        {
            if (Session.Character.MapInstance == null)
            {
                Session.SendPacket(StaticPacketHelper.Cancel());
                return;
            }

            Session.Character.WalkDisposable?.Dispose();
            Session.Character.Direction = Session.Character.BeforeDirection;

            switch (useSkillPacket.UserType)
            {
                case UserType.Npc:
                    {
                        MapNpc target = Session.Character.MapInstance.GetNpc(useSkillPacket.MapMonsterId);

                        if (target != null)
                        {
                            if ((Session.Character.Morph == 1000099 /* Hamster */ && target.NpcVNum == 2329 /* Cheese Chunk */)
                                || (Session.Character.Morph == 1000156 /* Bushtail */ && target.NpcVNum == 2330 /* Grass Clump /!\ VEGAN DETECTED /!\ */))
                            {
                                Session.SendPacket(StaticPacketHelper.Cancel(2));
                                Session.SendPacket($"delay 1000 13 #guri^513^2^{target.MapNpcId}");
                                Session.Character.MapInstance.Broadcast(UserInterfaceHelper.GenerateGuri(2, 1, Session.Character.CharacterId), ReceiverType.AllExceptMe);
                                return;
                            }
                        }
                    }
                    break;
            }

            List<CharacterSkill> skills2 = Session.Character.GetSkills();
            CharacterSkill ski2 = skills2.FirstOrDefault(s => s?.Skill?.CastId == useSkillPacket.CastId && (s.Skill?.UpgradeSkill == 0 || s.Skill?.SkillType == 1));

            if (!Session.Character.CanAttack() && ski2.Skill.Element != 0)
            {
                Session.SendPacket(StaticPacketHelper.Cancel());
                return;
            }

            switch (useSkillPacket.UserType)
            {
                case UserType.Player:
                    {
                        Character target = ServerManager.Instance.GetSessionByCharacterId(useSkillPacket.MapMonsterId)?.Character;

                        if (target != null && target.CharacterId != Session.Character.CharacterId)
                        {
                            if (target.HasBuff(BCardType.CardType.FrozenDebuff, (byte)AdditionalTypes.FrozenDebuff.EternalIce))
                            {
                                Session.SendPacket(StaticPacketHelper.Cancel(2));
                                return;
                            }

                        }
                    }
                    break;
            }

            if (Session.Character.IsLaurenaMorph())
            {
                Session.SendPacket(StaticPacketHelper.Cancel());
                return;
            }

            if (Session.Character.CanFight && useSkillPacket != null && !Session.Character.IsSeal)
            {
                if (useSkillPacket.UserType == UserType.Monster)
                {
                    MapMonster monsterToAttack = Session.Character.MapInstance.GetMonsterById(useSkillPacket.MapMonsterId);
                    if (monsterToAttack != null)
                    {
                        if (Session.Character.Quests.Any(q => q.Quest.QuestType == (int)QuestType.Required && q.Quest.QuestObjectives.Any(s => s.Data == monsterToAttack.MonsterVNum)))
                        {
                            Session.Character.IncrementQuests(QuestType.Required, monsterToAttack.MonsterVNum);
                            Session.SendPacket(StaticPacketHelper.Cancel());
                            return;
                        }
                    }
                }

                List<CharacterSkill> skills = Session.Character.GetSkills();

                if (skills != null)
                {
                    CharacterSkill ski = skills.FirstOrDefault(s => s?.Skill?.CastId == useSkillPacket.CastId && (s.Skill?.UpgradeSkill == 0 || s.Skill?.SkillType == 1));

                    if (ski != null)
                    {
                        if (ski.GetSkillBCards().ToList().Any(s => s.Type.Equals((byte)BCardType.CardType.MeditationSkill)
                            && s.SubType.Equals((byte)AdditionalTypes.MeditationSkill.Sacrifice)))
                        {
                            if (Session.Character.MapInstance.BattleEntities.ToList().FirstOrDefault(s => s.UserType == useSkillPacket.UserType && s.MapEntityId == useSkillPacket.MapMonsterId) is BattleEntity targetEntity)
                            {
                                if (Session.Character.BattleEntity.CanAttackEntity(targetEntity))
                                {
                                    Session.SendPacket(StaticPacketHelper.Cancel());
                                    return;
                                }
                            }
                        }
                        else if (!(ski.Skill.TargetType == 1 && ski.Skill.HitType != 1) && !(ski.Skill.TargetType == 2 && ski.Skill.HitType == 0) && !(ski.Skill.TargetType == 1 && ski.Skill.HitType == 1))
                        {
                            if (Session.Character.MapInstance.BattleEntities.ToList().FirstOrDefault(s => s.UserType == useSkillPacket.UserType && s.MapEntityId == useSkillPacket.MapMonsterId) is BattleEntity targetEntity)
                            {
                                if (!Session.Character.BattleEntity.CanAttackEntity(targetEntity))
                                {
                                    Session.SendPacket(StaticPacketHelper.Cancel());
                                    return;
                                }
                            }
                        }

                        if (ski.SkillVNum == 1187)
                        {
                            Session.Character.CanGetNewBuffElement = false;
                        }

                        if (Session.Character.HasBuff(5001) && ski.SkillVNum == 1609)
                        {
                            Session.Character.AddStaticBuff(new StaticBuffDTO
                            {
                                CardId = 5002,
                                CharacterId = Session.Character.CharacterId,
                                RemainingTime = 3600
                            },
                            stack: false);
                            Session.Character.RemoveBuff(5001); // TODO: Foreach for every character
                        }
                    }
                }

                Session.Character.RemoveBuff(614);
                Session.Character.RemoveBuff(615);
                Session.Character.RemoveBuff(616);

                bool isMuted = Session.Character.MuteMessage();

                if (isMuted || Session.Character.IsVehicled || Session.Character.InvisibleGm)
                {
                    Session.SendPacket(StaticPacketHelper.Cancel());
                    return;
                }

                if (useSkillPacket.MapX.HasValue && useSkillPacket.MapY.HasValue)
                {
                    Session.Character.PositionX = useSkillPacket.MapX.Value;
                    Session.Character.PositionY = useSkillPacket.MapY.Value;
                }

                if (Session.Character.IsSitting)
                {
                    Session.Character.Rest();
                }

                switch (useSkillPacket.UserType)
                {
                    case UserType.Npc:
                    case UserType.Monster:
                        if (Session.Character.Hp > 0)
                        {
                            Session.TargetHit(useSkillPacket.CastId, useSkillPacket.UserType, useSkillPacket.MapMonsterId);
                        }

                        break;

                    case UserType.Player:
                        if (Session.Character.Hp > 0)
                        {
                            if (useSkillPacket.MapMonsterId != Session.Character.CharacterId)
                            {
                                Session.TargetHit(useSkillPacket.CastId, useSkillPacket.UserType, useSkillPacket.MapMonsterId, true);
                            }
                            else
                            {
                                Session.TargetHit(useSkillPacket.CastId, useSkillPacket.UserType, useSkillPacket.MapMonsterId);
                            }
                        }
                        else
                        {
                            Session.SendPacket(StaticPacketHelper.Cancel(2));
                        }

                        break;

                    default:
                        Session.SendPacket(StaticPacketHelper.Cancel(2));
                        return;
                }

                if (skills != null)
                {
                    CharacterSkill ski = skills.FirstOrDefault(s => s?.Skill?.CastId == useSkillPacket.CastId);

                    if (ski == null || !(ski.Skill.TargetType == 1 && ski.Skill.HitType == 1))
                    {
                        if (Session.Character.MapInstance.BattleEntities.FirstOrDefault(s => s.MapEntityId == useSkillPacket.MapMonsterId) is BattleEntity target)
                        {
                            if (target.Hp <= 0)
                            {
                                Session.SendPacket(StaticPacketHelper.Cancel(2));
                            }
                        }
                        else
                        {
                            Session.SendPacket(StaticPacketHelper.Cancel(2));
                        }
                    }
                }

                CharacterSkill skis = skills.FirstOrDefault(s => s?.Skill?.CastId == useSkillPacket.CastId && (s.Skill?.UpgradeSkill == 0 || s.Skill?.SkillType == 1));

                if (Session.Character.UseSp && skis.Skill.CastId != 0)
                {
                    Session.SendPacket("ms_c 0");
                    Session.SendPackets(Session.Character.GenerateQuicklist());
                }
            }
            else
            {
                Session.SendPacket(StaticPacketHelper.Cancel(2));
            }
        }
    }
}
