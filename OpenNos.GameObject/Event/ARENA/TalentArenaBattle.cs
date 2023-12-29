using OpenNos.Domain;

namespace OpenNos.GameObject.Event
{
    public class TalentArenaBattle
    {
        #region Properties

        public MapInstance MapInstance { get; set; }

        public byte Side { get; set; }

        public byte Round { get; set; }

        public bool IsDead { get; set; }

        public ArenaTeamType ArenaTeamType { get; set; }

        public ClientSession Opponent { get; set; }

        public byte RoundWin { get; set; }

        public bool TieBreaker { get; set; }

        #endregion
    }
}