using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNos.DAL.EF;
using OpenNos.Data;

namespace OpenNos.Mapper.Mappers
{
    public class InstantBattleLogMapper : IMapper<InstantBattleLogDTO, InstantBattleLog>
    {
        public InstantBattleLog Map(InstantBattleLogDTO input)
        {
            if (input == null)
            {
                return null;
            }

            return new InstantBattleLog
            { 
                Id = input.Id,
                CharacterId = input.CharacterId,
                DateTime = input.DateTime
            };
        }

        public InstantBattleLogDTO Map(InstantBattleLog input)
        {
            if (input == null)
            {
                return null;
            }

            return new InstantBattleLogDTO
            {
                Id = input.Id,
                CharacterId = input.CharacterId,
                DateTime = input.DateTime
            };
        }

        public IEnumerable<InstantBattleLogDTO> Map(IEnumerable<InstantBattleLog> input)
        {
            var result = new List<InstantBattleLogDTO>();

            foreach (var data in input)
            {
                result.Add(Map(data));
            }

            return result;
        }

        public IEnumerable<InstantBattleLog> Map(IEnumerable<InstantBattleLogDTO> input)
        {
            var result = new List<InstantBattleLog>();

            foreach (var data in input)
            {
                result.Add(Map(data));
            }

            return result;
        }
    }
}
