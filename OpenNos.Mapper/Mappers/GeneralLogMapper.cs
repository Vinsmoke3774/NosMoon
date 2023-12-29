using System.Collections.Generic;
using OpenNos.DAL.EF;
using OpenNos.Data;

namespace OpenNos.Mapper.Mappers
{
    public class GeneralLogMapper : IMapper<GeneralLogDTO, GeneralLog>
    {
        public GeneralLog Map(GeneralLogDTO input)
        {
            if (input == null)
            {
                return null;
            }

            return new GeneralLog
            {
                AccountId = input.AccountId,
                CharacterId = input.CharacterId,
                IpAddress = input.IpAddress,
                LogData = input.LogData,
                LogId = input.LogId,
                LogType = input.LogType,
                Timestamp = input.Timestamp
            };
        }

        public GeneralLogDTO Map(GeneralLog input)
        {
            if (input == null)
            {
                return null;
            }

            return new GeneralLogDTO
            {
                AccountId = input.AccountId,
                CharacterId = input.CharacterId,
                IpAddress = input.IpAddress,
                LogData = input.LogData,
                LogId = input.LogId,
                LogType = input.LogType,
                Timestamp = input.Timestamp
            };
        }

        public IEnumerable<GeneralLogDTO> Map(IEnumerable<GeneralLog> input)
        {
            var result = new List<GeneralLogDTO>();

            foreach (var data in input)
            {
                result.Add(Map(data));
            }

            return result;
        }

        public IEnumerable<GeneralLog> Map(IEnumerable<GeneralLogDTO> input)
        {
            var result = new List<GeneralLog>();

            foreach (var data in input)
            {
                result.Add(Map(data));
            }

            return result;
        }
    }
}