using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.Data.Comparers
{
    public class CellonOptionDtoComparer : IEqualityComparer<CellonOptionDTO>
    {
        public bool Equals(CellonOptionDTO x, CellonOptionDTO y)
        {
            if (x == null || y == null)
            {
                return false;
            }

            return x.EquipmentSerialId == y.EquipmentSerialId &&
                   x.Type == y.Type &&
                   x.Value == y.Value &&
                   x.Level == y.Level;
        }

        public int GetHashCode(CellonOptionDTO obj)
        {
            return base.GetHashCode();
        }
    }
}
