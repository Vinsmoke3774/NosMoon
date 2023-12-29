using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.GameObject
{
    public class BazaarActionTimer
    {
        public DateTime LastBuyAction { get; set; }

        public DateTime LastGetAction { get; set; }

        public DateTime LastModAction { get; set; }
    }
}
