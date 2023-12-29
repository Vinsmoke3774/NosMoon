using System;

namespace OpenNos.Data
{
    [Serializable]
    public class FishInfoDTO
    {
        public long Id { get; set; }

        public short FishVNum { get; set; }

        public short Probability { get; set; }

        public short MapId1 { get; set; }

        public short MapId2 { get; set; }

        public short MapId3 { get; set; }

        public float MinFishLength { get; set; }

        public float MaxFishLength { get; set; }

        public bool IsFish { get; set; }
    }
}
