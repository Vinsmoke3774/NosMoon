using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.Data.Comparers
{
    public class ShellEffectDtoComparer : IEqualityComparer<ShellEffectDTO>
    {
        public bool Equals(ShellEffectDTO x, ShellEffectDTO y)
        {
            if (x == null || y == null)
            {
                return false;
            }
            return x.Effect == y.Effect && x.EffectLevel == y.EffectLevel && x.EquipmentSerialId.Equals(y.EquipmentSerialId) && x.IsRune == y.IsRune && x.Type == y.Type && x.Upgrade == y.Upgrade && x.Value == y.Value;
        }

        public int GetHashCode(ShellEffectDTO obj)
        {
            return base.GetHashCode();
        }
    }
}
