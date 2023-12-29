using OpenNos.Core;
using OpenNos.Core.Logger;
using OpenNos.DAL.EF;
using OpenNos.DAL.EF.Helpers;
using OpenNos.DAL.Interface;
using OpenNos.Data;
using OpenNos.Data.Enums;
using OpenNos.Mapper.Mappers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenNos.DAL.DAO
{
    public class BattlePassQuestLogsDAO : IBattlePassQuestLogsDAO
    {
        public DeleteResult Delete(Guid id)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                var account = context.BattlePassQuestLogs.FirstOrDefault(c => c.Id.Equals(id));

                if (account != null)
                {
                    context.BattlePassQuestLogs.Remove(account);
                    context.SaveChanges();
                }

                return DeleteResult.Deleted;
            }
            catch (Exception e)
            {
                Logger.Log.Error(
                    string.Format(Language.Instance.GetMessageFromKey("DELETE_ACCOUNT_ERROR"), id, e.Message),
                    e);
                return DeleteResult.Error;
            }
        }

        public IEnumerable<BattlePassQuestLogsDTO> LoadByCharactedId(long characterId)
        {
            using var context = DataAccessHelper.CreateContext();
            var result = new List<BattlePassQuestLogsDTO>();
            foreach (var entity in context.BattlePassQuestLogs.Where(s => s.CharacterId == characterId))
            {
                var dto = new BattlePassQuestLogsDTO();
                BattlePassQuestLogsMapper.ToBattlePassQuestLogsDTO(entity, dto);
                result.Add(dto);
            }
            return result;
        }
        public void InsertOrUpdateFromList(IEnumerable<BattlePassQuestLogsDTO> logs)
        {
            using var context = DataAccessHelper.CreateContext();

            foreach (var dto in logs)
            {
                var entity = new BattlePassQuestLogs();
                BattlePassQuestLogsMapper.ToBattlePassQuestLogs(dto, entity);

                context.BattlePassQuestLogs.AddOrUpdateExtension(entity);
            }

            context.SaveChanges();
        }
    }
}
