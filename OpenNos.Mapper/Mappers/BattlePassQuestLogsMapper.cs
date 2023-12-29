using OpenNos.DAL.EF;
using OpenNos.Data;

namespace OpenNos.Mapper.Mappers
{
    public static class BattlePassQuestLogsMapper
    {
        public static bool ToBattlePassQuestLogs(BattlePassQuestLogsDTO input, BattlePassQuestLogs output)
        {
            if (input == null) return false;

            output.Id = input.Id;
            output.CharacterId = input.CharacterId;
            output.QuestId = input.QuestId;
            output.Advancement = input.Advancement;
            output.Type = input.Type;
            output.AlreadyTaken = input.AlreadyTaken;

            return true;
        }

        public static bool ToBattlePassQuestLogsDTO(BattlePassQuestLogs input, BattlePassQuestLogsDTO output)
        {
            if (input == null) return false;

            output.Id = input.Id;
            output.CharacterId = input.CharacterId;
            output.QuestId = input.QuestId;
            output.Advancement = input.Advancement;
            output.Type = input.Type;
            output.AlreadyTaken = input.AlreadyTaken;

            return true;
        }
    }
}
