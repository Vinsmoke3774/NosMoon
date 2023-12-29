using System.Collections.Generic;
using OpenNos.DAL.EF;
using OpenNos.Data;

namespace OpenNos.Mapper.Mappers
{
    public class CharacterQuestMapper : IMapper<CharacterQuestDTO, CharacterQuest>
    {
        public CharacterQuestDTO Map(CharacterQuest input)
        {
            if (input == null)
            {
                return null;
            }

            return new CharacterQuestDTO
            {
                Id = input.Id,
                CharacterId = input.CharacterId,
                QuestId = input.QuestId,
                FirstObjective = input.FirstObjective,
                SecondObjective = input.SecondObjective,
                ThirdObjective = input.ThirdObjective,
                FourthObjective = input.FourthObjective,
                FifthObjective = input.FifthObjective,
                IsMainQuest = input.IsMainQuest
            };
        }

        public CharacterQuest Map(CharacterQuestDTO input)
        {
            if (input == null)
            {
                return null;
            }

            return new CharacterQuest
            {
                Id = input.Id,
                CharacterId = input.CharacterId,
                QuestId = input.QuestId,
                FirstObjective = input.FirstObjective,
                SecondObjective = input.SecondObjective,
                ThirdObjective = input.ThirdObjective,
                FourthObjective = input.FourthObjective,
                FifthObjective = input.FifthObjective,
                IsMainQuest = input.IsMainQuest
            };
        }

        public IEnumerable<CharacterQuestDTO> Map(IEnumerable<CharacterQuest> input)
        {
            var result = new List<CharacterQuestDTO>();

            foreach (var data in input)
            {
                result.Add(Map(data));
            }

            return result;
        }

        public IEnumerable<CharacterQuest> Map(IEnumerable<CharacterQuestDTO> input)
        {
            var result = new List<CharacterQuest>();

            foreach (var data in input)
            {
                result.Add(Map(data));
            }

            return result;
        }
    }
}