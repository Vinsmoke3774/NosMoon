using OpenNos.Core.Logger;
using OpenNos.DAL.EF;
using OpenNos.DAL.EF.Helpers;
using OpenNos.DAL.Interface;
using OpenNos.Data;
using OpenNos.Data.Enums;
using OpenNos.Mapper;
using OpenNos.Mapper.Mappers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenNos.DAL.DAO
{
    public class CharacterQuestDAO : ICharacterQuestDAO
    {
        private readonly IMapper<CharacterQuestDTO, CharacterQuest> _mapper = new CharacterQuestMapper();

        #region Methods

        public DeleteResult DeleteFromList(long characterId, IEnumerable<long> questIds)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                var questsFound = questIds.Select(questId => context.CharacterQuest.FirstOrDefault(s => s.CharacterId == characterId && s.QuestId == questId)).ToList();
                context.CharacterQuest.BulkDelete(questsFound);
                return DeleteResult.Deleted;
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
                return DeleteResult.Error;
            }
        }

        public DeleteResult DeleteForCharacterId(long characterId)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                var foundQuests = context.CharacterQuest.Where(s => s.CharacterId == characterId);

                context.CharacterQuest.BulkDelete(foundQuests);
                return DeleteResult.Deleted;
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
                return DeleteResult.Error;
            }
        }

        public DeleteResult DeleteFromList(IEnumerable<CharacterQuestDTO> quests)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                var toDelete = new List<CharacterQuest>();

                foreach (var quest in quests)
                {
                    toDelete.Add(_mapper.Map(quest));
                }

                context.BulkDelete(toDelete);
                return DeleteResult.Deleted;
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
                return DeleteResult.Error;
            }
        }

        private void Insert(CharacterQuestDTO questDto, ref List<CharacterQuest> listInsert)
        {
            CharacterQuest entity = _mapper.Map(questDto);
            listInsert.Add(entity);
        }

        private void Update(CharacterQuest entity, CharacterQuestDTO questDto, ref List<CharacterQuest> listUpdate)
        {
            if (entity != null)
            {
                entity = _mapper.Map(questDto);
                listUpdate.Add(entity);
            }
        }

        public SaveResult InsertOrUpdateFromList(IEnumerable<CharacterQuestDTO> quests)
        {
            var listInsert = new List<CharacterQuest>();
            var listUpdate = new List<CharacterQuest>();
            try
            {
                using var context = DataAccessHelper.CreateContext();

                foreach (var quest in quests)
                {
                    var primaryKey = quest.Id;
                    var entity = context.CharacterQuest.FirstOrDefault(s => s.Id == primaryKey);

                    if (entity == null)
                    {
                        Insert(quest, ref listInsert);
                    }
                    else
                    {
                        Update(entity, quest, ref listUpdate);
                    }
                }

                context.CharacterQuest.BulkInsert(listInsert);
                context.CharacterQuest.BulkUpdate(listUpdate);
                return SaveResult.Inserted;
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
                return SaveResult.Error;
            }
        }

        public IEnumerable<CharacterQuestDTO> LoadByCharacterId(long characterId)
        {
            using var context = DataAccessHelper.CreateContext();
            var quests = context.CharacterQuest.Where(s => s.CharacterId == characterId);
            var result = new List<CharacterQuestDTO>();

            foreach (var quest in quests)
            {
                result.Add(_mapper.Map(quest));
            }

            return result;
        }

        #endregion
    }
}