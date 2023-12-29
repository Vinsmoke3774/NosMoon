using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using OpenNos.DAL.EF;
using OpenNos.Data;

namespace OpenNos.Mapper.Mappers
{
    public class QuestLogMapper : IMapper<QuestLogDTO, QuestLog>
    {
        public QuestLog Map(QuestLogDTO input)
        {
            if (input == null)
            {
                return null;
            }

            return new QuestLog
            {
                CharacterId = input.CharacterId,
                QuestId = input.QuestId,
                IpAddress = input.IpAddress,
                LastDaily = input.LastDaily,
                Id = input.Id
            };
        }

        public QuestLogDTO Map(QuestLog input)
        {
            if (input == null)
            {
                return null;
            }

            return new QuestLogDTO
            {
                CharacterId = input.CharacterId,
                QuestId = input.QuestId,
                IpAddress = input.IpAddress,
                LastDaily = input.LastDaily,
                Id = input.Id
            };
        }

        public IEnumerable<QuestLogDTO> Map(IEnumerable<QuestLog> input)
        {
            var result = new List<QuestLogDTO>();

            foreach (var data in input)
            {
                result.Add(Map(data));
            }

            return result;
        }

        public IEnumerable<QuestLog> Map(IEnumerable<QuestLogDTO> input)
        {
            var result = new List<QuestLog>();

            foreach (var data in input)
            {
                result.Add(Map(data));
            }

            return result;
        }
    }
}