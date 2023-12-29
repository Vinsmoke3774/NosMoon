using OpenNos.Data;
using OpenNos.Data.Enums;
using System.Collections.Generic;

namespace OpenNos.DAL.Interface
{
    public interface IFishInfoDAO
    {
        SaveResult InsertOrUpdateFromList(List<FishInfoDTO> fishes);

        IEnumerable<FishInfoDTO> LoadAll();
    }
}
