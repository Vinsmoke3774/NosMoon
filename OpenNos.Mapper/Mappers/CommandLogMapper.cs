using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNos.DAL.EF;
using OpenNos.Data;

namespace OpenNos.Mapper.Mappers
{
    public class CommandLogMapper : IMapper<CommandLogDTO, CommandLog>
    {
        public CommandLog Map(CommandLogDTO input)
        {
            if (input == null)
            {
                return null;
            }

            return new CommandLog
            {
                CharacterId = input.CharacterId,
                AccountId = input.AccountId,
                Command = input.Command,
                DateTime = input.DateTime,
                Name = input.Name
            };
        }

        public CommandLogDTO Map(CommandLog input)
        {
            if (input == null)
            {
                return null;
            }

            return new CommandLogDTO
            {
                CharacterId = input.CharacterId,
                AccountId = input.AccountId,
                Command = input.Command,
                DateTime = input.DateTime,
                Name = input.Name
            };
        }

        public IEnumerable<CommandLogDTO> Map(IEnumerable<CommandLog> input)
        {
            var result = new List<CommandLogDTO>();

            foreach (var data in input)
            {
                result.Add(Map(data));
            }

            return result;
        }

        public IEnumerable<CommandLog> Map(IEnumerable<CommandLogDTO> input)
        {
            var result = new List<CommandLog>();

            foreach (var data in input)
            {
                result.Add(Map(data));
            }

            return result;
        }
    }
}
