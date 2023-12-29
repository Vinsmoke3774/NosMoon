using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNos.DAL.EF;
using OpenNos.Data;

namespace OpenNos.Mapper.Mappers
{
    public class WhitelistedPlayerMapper : IMapper<WhitelistedPlayerDTO, WhitelistedPlayerEntity>
    {
        public WhitelistedPlayerEntity Map(WhitelistedPlayerDTO input)
        {
            if (input == null)
            {
                return null;
            }

            return new WhitelistedPlayerEntity
            {
                Id = input.Id,
                IpAddress = input.IpAddress
            };
        }

        public WhitelistedPlayerDTO Map(WhitelistedPlayerEntity input)
        {
            if (input == null)
            {
                return null;
            }

            return new WhitelistedPlayerDTO
            {
                Id = input.Id,
                IpAddress = input.IpAddress
            };
        }

        public IEnumerable<WhitelistedPlayerDTO> Map(IEnumerable<WhitelistedPlayerEntity> input)
        {
            var result = new List<WhitelistedPlayerDTO>();

            foreach (var data in input)
            {
                result.Add(Map(data));
            }

            return result;
        }

        public IEnumerable<WhitelistedPlayerEntity> Map(IEnumerable<WhitelistedPlayerDTO> input)
        {
            var result = new List<WhitelistedPlayerEntity>();

            foreach (var data in input)
            {
                result.Add(Map(data));
            }

            return result;
        }
    }
}
