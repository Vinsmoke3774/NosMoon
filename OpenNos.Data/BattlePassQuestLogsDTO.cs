using OpenNos.Domain;
using System;

namespace OpenNos.Data
{
    [Serializable]
    public class BattlePassQuestLogsDTO
    {
        public Guid Id { get; set; }

        public long CharacterId { get; set; }

        public long QuestId { get; set; }

        public long Advancement { get; set; }

        public BattlePassQuestType Type { get; set; }

        public bool AlreadyTaken { get; set; }
    }
}
