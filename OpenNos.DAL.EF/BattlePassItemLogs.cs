namespace OpenNos.DAL.EF
{
    public class BattlePassItemLogs : SynchronizableBaseEntity
    {
        public long CharacterId { get; set; }

        public long Palier { get; set; }

        public bool IsPremium { get; set; }
    }
}
