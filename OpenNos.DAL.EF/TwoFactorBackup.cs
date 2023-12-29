using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.DAL.EF
{
    public class TwoFactorBackup : SynchronizableBaseEntity
    {
        public long AccountId { get; set; }

        public string Code { get; set; }
    }
}
