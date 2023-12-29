using OpenNos.DAL.EF;
using OpenNos.Data;
using System.Collections.Generic;

namespace OpenNos.Mapper.Mappers
{
    public class FishingSpotsMapper
    {
        public static bool ToFishingSpots(FishingSpotsDTO input, FishingSpots output)
        {
            if (input == null)
            {
                return false;
            }

            output.Direction = input.Direction;
            output.Id = input.Id;
            output.MapId = input.MapId;
            output.MapX = input.MapX;
            output.MapY = input.MapY;
            output.MinLevel = input.MinLevel;

            return true;
        }

        public static bool ToFishingSpotsDTO(FishingSpots input, FishingSpotsDTO output)
        {
            if (input == null)
            {
                return false;
            }

            output.Direction = input.Direction;
            output.Id = input.Id;
            output.MapId = input.MapId;
            output.MapX = input.MapX;
            output.MapY = input.MapY;
            output.MinLevel = input.MinLevel;

            return true;
        }

        public FishingSpots Map(FishingSpotsDTO input)
        {
            if (input == null)
            {
                return null;
            }

            return new FishingSpots
            {
                Direction = input.Direction,
                Id = input.Id,
                MapId = input.MapId,
                MapX = input.MapX,
                MapY = input.MapY,
                MinLevel = input.MinLevel
            };
        }

        public FishingSpotsDTO Map(FishingSpots input)
        {
            if (input == null)
            {
                return null;
            }

            return new FishingSpotsDTO
            {
                Direction = input.Direction,
                Id = input.Id,
                MapId = input.MapId,
                MapX = input.MapX,
                MapY = input.MapY,
                MinLevel = input.MinLevel
            };
        }

        public IEnumerable<FishingSpotsDTO> Map(IEnumerable<FishingSpots> input)
        {
            var result = new List<FishingSpotsDTO>();

            foreach (var data in input)
            {
                result.Add(Map(data));
            }

            return result;
        }

        public IEnumerable<FishingSpots> Map(IEnumerable<FishingSpotsDTO> input)
        {
            var result = new List<FishingSpots>();

            foreach (var data in input)
            {
                result.Add(Map(data));
            }

            return result;
        }
    }
}
