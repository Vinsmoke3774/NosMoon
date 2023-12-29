using System;

namespace OpenNos.Data
{
    [Serializable]
    public class CharacterQuestDTO : SynchronizableBaseDTO
    {
        #region Properties

        public long CharacterId { get; set; }

        public int FifthObjective { get; set; }

        public int FirstObjective { get; set; }

        public int FourthObjective { get; set; }

        public bool IsMainQuest { get; set; }

        public long QuestId { get; set; }

        public int SecondObjective { get; set; }

        public int ThirdObjective { get; set; }

        public override bool Equals(object obj)
        {
            if (!(obj is CharacterQuestDTO characterQuest))
            {
                return false;
            }

            // That should do it
            return CharacterId == characterQuest.CharacterId && QuestId == characterQuest.QuestId;
        }

        #endregion
    }
}