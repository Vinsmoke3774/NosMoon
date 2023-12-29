using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNos.Domain;

namespace OpenNos.GameObject
{
    public class ExchangeListItem
    {
        public InventoryType Type { get; set; }

        public short Slot { get; set; }

        public short Quantity { get; set; }
    }
}
