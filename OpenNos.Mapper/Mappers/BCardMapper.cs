using System.Collections.Generic;
using OpenNos.DAL.EF;
using OpenNos.Data;

namespace OpenNos.Mapper.Mappers
{
    public class BCardMapper : IMapper<BCardDTO, BCard>
    {
        public BCard Map(BCardDTO input)
        {
            if (input == null)
            {
                return null;
            }

            return new BCard
            {
                BCardId = input.BCardId,
                CardId = input.CardId,
                CastType = input.CastType,
                FirstData = input.FirstData,
                IsLevelDivided = input.IsLevelDivided,
                IsLevelScaled = input.IsLevelScaled,
                ItemVNum = input.ItemVNum,
                NpcMonsterVNum = input.NpcMonsterVNum,
                SecondData = input.SecondData,
                SkillVNum = input.SkillVNum,
                SubType = input.SubType,
                ThirdData = input.ThirdData,
                Type = input.Type,
                MinLevel = input.MinLevel,
                MaxLevel = input.MaxLevel
            };
        }

        public BCardDTO Map(BCard input)
        {
            if (input == null)
            {
                return null;
            }

            return new BCardDTO
            {
                BCardId = input.BCardId,
                CardId = input.CardId,
                CastType = input.CastType,
                FirstData = input.FirstData,
                IsLevelDivided = input.IsLevelDivided,
                IsLevelScaled = input.IsLevelScaled,
                ItemVNum = input.ItemVNum,
                NpcMonsterVNum = input.NpcMonsterVNum,
                SecondData = input.SecondData,
                SkillVNum = input.SkillVNum,
                SubType = input.SubType,
                ThirdData = input.ThirdData,
                Type = input.Type,
                MinLevel = input.MinLevel,
                MaxLevel = input.MaxLevel
            };
        }

        public IEnumerable<BCardDTO> Map(IEnumerable<BCard> input)
        {
            var result = new List<BCardDTO>();

            foreach (var data in input)
            {
                result.Add(Map(data));
            }

            return result;
        }

        public IEnumerable<BCard> Map(IEnumerable<BCardDTO> input)
        {
            var result = new List<BCard>();

            foreach (var data in input)
            {
                result.Add(Map(data));
            }

            return result;
        }
    }
}