using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.Data
{
    public class TwoFactorBackupDTO : SynchronizableBaseDTO
    {
        public long AccountId { get; set; }

        public string Code { get; set; }
    }
}
