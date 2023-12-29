using System;
using System.ComponentModel.DataAnnotations;

namespace OpenNos.Data
{
    [Serializable]
    public class FishingSpotsDTO
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
