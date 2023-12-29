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
    public class BattlePassItemLogsDAO : IBattlePassItemLogsDAO
    {
        public IEnumerable<BattlePassItemLogsDTO> LoadByCharactedId(long characterId)
        {
            using var context = DataAccessHelper.CreateContext();
            var result = new List<BattlePassItemLogsDTO>();
            foreach (var entity in context.BattlePassItemLogs.Where(s => s.CharacterId == characterId))
            {
                var dto = new BattlePassItemLogsDTO();
                BattlePassItemLogsMapper.ToBattlePassItemLogsDTO(entity, dto);
                result.Add(dto);
            }
            return result;
        }
        public SaveResult InsertOrUpdateFromList(IEnumerable<BattlePassItemLogsDTO> logs)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                context.Configuration.AutoDetectChangesEnabled = false;
                foreach (var card in logs)
                {
                    InsertOrUpdate(card);
                }
                context.Configuration.AutoDetectChangesEnabled = true;
                context.SaveChanges();
                return SaveResult.Inserted;
            }
            catch (Exception e)
            {
                Logger.Log.Error("InsertOrUpdateFromList", e);
                return SaveResult.Error;
            }
        }

        public SaveResult InsertOrUpdate(BattlePassItemLogsDTO card)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                var entity = context.BattlePassItemLogs.FirstOrDefault(c => c.CharacterId == card.CharacterId && c.IsPremium == card.IsPremium && c.Palier == card.Palier);

                if (entity == null)
                {
                    card = insert(card, context);
                    return SaveResult.Inserted;
                }

                //card = update(entity, card, context); :)
                return SaveResult.Updated;
            }
            catch (Exception e)
            {
                Logger.Log.Error(string.Format(Language.Instance.GetMessageFromKey("UPDATE_CARD_ERROR"), 0, e.Message), e);
                return SaveResult.Error;
            }
        }

        private static BattlePassItemLogsDTO insert(BattlePassItemLogsDTO card, OpenNosContext context)
        {
            var entity = new BattlePassItemLogs();
            BattlePassItemLogsMapper.ToBattlePassItemLogs(card, entity);
            context.BattlePassItemLogs.Add(entity);
            context.SaveChanges();
            if (BattlePassItemLogsMapper.ToBattlePassItemLogsDTO(entity, card))
            {
                return card;
            }

            return null;
        }

        private static BattlePassItemLogsDTO update(BattlePassItemLogs entity, BattlePassItemLogsDTO card, OpenNosContext context)
        {
            if (entity != null)
            {
                BattlePassItemLogsMapper.ToBattlePassItemLogs(card, entity);
                context.SaveChanges();
            }

            if (BattlePassItemLogsMapper.ToBattlePassItemLogsDTO(entity, card))
            {
                return card;
            }

            return null;
        }
    }
}
