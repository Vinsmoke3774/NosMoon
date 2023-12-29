using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNos.Core;

namespace OpenNos.Data
{
    public class CommandLogDTO
    {
        public Guid Id { get; set; }

        public string Command { get; set; }

        public long CharacterId { get; set; }

        public string Name { get; set; }

        public long AccountId { get; set; }

        public DateTime DateTime { get; set; }
    }
}
