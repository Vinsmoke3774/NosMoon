namespace OpenNos.DAL.EF
{
    public class BattlePassItem : SynchronizableBaseEntity
    {
        public short ItemVNum { get; set; }

        public short Amount { get; set; }

        public bool IsSuperReward { get; set; }

        public bool IsPremium { get; set; }

        public long Palier { get; set; }
    }
}
