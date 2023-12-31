﻿/*
 * This file is part of the OpenNos Emulator Project. See AUTHORS file for Copyright information
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 */

using OpenNos.Core;
using OpenNos.Core.Logger;
using OpenNos.DAL.EF;
using OpenNos.DAL.EF.Helpers;
using OpenNos.DAL.Interface;
using OpenNos.Data;
using OpenNos.Data.Enums;
using System;
using System.Collections.Generic;

namespace OpenNos.DAL.DAO
{
    public class QuestDAO : IQuestDAO
    {
        #region Methods

        public DeleteResult DeleteById(long id)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                var deleteEntity = context.Quest.Find(id);
                if (deleteEntity != null)
                {
                    context.Quest.Remove(deleteEntity);
                    context.SaveChanges();
                }

                return DeleteResult.Deleted;
            }
            catch (Exception e)
            {
                Logger.Log.Error(string.Format(Language.Instance.GetMessageFromKey("DELETE_ERROR"), id, e.Message), e);
                return DeleteResult.Error;
            }
        }

        public void Insert(List<QuestDTO> questList)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                context.Configuration.AutoDetectChangesEnabled = false;
                foreach (var quest in questList)
                {
                    var entity = new Quest();
                    Mapper.Mappers.QuestMapper.ToQuest(quest, entity);
                    context.Quest.Add(entity);
                }
                context.Configuration.AutoDetectChangesEnabled = true;
                context.SaveChanges();
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
            }
        }

        public QuestDTO InsertOrUpdate(QuestDTO quest)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                var entity = context.Quest.Find(quest.QuestId);

                if (entity == null)
                {
                    return insert(quest, context);
                }
                return update(entity, quest, context);
            }
            catch (Exception e)
            {
                Logger.Log.Error(string.Format(Language.Instance.GetMessageFromKey("INSERT_ERROR"), quest, e.Message), e);
                return quest;
            }
        }

        public IEnumerable<QuestDTO> LoadAll()
        {
            using var context = DataAccessHelper.CreateContext();
            var result = new List<QuestDTO>();
            foreach (var entity in context.Quest)
            {
                var dto = new QuestDTO();
                Mapper.Mappers.QuestMapper.ToQuestDTO(entity, dto);
                result.Add(dto);
            }
            return result;
        }

        public QuestDTO LoadById(long id)
        {
            using var context = DataAccessHelper.CreateContext();
            var dto = new QuestDTO();
            if (Mapper.Mappers.QuestMapper.ToQuestDTO(context.Quest.Find(id), dto))
            {
                return dto;
            }

            return null;
        }

        private static QuestDTO insert(QuestDTO quest, OpenNosContext context)
        {
            var entity = new Quest();
            Mapper.Mappers.QuestMapper.ToQuest(quest, entity);
            context.Quest.Add(entity);
            context.SaveChanges();
            if (Mapper.Mappers.QuestMapper.ToQuestDTO(entity, quest))
            {
                return quest;
            }

            return null;
        }

        private static QuestDTO update(Quest entity, QuestDTO quest, OpenNosContext context)
        {
            if (entity != null)
            {
                Mapper.Mappers.QuestMapper.ToQuest(quest, entity);
                context.SaveChanges();
            }

            if (Mapper.Mappers.QuestMapper.ToQuestDTO(entity, quest))
            {
                return quest;
            }

            return null;
        }

        #endregion
    }
}