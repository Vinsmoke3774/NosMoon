using System.Collections.Generic;
using System.Diagnostics;
using OpenNos.DAL.EF;
using OpenNos.Data;

namespace OpenNos.Mapper.Mappers
{
    public class ShellEffectMapper : IMapper<ShellEffectDTO, ShellEffect>
    {
        public ShellEffect Map(ShellEffectDTO input)
        {
            if (input == null)
            {
                return null;
            }

            return new ShellEffect
            {
                Effect = input.Effect,
                EffectLevel = input.EffectLevel,
                EquipmentSerialId = input.EquipmentSerialId,
                ShellEffectId = input.ShellEffectId,
                Value = input.Value,
                IsRune = input.IsRune,
                Type = input.Type,
                Upgrade = input.Upgrade,
            };
        }

        public ShellEffectDTO Map(ShellEffect input)
        {
            if (input == null)
            {
                return null;
            }

            return new ShellEffectDTO
            {
                Effect = input.Effect,
                EffectLevel = input.EffectLevel,
                EquipmentSerialId = input.EquipmentSerialId,
                ShellEffectId = input.ShellEffectId,
                Value = input.Value,
                IsRune = input.IsRune,
                Type = input.Type,
                Upgrade = input.Upgrade,
            };
        }

        public IEnumerable<ShellEffectDTO> Map(IEnumerable<ShellEffect> input)
        {
            var result = new List<ShellEffectDTO>();

            foreach (var data in input)
            {
                result.Add(Map(data));
            }

            return result;
        }

        public IEnumerable<ShellEffect> Map(IEnumerable<ShellEffectDTO> input)
        {
            var result = new List<ShellEffect>();

            foreach (var data in input)
            {
                result.Add(Map(data));
            }

            return result;
        }
    }
}