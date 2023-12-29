using OpenNos.DAL.EF.Helpers;
using OpenNos.DAL.Interface;
using OpenNos.Data;
using System.Collections.Generic;

namespace OpenNos.DAL.DAO
{
    public class BattlePassQuestDAO : IBattlePassQuestDAO
    {
        public List<BattlePassQuestDTO> LoadAll()
        {
            using var context = DataAccessHelper.CreateContext();
            var result = new List<BattlePassQuestDTO>();
            foreach (var battlePassPalier in context.BattlePassQuest)
            {
                var dto = new BattlePassQuestDTO();
                Mapper.Mappers.BattlePassQuestMapper.ToBattlePassQuestDTO(battlePassPalier, dto);
                result.Add(dto);
            }
            return result;
        }
    }
}
