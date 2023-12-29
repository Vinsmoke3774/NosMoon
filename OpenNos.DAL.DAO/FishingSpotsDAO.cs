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
    public class FishingSpotsDAO : IFishingSpotsDAO
    {
        public SaveResult InsertOrUpdateFromList(List<FishingSpotsDTO> spots)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                context.Configuration.AutoDetectChangesEnabled = false;
                foreach (var card in spots)
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

        public IEnumerable<FishingSpotsDTO> LoadAll()
        {
            using var context = DataAccessHelper.CreateContext();
            var result = new List<FishingSpotsDTO>();
            foreach (var entity in context.FishingSpots)
            {
                var dto = new FishingSpotsDTO();
                FishingSpotsMapper.ToFishingSpotsDTO(entity, dto);
                result.Add(dto);
            }
            return result;
        }

        public SaveResult InsertOrUpdate(FishingSpotsDTO card)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                long CardId = card.Id;
                var entity = context.FishingSpots.FirstOrDefault(c => c.Id == CardId);

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

        private static FishingSpotsDTO insert(FishingSpotsDTO card, OpenNosContext context)
        {
            var entity = new FishingSpots();
            FishingSpotsMapper.ToFishingSpots(card, entity);
            context.FishingSpots.Add(entity);
            context.SaveChanges();
            if (FishingSpotsMapper.ToFishingSpotsDTO(entity, card))
            {
                return card;
            }

            return null;
        }

        private static FishingSpotsDTO update(FishingSpots entity, FishingSpotsDTO card, OpenNosContext context)
        {
            if (entity != null)
            {
                FishingSpotsMapper.ToFishingSpots(card, entity);
                context.SaveChanges();
            }

            if (FishingSpotsMapper.ToFishingSpotsDTO(entity, card))
            {
                return card;
            }

            return null;
        }
    }
}
