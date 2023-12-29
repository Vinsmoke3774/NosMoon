using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNos.Domain;

namespace OpenNos.Data
{
    [Serializable]
    public class LevelUpRewardDTO
    {
        public long Id { get; set; }

        public LevelupRewardType Type { get; set; }

        public int Value { get; set; }

        public short RequiredLevel { get; set; }

        public short Amount { get; set; }
    }
}
