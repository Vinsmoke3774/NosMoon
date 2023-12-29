using OpenNos.DAL.EF;
using OpenNos.Data;

namespace OpenNos.Mapper.Mappers
{
    public static class FishInfoMapper
    {
        public static bool ToFishInfo(FishInfoDTO input, FishInfo output)
        {
            if (input == null)
            {
                return false;
            }

            output.Id = input.Id;
            output.FishVNum = input.FishVNum;
            output.MapId1 = input.MapId1;
            output.MapId2 = input.MapId2;
            output.MapId3 = input.MapId3;
            output.MaxFishLength = input.MaxFishLength;
            output.MinFishLength = input.MinFishLength;
            output.Probability = input.Probability;
            output.IsFish = input.IsFish;

            return true;
        }

        public static bool ToFishInfoDTO(FishInfo input, FishInfoDTO output)
        {
            if (input == null)
            {
                return false;
            }

            output.Id = input.Id;
            output.FishVNum = input.FishVNum;
            output.MapId1 = input.MapId1;
            output.MapId2 = input.MapId2;
            output.MapId3 = input.MapId3;
            output.MaxFishLength = input.MaxFishLength;
            output.MinFishLength = input.MinFishLength;
            output.Probability = input.Probability;
            output.IsFish = input.IsFish;

            return true;
        }
    }
}