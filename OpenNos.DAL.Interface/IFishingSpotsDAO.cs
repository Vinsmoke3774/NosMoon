using OpenNos.Data;
using OpenNos.Data.Enums;
using System.Collections.Generic;

namespace OpenNos.DAL.Interface
{
    public interface IFishingSpotsDAO
    {
        SaveResult InsertOrUpdateFromList(List<FishingSpotsDTO> spots);

        IEnumerable<FishingSpotsDTO> LoadAll();
    }
}
