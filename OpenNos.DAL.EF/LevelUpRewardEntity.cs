using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNos.Domain;

namespace OpenNos.DAL.EF
{
    [Table("LevelUpRewards")]
    public class LevelUpRewardEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public LevelupRewardType Type { get; set; }

        public int Value { get; set; }

        public short RequiredLevel { get; set; }

        public short Amount { get; set; }
    }
}
