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
    public class FishingLogDAO : IFishingLogDAO
    {
        public SaveResult InsertOrUpdateFromList(IEnumerable<FishingLogDTO> logs)
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
                Logger.Log.Error(null, e);
                return SaveResult.Error;
            }
        }

        public IEnumerable<FishingLogDTO> LoadByCharacterId(long characterId)
        {
            using var context = DataAccessHelper.CreateContext();
            var result = new List<FishingLogDTO>();
            foreach (var entity in context.FishingLog.Where(s => s.CharacterId == characterId))
            {
                var dto = new FishingLogDTO();
                FishingLogMapper.ToFishingLogDTO(entity, dto);
                result.Add(dto);
            }
            return result;
        }

        public SaveResult InsertOrUpdate(FishingLogDTO card)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                long CardId = card.Id;
                var entity = context.FishingLog.FirstOrDefault(c => c.Id == CardId);

                if (entity == null)
                {
                    card = insert(card, context);
                    return SaveResult.Inserted;
                }

                card = update(entity, card, context);
                return SaveResult.Updated;
            }
            catch (Exception e)
            {
                Logger.Log.Error(string.Format(Language.Instance.GetMessageFromKey("UPDATE_CARD_ERROR"), card.Id, e.Message), e);
                return SaveResult.Error;
            }
        }

        private static FishingLogDTO insert(FishingLogDTO card, OpenNosContext context)
        {
            var entity = new FishingLog();
            FishingLogMapper.ToFishingLog(card, entity);
            context.FishingLog.Add(entity);
            context.SaveChanges();
            if (FishingLogMapper.ToFishingLogDTO(entity, card))
            {
                return card;
            }

            return null;
        }

        private static FishingLogDTO update(FishingLog entity, FishingLogDTO card, OpenNosContext context)
        {
            if (entity != null)
            {
                FishingLogMapper.ToFishingLog(card, entity);
                context.SaveChanges();
            }

            if (FishingLogMapper.ToFishingLogDTO(entity, card))
            {
                return card;
            }

            return null;
        }
    }
}
