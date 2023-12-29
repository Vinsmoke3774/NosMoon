using OpenNos.Core.Logger;
using OpenNos.DAL.EF;
using OpenNos.DAL.EF.Helpers;
using OpenNos.DAL.Interface;
using OpenNos.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenNos.DAL.DAO
{
    public class QuestObjectiveDAO : IQuestObjectiveDAO
    {
        #region Methods

        public void Insert(List<QuestObjectiveDTO> quests)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                context.Configuration.AutoDetectChangesEnabled = false;
                foreach (var quest in quests)
                {
                    var entity = new QuestObjective();
                    Mapper.Mappers.QuestObjectiveMapper.ToQuestObjective(quest, entity);
                    context.QuestObjective.Add(entity);
                }
                context.Configuration.AutoDetectChangesEnabled = true;
                context.SaveChanges();
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
            }
        }

        public QuestObjectiveDTO Insert(QuestObjectiveDTO questObjective)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                var entity = new QuestObjective();
                Mapper.Mappers.QuestObjectiveMapper.ToQuestObjective(questObjective, entity);
                context.QuestObjective.Add(entity);
                context.SaveChanges();
                if (Mapper.Mappers.QuestObjectiveMapper.ToQuestObjectiveDTO(entity, questObjective))
                {
                    return questObjective;
                }

                return null;
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
                return null;
            }
        }

        public List<QuestObjectiveDTO> LoadAll()
        {
            using var context = DataAccessHelper.CreateContext();
            var result = new List<QuestObjectiveDTO>();
            foreach (var questObjective in context.QuestObjective)
            {
                var dto = new QuestObjectiveDTO();
                Mapper.Mappers.QuestObjectiveMapper.ToQuestObjectiveDTO(questObjective, dto);
                result.Add(dto);
            }
            return result;
        }

        public IEnumerable<QuestObjectiveDTO> LoadByQuestId(long questId)
        {
            using var context = DataAccessHelper.CreateContext();
            var result = new List<QuestObjectiveDTO>();
            foreach (var questObjective in context.QuestObjective.Where(s => s.QuestId == questId))
            {
                var dto = new QuestObjectiveDTO();
                Mapper.Mappers.QuestObjectiveMapper.ToQuestObjectiveDTO(questObjective, dto);
                result.Add(dto);
            }
            return result;
        }

        #endregion
    }
}