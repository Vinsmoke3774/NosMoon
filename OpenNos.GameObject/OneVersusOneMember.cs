using OpenNos.Domain;

namespace OpenNos.GameObject
{
    public class OneVersusOneMember
    {
        #region Properties

        public EventType OneVersusOneType { get; set; }

        public byte Kills { get; set; }

        public bool IsDead { get; set; }

        public bool IsWaiting { get; set; }

        public ClientSession Session { get; set; }

        #endregion
    }
}