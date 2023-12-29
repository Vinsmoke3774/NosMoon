using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNos.DAL.EF;
using OpenNos.Data;

namespace OpenNos.Mapper.Mappers
{
    public class TitleLogMapper : IMapper<TitleLogDTO, TitleLog>
    {
        public TitleLog Map(TitleLogDTO input)
        {
            if (input == null)
            {
                return null;
            }

            return new TitleLog
            {
                Id = input.Id,
                CharacterId = input.CharacterId,
                TitleId = input.TitleId,
                DateTime = DateTime.Now
            };
        }

        public TitleLogDTO Map(TitleLog input)
        {
            if (input == null)
            {
                return null;
            }

            return new TitleLogDTO
            {
                Id = input.Id,
                CharacterId = input.CharacterId,
                TitleId = input.TitleId,
                DateTime = DateTime.Now
            };
        }

        public IEnumerable<TitleLogDTO> Map(IEnumerable<TitleLog> input)
        {
            var result = new List<TitleLogDTO>();

            foreach (var data in input)
            {
                result.Add(Map(data));
            }

            return result;
        }

        public IEnumerable<TitleLog> Map(IEnumerable<TitleLogDTO> input)
        {
            var result = new List<TitleLog>();

            foreach (var data in input)
            {
                result.Add(Map(data));
            }

            return result;
        }
    }
}
