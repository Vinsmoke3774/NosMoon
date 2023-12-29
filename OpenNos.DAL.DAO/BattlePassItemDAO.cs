using OpenNos.DAL.EF.Helpers;
using OpenNos.DAL.Interface;
using OpenNos.Data;
using System.Collections.Generic;

namespace OpenNos.DAL.DAO
{
    public class BattlePassItemDAO : IBattlePassItemDAO
    {
        public List<BattlePassItemDTO> LoadAll()
        {
            using var context = DataAccessHelper.CreateContext();
            var result = new List<BattlePassItemDTO>();
            foreach (var battlePassItem in context.BattlePassItems)
            {
                var dto = new BattlePassItemDTO();
                Mapper.Mappers.BattlePassItemMapper.ToBattlePassItemDTO(battlePassItem, dto);
                result.Add(dto);
            }
            return result;
        }
    }
}
