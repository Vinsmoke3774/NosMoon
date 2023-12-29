using OpenNos.DAL.EF;
using OpenNos.Data;

namespace OpenNos.Mapper.Mappers
{
    public static class BattlePassPalierMapper
    {
        public static bool ToBattlePassPalier(BattlePassPalierDTO input, BattlePassPalier output)
        {
            if (input == null) return false;

            output.Id = input.Id;
            output.MinimumBattlePassPoint = input.MinimumBattlePassPoint;
            output.MaximumBattlePassPoint = input.MaximumBattlePassPoint;

            return true;
        }

        public static bool ToBattlePassPalierDTO(BattlePassPalier input, BattlePassPalierDTO output)
        {
            if (input == null) return false;

            output.Id = input.Id;
            output.MinimumBattlePassPoint = input.MinimumBattlePassPoint;
            output.MaximumBattlePassPoint = input.MaximumBattlePassPoint;

            return true;
        }
    }
}
