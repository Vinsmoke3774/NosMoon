using System;

namespace OpenNos.Data
{
    [Serializable]
    public class BattlePassItemLogsDTO
    {
        public Guid Id { get; set; }

        public long CharacterId { get; set; }

        public long Palier { get; set; }

        public bool IsPremium { get; set; }
    }
}
