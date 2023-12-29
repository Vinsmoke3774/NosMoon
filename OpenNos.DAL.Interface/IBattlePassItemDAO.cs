using OpenNos.Data;
using System.Collections.Generic;

namespace OpenNos.DAL.Interface
{
    public interface IBattlePassItemDAO
    {
        List<BattlePassItemDTO> LoadAll();
    }
}
