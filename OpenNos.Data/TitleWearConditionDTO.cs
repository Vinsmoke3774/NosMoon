using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.Data
{
    [Serializable]
    public class TitleWearConditionDTO
    {
        [Key]
        public long Id { get; set; }

        public long TitleVNum { get; set; }

        public long ConditionVNum { get; set; }

        public string Message { get; set; }
    }
}
