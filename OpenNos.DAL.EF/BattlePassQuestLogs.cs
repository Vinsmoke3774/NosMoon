using OpenNos.Domain;

namespace OpenNos.DAL.EF
{
    public class BattlePassQuestLogs : SynchronizableBaseEntity
    {
        public long CharacterId { get; set; }

        public long QuestId { get; set; }

        public long Advancement { get; set; }

        public BattlePassQuestType Type { get; set; }

        public bool AlreadyTaken { get; set; }
    }
}
