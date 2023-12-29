using OpenNos.DAL.EF;
using OpenNos.Data;

namespace OpenNos.Mapper.Mappers
{
    public static class BattlePassItemLogsMapper
    {
        public static bool ToBattlePassItemLogs(BattlePassItemLogsDTO input, BattlePassItemLogs output)
        {
            if (input == null) return false;

            output.Id = input.Id;
            output.CharacterId = input.CharacterId;
            output.Palier = input.Palier;
            output.IsPremium = input.IsPremium;

            return true;
        }

        public static bool ToBattlePassItemLogsDTO(BattlePassItemLogs input, BattlePassItemLogsDTO output)
        {
            if (input == null) return false;

            output.Id = input.Id;
            output.CharacterId = input.CharacterId;
            output.Palier = input.Palier;
            output.IsPremium = input.IsPremium;

            return true;
        }
    }
}
