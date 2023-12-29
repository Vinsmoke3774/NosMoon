using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNos.DAL.EF;
using OpenNos.Data;

namespace OpenNos.Mapper.Mappers
{
    public class LevelUpRewardMapper : IMapper<LevelUpRewardDTO, LevelUpRewardEntity>
    {
        public LevelUpRewardEntity Map(LevelUpRewardDTO input)
        {
            if (input == null)
            {
                return null;
            }

            return new LevelUpRewardEntity
            {
                Id = input.Id,
                RequiredLevel = input.RequiredLevel,
                Type = input.Type,
                Value = input.Value,
                Amount = input.Amount
            };
        }

        public LevelUpRewardDTO Map(LevelUpRewardEntity input)
        {
            if (input == null)
            {
                return null;
            }

            return new LevelUpRewardDTO
            {
                Id = input.Id,
                RequiredLevel = input.RequiredLevel,
                Type = input.Type,
                Value = input.Value,
                Amount = input.Amount
            };
        }

        public IEnumerable<LevelUpRewardDTO> Map(IEnumerable<LevelUpRewardEntity> input)
        {
            var result = new List<LevelUpRewardDTO>();

            foreach (var data in input)
            {
                result.Add(Map(data));
            }

            return result;
        }

        public IEnumerable<LevelUpRewardEntity> Map(IEnumerable<LevelUpRewardDTO> input)
        {
            var result = new List<LevelUpRewardEntity>();

            foreach (var data in input)
            {
                result.Add(Map(data));
            }

            return result;
        }
    }
}
