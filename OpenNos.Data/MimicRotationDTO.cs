using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNos.Domain;

namespace OpenNos.Data
{
    public class MimicRotationDTO
    {
        [Key]
        public long Id { get; set; }

        public int ItemVnum { get; set; }

        public short ItemAmount { get; set; }

        public double Percentage { get; set; }

        public MimicRotationType RotationType { get; set; }

        public bool IsSuperReward { get; set; }
    }
}
