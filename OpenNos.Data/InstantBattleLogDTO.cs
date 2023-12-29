using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.Data
{
    public class InstantBattleLogDTO
    {
        [Key]
        public long Id { get; set; }

        public long CharacterId { get; set; }

        public DateTime DateTime { get; set; }
    }
}
