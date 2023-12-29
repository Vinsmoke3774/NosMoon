using System.Collections.Generic;
using OpenNos.DAL.EF;
using OpenNos.Data;

namespace OpenNos.Mapper.Mappers
{
    public class DropMapper : IMapper<DropDTO, Drop>
    {
        public Drop Map(DropDTO input)
        {
            if (input == null)
            {
                return null;
            }

            return new Drop
            {
                Amount = input.Amount,
                DropChance = input.DropChance,
                DropId = input.DropId,
                ItemVNum = input.ItemVNum,
                MapTypeId = input.MapTypeId,
                MonsterVNum = input.MonsterVNum
            };
        }

        public DropDTO Map(Drop input)
        {
            if (input == null)
            {
                return null;
            }

            return new DropDTO
            {
                Amount = input.Amount,
                DropChance = input.DropChance,
                DropId = input.DropId,
                ItemVNum = input.ItemVNum,
                MapTypeId = input.MapTypeId,
                MonsterVNum = input.MonsterVNum
            };
        }

        public IEnumerable<DropDTO> Map(IEnumerable<Drop> input)
        {
            var result = new List<DropDTO>();

            foreach (var data in input)
            {
                result.Add(Map(data));
            }

            return result;
        }

        public IEnumerable<Drop> Map(IEnumerable<DropDTO> input)
        {
            var result = new List<Drop>();

            foreach (var data in input)
            {
                result.Add(Map(data));
            }

            return result;
        }
    }
}