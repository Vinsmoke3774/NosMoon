using OpenNos.Data;
using OpenNos.Data.Enums;
using System.Collections.Generic;

namespace OpenNos.DAL.Interface
{
    public interface ICharacterTimespaceLogDAO
    {
        IEnumerable<CharacterTimespaceLogDTO> LoadByCharactedId(long characterId);

        SaveResult InsertOrUpdateFromList(IEnumerable<CharacterTimespaceLogDTO> logs);

        CharacterTimespaceLogDTO GetHighestScoreByScriptedInstanceId(long scriptedInstanceId);

        SaveResult InsertOrUpdate(CharacterTimespaceLogDTO card);

        List<CharacterTimespaceLogDTO> LoadAll();
    }
}
