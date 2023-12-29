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
using System.Data.Entity;
using System.Linq;

namespace OpenNos.DAL.DAO
{
    public class QuestLogDAO : IQuestLogDAO
    {
        #region Methods

        private readonly IMapper<QuestLogDTO, QuestLog> _mapper = new QuestLogMapper();

        public QuestLogDTO Insert(QuestLogDTO questLog, OpenNosContext context)
        {
            try
            {
                var entity = new QuestLog();
                entity = _mapper.Map(questLog);
                context.QuestLog.Add(entity);
                context.SaveChanges();
                if (_mapper.Map(entity) != null)
                {
                    return questLog;
                }

                return null;
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
                return null;
            }
        }

        public SaveResult InsertOrUpdate(ref QuestLogDTO quest)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                var questId = quest.QuestId;
                var characterId = quest.CharacterId;
                var entity = context.QuestLog.FirstOrDefault(c => c.QuestId.Equals(questId) && c.CharacterId.Equals(characterId));

                if (entity == null)
                {
                    quest = Insert(quest, context);
                    return SaveResult.Inserted;
                }

                quest.QuestId = entity.QuestId;
                quest = Update(entity, quest, context);
                return SaveResult.Updated;
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
                return SaveResult.Error;
            }
        }

        public SaveResult InsertOrUpdateFromList(IEnumerable<QuestLogDTO> dtos)
        {
            var listInsert = new List<QuestLog>();
            var listUpdate = new List<QuestLog>();

            try
            {
                using var context = DataAccessHelper.CreateContext();

                context.Database.CommandTimeout = 60;
                foreach (var dto in dtos)
                {
                    listInsert.Add(_mapper.Map(dto));
                    //var entity = context.QuestLog.FirstOrDefault(s => s.Id == dto.Id);

                    //if (entity == null)
                    //{
                    //    listInsert.Add(_mapper.Map(dto));
                    //}
                    //else
                    //{
                    //    listUpdate.Add(_mapper.Map(dto));
                    //}
                }

                context.QuestLog.BulkInsert(listInsert);
                context.QuestLog.BulkUpdate(listUpdate);
                return SaveResult.Inserted;
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
                return SaveResult.Error;
            }
        }

        public DeleteResult DeleteByCharacterId(long characterId)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                var entities = context.QuestLog.Where(s => s.CharacterId == characterId);
                context.QuestLog.RemoveRange(entities);

                context.SaveChanges();
                return DeleteResult.Deleted;
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
                return DeleteResult.Error;
            }
        }

        public IEnumerable<QuestLogDTO> LoadByCharacterId(long characterId)
        {
            using var context = DataAccessHelper.CreateContext();
            var result = new List<QuestLogDTO>();
            try
            {

                foreach (var questLog in context.QuestLog.Where(s => s.CharacterId == characterId))
                {
                    var dto = new QuestLogDTO();
                    dto = _mapper.Map(questLog);
                    result.Add(dto);
                }
            }
            catch
            { }
            return result;
        }

        public QuestLogDTO LoadById(long id)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                var dto = new QuestLogDTO();
                dto = _mapper.Map(context.QuestLog.FirstOrDefault(i => i.Id.Equals(id)));
                if (dto != null)
                {
                    return dto;
                }

                return null;
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
                return null;
            }
        }

        public QuestLogDTO Update(QuestLog old, QuestLogDTO replace, OpenNosContext context)
        {
            if (old != null)
            {
                old = _mapper.Map(replace);
                context.Entry(old).State = EntityState.Modified;
                context.SaveChanges();
            }
            if (_mapper.Map(old) != null)
            {
                return replace;
            }

            return null;
        }

        public bool IdAlreadySet(long id)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                return context.QuestLog.Any(s => s.Id == id);
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
                return false;
            }
        }

        #endregion
    }
}