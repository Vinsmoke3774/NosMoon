using System;

namespace OpenNos.Data
{
    [Serializable]
    public class CharacterTitleDTO
    {
        #region Properties

        public long CharacterId { get; set; }

        public long CharacterTitleId { get; set; }

        public byte Stat { get; set; }

        public long TitleVnum { get; set; }

        #endregion
    }
}