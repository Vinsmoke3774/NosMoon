using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.DAL.EF.Comparers
{
    public class CellonOptionComparer : IEqualityComparer<CellonOption>
    {
        public bool Equals(CellonOption x, CellonOption y)
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

        public int GetHashCode(CellonOption obj)
        {
            return base.GetHashCode();
        }
    }
}
