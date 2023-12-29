namespace OpenNos.GameObject
{
    public class DeathMatchMember
    {
        #region Properties

        public byte Kills { get; set; }

        public bool IsDead { get; set; }

        public bool IsWaiting { get; set; }

        public ClientSession Session { get; set; }

        #endregion
    }
}