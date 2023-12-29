using OpenNos.DAL.EF;
using OpenNos.Data;

namespace OpenNos.Mapper.Mappers
{
    public static class BattlePassItemMapper
    {
        public static bool ToBattlePassItem(BattlePassItemDTO input, BattlePassItem output)
        {
            if (input == null) return false;

            output.ItemVNum = input.ItemVNum;
            output.Amount = input.Amount;
            output.IsSuperReward = input.IsSuperReward;
            output.IsPremium = input.IsPremium;
            output.Palier = input.Palier;

            return true;
        }

        public static bool ToBattlePassItemDTO(BattlePassItem input, BattlePassItemDTO output)
        {
            if (input == null) return false;

            output.ItemVNum = input.ItemVNum;
            output.Amount = input.Amount;
            output.IsSuperReward = input.IsSuperReward;
            output.IsPremium = input.IsPremium;
            output.Palier = input.Palier;

            return true;
        }
    }
}
