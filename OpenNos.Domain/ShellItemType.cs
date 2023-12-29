using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.Domain
{
    public enum ShellItemType : byte
    {
        FullShellWeapon = 0,
        FullShellArmor = 1,
        SpecialShellWeapon = 2,
        SpecialShellArmor = 3,
        PvpShellWeapon = 4,
        PvpShellArmor = 5,
        PerfectShellWeapon = 6,
        PerfectShellArmor = 7,
        HalfShellWeapon = 8,
        HalfShellArmor = 9,
        CustomChampionShellWeapon = 10,
        CustomChampionShellArmor = 11
    }
}
