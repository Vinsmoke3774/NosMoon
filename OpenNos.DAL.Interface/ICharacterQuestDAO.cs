using OpenNos.Data;
using System.Collections.Generic;
using OpenNos.Data.Enums;
using System;
using System.Threading.Tasks;

namespace OpenNos.DAL.Interface
{
    public interface ICharacterQuestDAO
    {
        #region Methods

        DeleteResult DeleteFromList(long characterId, IEnumerable<long> questIds);

        SaveResult InsertOrUpdateFromList(IEnumerable<CharacterQuestDTO> quests);

        IEnumerable<CharacterQuestDTO> LoadByCharacterId(long characterId);

        DeleteResult DeleteForCharacterId(long characterId);

        #endregion
    }
}