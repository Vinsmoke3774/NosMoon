using System;

namespace OpenNos.Data
{
    [Serializable]
    public class FishingLogDTO
    {
        public long Id { get; set; }

        public long CharacterId { get; set; }

        public short FishId { get; set; }

        public int FishCount { get; set; }

        public int MaxLength { get; set; }
    }
}
