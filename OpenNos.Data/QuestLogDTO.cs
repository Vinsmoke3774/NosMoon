using System;
using System.ComponentModel.DataAnnotations;

namespace OpenNos.Data
{
    [Serializable]
    public class QuestLogDTO
    {
        #region Properties

        public long CharacterId { get; set; }

        [Key]
        public long Id { get; set; }

        public string IpAddress { get; set; }

        public DateTime? LastDaily { get; set; }

        public long QuestId { get; set; }

        #endregion
    }
}