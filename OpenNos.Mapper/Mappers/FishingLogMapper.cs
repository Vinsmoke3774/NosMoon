using OpenNos.DAL.EF;
using OpenNos.Data;

namespace OpenNos.Mapper.Mappers
{
    public static class FishingLogMapper
    {
        public static bool ToFishingLog(FishingLogDTO input, FishingLog output)
        {
            if (input == null)
            {
                return false;
            }

            output.CharacterId = input.CharacterId;
            output.FishCount = input.FishCount;
            output.FishId = input.FishId;
            output.Id = input.Id;
            output.MaxLength = input.MaxLength;

            return true;
        }

        public static bool ToFishingLogDTO(FishingLog input, FishingLogDTO output)
        {
            if (input == null)
            {
                return false;
            }

            output.CharacterId = input.CharacterId;
            output.FishCount = input.FishCount;
            output.FishId = input.FishId;
            output.Id = input.Id;
            output.MaxLength = input.MaxLength;

            return true;
        }
    }
}
