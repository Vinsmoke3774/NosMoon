/*
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

using OpenNos.Core.Logger;
using OpenNos.DAL.EF;
using OpenNos.DAL.EF.Helpers;
using OpenNos.DAL.Interface;
using OpenNos.Data;
using OpenNos.Data.Enums;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace OpenNos.DAL.DAO
{
    public class MinigameLogDAO : IMinigameLogDAO
    {
        #region Methods

        public SaveResult InsertOrUpdate(ref MinigameLogDTO minigameLog)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                var minigameLogId = minigameLog.MinigameLogId;
                var entity = context.MinigameLog.FirstOrDefault(c => c.MinigameLogId.Equals(minigameLogId));

                if (entity == null)
                {
                    minigameLog = insert(minigameLog, context);
                    return SaveResult.Inserted;
                }
                minigameLog = update(entity, minigameLog, context);
                return SaveResult.Updated;
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
                return SaveResult.Error;
            }
        }

        public IEnumerable<MinigameLogDTO> LoadByCharacterId(long characterId)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                IEnumerable<MinigameLog> minigameLog = context.MinigameLog.Where(a => a.CharacterId.Equals(characterId)).ToList();
                if (minigameLog != null)
                {
                    var result = new List<MinigameLogDTO>();
                    foreach (var input in minigameLog)
                    {
                        var dto = new MinigameLogDTO();
                        if (Mapper.Mappers.MinigameLogMapper.ToMinigameLogDTO(input, dto))
                        {
                            result.Add(dto);
                        }
                    }
                    return result;
                }
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
            }
            return null;
        }

        public MinigameLogDTO LoadById(long minigameLogId)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                var minigameLog = context.MinigameLog.FirstOrDefault(a => a.MinigameLogId.Equals(minigameLogId));
                if (minigameLog != null)
                {
                    var minigameLogDTO = new MinigameLogDTO();
                    if (Mapper.Mappers.MinigameLogMapper.ToMinigameLogDTO(minigameLog, minigameLogDTO))
                    {
                        return minigameLogDTO;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
            }
            return null;
        }

        private static MinigameLogDTO insert(MinigameLogDTO account, OpenNosContext context)
        {
            var entity = new MinigameLog();
            Mapper.Mappers.MinigameLogMapper.ToMinigameLog(account, entity);
            context.MinigameLog.Add(entity);
            context.SaveChanges();
            Mapper.Mappers.MinigameLogMapper.ToMinigameLogDTO(entity, account);
            return account;
        }

        private static MinigameLogDTO update(MinigameLog entity, MinigameLogDTO account, OpenNosContext context)
        {
            if (entity != null)
            {
                Mapper.Mappers.MinigameLogMapper.ToMinigameLog(account, entity);
                context.Entry(entity).State = EntityState.Modified;
                context.SaveChanges();
            }
            if (Mapper.Mappers.MinigameLogMapper.ToMinigameLogDTO(entity, account))
            {
                return account;
            }

            return null;
        }

        #endregion
    }
}