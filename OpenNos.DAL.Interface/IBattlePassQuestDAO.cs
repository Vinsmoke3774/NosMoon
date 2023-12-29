using OpenNos.Data;
using System.Collections.Generic;

namespace OpenNos.DAL.Interface
{
    public interface IBattlePassQuestDAO
    {
        List<BattlePassQuestDTO> LoadAll();
    }
}
