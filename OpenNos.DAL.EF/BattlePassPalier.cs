using System.ComponentModel.DataAnnotations;

namespace OpenNos.DAL.EF
{
    public class BattlePassPalier
    {
        [Key]
        public long Id { get; set; }

        public int MinimumBattlePassPoint { get; set; }

        public int MaximumBattlePassPoint { get; set; }
    }
}
