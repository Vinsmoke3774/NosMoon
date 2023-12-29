using OpenNos.Data;
using OpenNos.Data.Enums;
using System.Collections.Generic;

namespace OpenNos.DAL.Interface
{
    public interface IFishingLogDAO
    {
        IEnumerable<FishingLogDTO> LoadByCharacterId(long characterId);

        SaveResult InsertOrUpdateFromList(IEnumerable<FishingLogDTO> logs);
    }
}
