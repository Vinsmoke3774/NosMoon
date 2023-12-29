using OpenNos.Core;
using OpenNos.Core.Logger;
using OpenNos.DAL.EF;
using OpenNos.DAL.EF.Helpers;
using OpenNos.DAL.Interface;
using OpenNos.Data;
using OpenNos.Data.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenNos.DAL.DAO
{
    public class FishInfoDAO : IFishInfoDAO
    {
        public SaveResult InsertOrUpdateFromList(List<FishInfoDTO> fishes)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                context.Configuration.AutoDetectChangesEnabled = false;
                foreach (var card in fishes)
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

        public IEnumerable<FishInfoDTO> LoadAll()
        {
            using var context = DataAccessHelper.CreateContext();
            var result = new List<FishInfoDTO>();
            foreach (var entity in context.FishInfo)
            {
                var dto = new FishInfoDTO();
                Mapper.Mappers.FishInfoMapper.ToFishInfoDTO(entity, dto);
                result.Add(dto);
            }
            return result;
        }

        public SaveResult InsertOrUpdate(FishInfoDTO card)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                long CardId = card.Id;
                var entity = context.FishInfo.FirstOrDefault(c => c.Id == CardId);

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

        private static FishInfoDTO insert(FishInfoDTO card, OpenNosContext context)
        {
            var entity = new FishInfo();
            Mapper.Mappers.FishInfoMapper.ToFishInfo(card, entity);
            context.FishInfo.Add(entity);
            context.SaveChanges();
            if (Mapper.Mappers.FishInfoMapper.ToFishInfoDTO(entity, card))
            {
                return card;
            }

            return null;
        }

        private static FishInfoDTO update(FishInfo entity, FishInfoDTO card, OpenNosContext context)
        {
            if (entity != null)
            {
                Mapper.Mappers.FishInfoMapper.ToFishInfo(card, entity);
                context.SaveChanges();
            }

            if (Mapper.Mappers.FishInfoMapper.ToFishInfoDTO(entity, card))
            {
                return card;
            }

            return null;
        }
    }
}
