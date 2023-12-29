using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNos.Data;
using OpenNos.Data.Enums;

namespace OpenNos.DAL.Interface
{
    public interface IInstantBattleLogDAO
    {
        IEnumerable<InstantBattleLogDTO> LoadByCharacterId(long characterId);

        SaveResult InsertOrUpdateFromList(IEnumerable<InstantBattleLogDTO> logs);

        DeleteResult DeleteByCharacterId(long characterId);
    }
}
