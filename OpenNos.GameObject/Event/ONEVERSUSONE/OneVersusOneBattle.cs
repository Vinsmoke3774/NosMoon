namespace OpenNos.GameObject.Event
{
    public class OneVersusOneBattle
    {
        #region Properties

        public bool IsDead { get; set; }

        public ClientSession Enemy { get; set; }

        public short Kills { get; set; }

        public string Name { get; set; }

        #endregion
    }
}