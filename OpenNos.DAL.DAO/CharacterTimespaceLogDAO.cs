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
    public class CharacterTimespaceLogDAO : ICharacterTimespaceLogDAO
    {
        public CharacterTimespaceLogDTO GetHighestScoreByScriptedInstanceId(long scriptedInstanceId)
        {
            using var context = DataAccessHelper.CreateContext();
            var entity = context.CharacterTimespaceLog.Where(s => s.ScriptedInstanceId == scriptedInstanceId).OrderByDescending(s => s.Score).FirstOrDefault();
            var dto = new CharacterTimespaceLogDTO();
            CharacterTimespaceLogMapper.ToCharacterTimespaceLogDto(entity, dto);
            return dto;
        }

        public List<CharacterTimespaceLogDTO> LoadAll()
        {
            using var context = DataAccessHelper.CreateContext();
            var result = new List<CharacterTimespaceLogDTO>();
            foreach (var battlePassItem in context.CharacterTimespaceLog)
            {
                var dto = new CharacterTimespaceLogDTO();
                CharacterTimespaceLogMapper.ToCharacterTimespaceLogDto(battlePassItem, dto);
                result.Add(dto);
            }
            return result;
        }

        public SaveResult InsertOrUpdateFromList(IEnumerable<CharacterTimespaceLogDTO> logs)
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

        public SaveResult InsertOrUpdate(CharacterTimespaceLogDTO card)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                long logId = card.ScriptedInstanceId;
                var entity = context.CharacterTimespaceLog.FirstOrDefault(c => c.Date == card.Date && c.CharacterId == card.CharacterId && c.Score == card.Score);

                if (entity == null)
                {
                    card = insert(card, context);
                    return SaveResult.Inserted;
                }

                var r = new CharacterTimespaceLog();
                CharacterTimespaceLogMapper.ToCharacterTimespaceLogEntity(card, r);
                var z = new CharacterTimespaceLogDTO();
                CharacterTimespaceLogMapper.ToCharacterTimespaceLogDto(entity, z);
                update(r, z, context);
                return SaveResult.Updated;
            }
            catch (Exception e)
            {
                Logger.Log.Error(string.Format(Language.Instance.GetMessageFromKey("UPDATE_CARD_ERROR"), card.LogId, e.Message), e);
                return SaveResult.Error;
            }
        }

        public IEnumerable<CharacterTimespaceLogDTO> LoadByCharactedId(long characterId)
        {
            using var context = DataAccessHelper.CreateContext();
            var result = new List<CharacterTimespaceLogDTO>();
            foreach (var entity in context.CharacterTimespaceLog.Where(s => s.CharacterId == characterId))
            {
                var dto = new CharacterTimespaceLogDTO();
                CharacterTimespaceLogMapper.ToCharacterTimespaceLogDto(entity, dto);
                result.Add(dto);
            }
            return result;
        }

        private static CharacterTimespaceLogDTO insert(CharacterTimespaceLogDTO card, OpenNosContext context)
        {
            var entity = new CharacterTimespaceLog();
            CharacterTimespaceLogMapper.ToCharacterTimespaceLogEntity(card, entity);
            context.CharacterTimespaceLog.Add(entity);
            context.SaveChanges();
            if (CharacterTimespaceLogMapper.ToCharacterTimespaceLogDto(entity, card))
            {
                return card;
            }

            return null;
        }

        private static void update(CharacterTimespaceLog entity, CharacterTimespaceLogDTO card, OpenNosContext context)
        {
            context.CharacterTimespaceLog.AddOrUpdateExtension(entity);
            context.SaveChanges();
        }
    }
}
