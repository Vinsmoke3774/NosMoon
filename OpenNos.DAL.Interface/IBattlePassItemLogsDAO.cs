using OpenNos.Data;
using OpenNos.Data.Enums;
using System.Collections.Generic;

namespace OpenNos.DAL.Interface
{
    public interface IBattlePassItemLogsDAO
    {
        IEnumerable<BattlePassItemLogsDTO> LoadByCharactedId(long characterId);

        SaveResult InsertOrUpdateFromList(IEnumerable<BattlePassItemLogsDTO> logs);
    }
}
