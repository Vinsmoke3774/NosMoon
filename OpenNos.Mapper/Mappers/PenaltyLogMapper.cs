using System.Collections.Generic;
using OpenNos.DAL.EF;
using OpenNos.Data;

namespace OpenNos.Mapper.Mappers
{
    public class PenaltyLogMapper : IMapper<PenaltyLogDTO, PenaltyLog>
    {
        public PenaltyLog Map(PenaltyLogDTO input)
        {
            if (input == null)
            {
                return null;
            }

            return new PenaltyLog
            {
                AccountId = input.AccountId,
                IP = input.IP,
                AdminName = input.AdminName,
                DateEnd = input.DateEnd,
                DateStart = input.DateStart,
                Penalty = input.Penalty,
                PenaltyLogId = input.PenaltyLogId,
                Reason = input.Reason,
            };
        }

        public PenaltyLogDTO Map(PenaltyLog input)
        {
            if (input == null)
            {
                return null;
            }

            return new PenaltyLogDTO
            {
                AccountId = input.AccountId,
                IP = input.IP,
                AdminName = input.AdminName,
                DateEnd = input.DateEnd,
                DateStart = input.DateStart,
                Penalty = input.Penalty,
                PenaltyLogId = input.PenaltyLogId,
                Reason = input.Reason,
            };
        }

        public IEnumerable<PenaltyLogDTO> Map(IEnumerable<PenaltyLog> input)
        {
            var result = new List<PenaltyLogDTO>();

            foreach (var data in input)
            {
                result.Add(Map(data));
            }

            return result;
        }

        public IEnumerable<PenaltyLog> Map(IEnumerable<PenaltyLogDTO> input)
        {
            var result = new List<PenaltyLog>();

            foreach (var data in input)
            {
                result.Add(Map(data));
            }

            return result;
        }
    }
}