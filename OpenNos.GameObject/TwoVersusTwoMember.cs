namespace OpenNos.GameObject
{
    public class TwoVersusTwoMember
    {
        public long GroupedId { get; set; }

        public bool IsDead { get; set; }

        public long EnemyGroupId { get; set; }

        public bool IsWaiting { get; set; } = true;

        public long CharacterId { get; set; }
    }
}