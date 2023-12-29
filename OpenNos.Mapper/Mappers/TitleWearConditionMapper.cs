using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNos.DAL.EF;
using OpenNos.Data;

namespace OpenNos.Mapper.Mappers
{
    public class TitleWearConditionMapper : IMapper<TitleWearConditionDTO, TitleWearConditionEntity>
    {
        public TitleWearConditionEntity Map(TitleWearConditionDTO input)
        {
            if (input == null)
            {
                return null;
            }

            return new TitleWearConditionEntity
            {
                Id = input.Id,
                ConditionVNum = input.ConditionVNum,
                TitleVNum =  input.TitleVNum,
                Message = input.Message
            };
        }

        public TitleWearConditionDTO Map(TitleWearConditionEntity input)
        {
            if (input == null)
            {
                return null;
            }

            return new TitleWearConditionDTO
            {
                Id = input.Id,
                ConditionVNum = input.ConditionVNum,
                TitleVNum = input.TitleVNum,
                Message = input.Message
            };
        }

        public IEnumerable<TitleWearConditionDTO> Map(IEnumerable<TitleWearConditionEntity> input)
        {
            var result = new List<TitleWearConditionDTO>();

            foreach (var data in input)
            {
                result.Add(Map(data));
            }

            return result;
        }

        public IEnumerable<TitleWearConditionEntity> Map(IEnumerable<TitleWearConditionDTO> input)
        {
            var result = new List<TitleWearConditionEntity>();

            foreach (var data in input)
            {
                result.Add(Map(data));
            }

            return result;
        }
    }
}
