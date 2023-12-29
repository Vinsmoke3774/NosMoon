using OpenNos.Domain;

namespace OpenNos.GameObject
{
    public class ArenaMember
    {
        #region Properties

        public EventType ArenaType { get; set; }

        public bool IsDead { get; set; }

        public byte RoundWin { get; set; }

        public long? GroupId { get; set; }

        public ClientSession Session { get; set; }

        public short Time { get; set; }

        #endregion
    }
}