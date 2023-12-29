using OpenNos.Domain;
using System;

namespace OpenNos.GameObject
{
    public class KingOfTheHillCharacter
    {
        public Guid MapInstanceId { get; set; }

        public bool IsQueen { get; set; }

        public bool IsKing { get; set; }

        public KingOfTheHillTeamType TeamType { get; set; }

        public long Points { get; set; }
    }
}
