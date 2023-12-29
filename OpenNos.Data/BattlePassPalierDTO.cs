using System;

namespace OpenNos.Data
{
    [Serializable]
    public class BattlePassPalierDTO
    {
        public long Id { get; set; }

        public int MinimumBattlePassPoint { get; set; }

        public int MaximumBattlePassPoint { get; set; }
    }
}
