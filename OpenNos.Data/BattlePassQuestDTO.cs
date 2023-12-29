using OpenNos.Domain;
using System;

namespace OpenNos.Data
{
    [Serializable]
    public class BattlePassQuestDTO
    {
        public long Id { get; set; }

        public BattlePassMissionType MissionType { get; set; }

        public BattlePassMissionSubType MissionSubType { get; set; }

        public BattlePassQuestType TaskType { get; set; }

        public short FirstData { get; set; }

        public long MinObjectiveValue { get; set; }

        public long MaxObjectiveValue { get; set; }

        public short Reward { get; set; }

        public DateTime Start { get; set; }
    }
}
