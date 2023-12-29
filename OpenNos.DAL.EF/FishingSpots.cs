using System.ComponentModel.DataAnnotations;

namespace OpenNos.DAL.EF
{
    public class FishingSpots
    {
        [Key]
        public long Id { get; set; }

        public short MapId { get; set; }

        public short MapX { get; set; }

        public short MapY { get; set; }

        public short Direction { get; set; }

        public short MinLevel { get; set; }
    }
}
