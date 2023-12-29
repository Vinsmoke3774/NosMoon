using OpenNos.Data;
using OpenNos.Data.Enums;
using System.Collections.Generic;

namespace OpenNos.DAL.Interface
{
    public interface IQuestLogDAO
    {
        #region Methods

        SaveResult InsertOrUpdate(ref QuestLogDTO questLog);

        SaveResult InsertOrUpdateFromList(IEnumerable<QuestLogDTO> dtos);

        DeleteResult DeleteByCharacterId(long characterId);

        IEnumerable<QuestLogDTO> LoadByCharacterId(long id);

        QuestLogDTO LoadById(long id);

        #endregion
    }
}