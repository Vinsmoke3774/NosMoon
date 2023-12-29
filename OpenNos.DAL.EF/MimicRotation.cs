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
    public class MimicRotation
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long Id { get; set; }

        public int ItemVnum { get; set; }

        public short ItemAmount { get; set; }

        public double Percentage { get; set; }

        public MimicRotationType RotationType { get; set; }

        public bool IsSuperReward { get; set; }
    }
}
