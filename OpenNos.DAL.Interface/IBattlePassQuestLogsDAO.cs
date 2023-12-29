using OpenNos.Data;
using OpenNos.Data.Enums;
using System;
using System.Collections.Generic;

namespace OpenNos.DAL.Interface
{
    public interface IBattlePassQuestLogsDAO
    {
        DeleteResult Delete(Guid Id);

        IEnumerable<BattlePassQuestLogsDTO> LoadByCharactedId(long characterId);

        void InsertOrUpdateFromList(IEnumerable<BattlePassQuestLogsDTO> logs);
    }
}
