using System;

namespace OpenNos.Data
{
    [Serializable]
    public class BattlePassItemDTO
    {
        public short ItemVNum { get; set; }

        public short Amount { get; set; }

        public bool IsSuperReward { get; set; }

        public bool IsPremium { get; set; }

        public long Palier { get; set; }
    }
}
