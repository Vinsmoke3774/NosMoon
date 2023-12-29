using System.Collections.Generic;
using OpenNos.DAL.EF.Entities;
using OpenNos.Data;

namespace OpenNos.Mapper.Mappers
{
    public class MateMapper : IMapper<MateDTO, Mate>
    {
        public Mate Map(MateDTO input)
        {
            if (input == null)
            {
                return null;
            }

            return new Mate
            {
                Attack = input.Attack,
                CanPickUp = input.CanPickUp,
                CharacterId = input.CharacterId,
                Defence = input.Defence,
                Direction = input.Direction,
                Experience = input.Experience,
                Hp = input.Hp,
                IsSummonable = input.IsSummonable,
                IsTeamMember = input.IsTeamMember,
                Level = input.Level,
                Loyalty = input.Loyalty,
                MapX = input.MapX,
                MapY = input.MapY,
                MateId = input.MateId,
                MateType = input.MateType,
                Mp = input.Mp,
                Name = input.Name,
                NpcMonsterVNum = input.NpcMonsterVNum,
                Skin = input.Skin
            };
        }

        public MateDTO Map(Mate input)
        {
            if (input == null)
            {
                return null;
            }

            return new MateDTO
            {
                Attack = input.Attack,
                CanPickUp = input.CanPickUp,
                CharacterId = input.CharacterId,
                Defence = input.Defence,
                Direction = input.Direction,
                Experience = input.Experience,
                Hp = input.Hp,
                IsSummonable = input.IsSummonable,
                IsTeamMember = input.IsTeamMember,
                Level = input.Level,
                Loyalty = input.Loyalty,
                MapX = input.MapX,
                MapY = input.MapY,
                MateId = input.MateId,
                MateType = input.MateType,
                Mp = input.Mp,
                Name = input.Name,
                NpcMonsterVNum = input.NpcMonsterVNum,
                Skin = input.Skin
            };
        }

        public IEnumerable<MateDTO> Map(IEnumerable<Mate> input)
        {
            var result = new List<MateDTO>();

            foreach (var data in input)
            {
                result.Add(Map(data));
            }

            return result;
        }

        public IEnumerable<Mate> Map(IEnumerable<MateDTO> input)
        {
            var result = new List<Mate>();

            foreach (var data in input)
            {
                result.Add(Map(data));
            }

            return result;
        }
    }
}