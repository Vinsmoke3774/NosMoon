using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNos.DAL.EF;
using OpenNos.Data;

namespace OpenNos.Mapper.Mappers
{
    public class MimicRotationMapper : IMapper<MimicRotationDTO, MimicRotation>
    {
        public MimicRotation Map(MimicRotationDTO input)
        {
            if (input == null)
            {
                return null;
            }

            return new MimicRotation
            {
                IsSuperReward = input.IsSuperReward,
                ItemAmount = input.ItemAmount,
                ItemVnum = input.ItemVnum,
                Percentage = input.Percentage,
                RotationType = input.RotationType,
                Id = input.Id
            };
        }

        public MimicRotationDTO Map(MimicRotation input)
        {
            if (input == null)
            {
                return null;
            }

            return new MimicRotationDTO
            {
                IsSuperReward = input.IsSuperReward,
                ItemAmount = input.ItemAmount,
                ItemVnum = input.ItemVnum,
                Percentage = input.Percentage,
                RotationType = input.RotationType,
                Id = input.Id
            };
        }

        public IEnumerable<MimicRotationDTO> Map(IEnumerable<MimicRotation> input)
        {
            var result = new List<MimicRotationDTO>();

            foreach (var data in input)
            {
                result.Add(Map(data));
            }

            return result;
        }

        public IEnumerable<MimicRotation> Map(IEnumerable<MimicRotationDTO> input)
        {
            var result = new List<MimicRotation>();

            foreach (var data in input)
            {
                result.Add(Map(data));
            }

            return result;
        }
    }
}
