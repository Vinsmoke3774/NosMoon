using System.Collections.Generic;
using OpenNos.DAL.EF;
using OpenNos.Data;

namespace OpenNos.Mapper.Mappers
{
    public class NpcMonsterSkillMapper : IMapper<NpcMonsterSkillDTO, NpcMonsterSkill>
    {
        public NpcMonsterSkill Map(NpcMonsterSkillDTO input)
        {
            if (input == null)
            {
                return null;
            }

            return new NpcMonsterSkill
            {
                NpcMonsterSkillId = input.NpcMonsterSkillId,
                NpcMonsterVNum = input.NpcMonsterVNum,
                Rate = input.Rate,
                SkillVNum = input.SkillVNum
            };
        }

        public NpcMonsterSkillDTO Map(NpcMonsterSkill input)
        {
            if (input == null)
            {
                return null;
            }

            return new NpcMonsterSkillDTO
            {
                NpcMonsterSkillId = input.NpcMonsterSkillId,
                NpcMonsterVNum = input.NpcMonsterVNum,
                Rate = input.Rate,
                SkillVNum = input.SkillVNum
            };
        }

        public IEnumerable<NpcMonsterSkillDTO> Map(IEnumerable<NpcMonsterSkill> input)
        {
            var result = new List<NpcMonsterSkillDTO>();

            foreach (var data in input)
            {
                result.Add(Map(data));
            }

            return result;
        }

        public IEnumerable<NpcMonsterSkill> Map(IEnumerable<NpcMonsterSkillDTO> input)
        {
            var result = new List<NpcMonsterSkill>();

            foreach (var data in input)
            {
                result.Add(Map(data));
            }

            return result;
        }
    }
}