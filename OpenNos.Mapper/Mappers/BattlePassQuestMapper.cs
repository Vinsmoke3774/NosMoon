using OpenNos.DAL.EF;
using OpenNos.Data;

namespace OpenNos.Mapper.Mappers
{
    public static class BattlePassQuestMapper
    {
        public static bool ToBattlePassQuest(BattlePassQuestDTO input, BattlePassQuest output)
        {
            if (input == null) return false;

            output.Id = input.Id;
            output.MissionType = input.MissionType;
            output.MissionSubType = input.MissionSubType;
            output.TaskType = input.TaskType;
            output.FirstData = input.FirstData;
            output.MinObjectiveValue = input.MinObjectiveValue;
            output.MaxObjectiveValue = input.MaxObjectiveValue;
            output.Reward = input.Reward;
            output.Start = input.Start;

            return true;
        }

        public static bool ToBattlePassQuestDTO(BattlePassQuest input, BattlePassQuestDTO output)
        {
            if (input == null) return false;

            output.Id = input.Id;
            output.MissionType = input.MissionType;
            output.MissionSubType = input.MissionSubType;
            output.TaskType = input.TaskType;
            output.FirstData = input.FirstData;
            output.MinObjectiveValue = input.MinObjectiveValue;
            output.MaxObjectiveValue = input.MaxObjectiveValue;
            output.Reward = input.Reward;
            output.Start = input.Start;

            return true;
        }
    }
}
