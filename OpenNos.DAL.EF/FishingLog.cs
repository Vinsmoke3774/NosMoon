using System.ComponentModel.DataAnnotations;

namespace OpenNos.DAL.EF
{
    public class FishingLog
    {
        [Key]
        public long Id { get; set; }

        public long CharacterId { get; set; }

        public short FishId { get; set; }

        public int FishCount { get; set; }

        public int MaxLength { get; set; }
    }
}
