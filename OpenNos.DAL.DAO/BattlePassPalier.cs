using OpenNos.DAL.EF.Helpers;
using OpenNos.DAL.Interface;
using OpenNos.Data;
using System.Collections.Generic;

namespace OpenNos.DAL.DAO
{
    public class BattlePassPalierDAO : IBattlePassPalierDAO
    {
        public List<BattlePassPalierDTO> LoadAll()
        {
            using var context = DataAccessHelper.CreateContext();
            var result = new List<BattlePassPalierDTO>();
            foreach (var battlePassPalier in context.BattlePassPaliers)
            {
                var dto = new BattlePassPalierDTO();
                Mapper.Mappers.BattlePassPalierMapper.ToBattlePassPalierDTO(battlePassPalier, dto);
                result.Add(dto);
            }
            return result;
        }
    }
}
