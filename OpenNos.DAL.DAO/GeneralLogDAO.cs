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
using OpenNos.Mapper;
using OpenNos.Mapper.Mappers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenNos.DAL.DAO
{
    public class GeneralLogDAO : IGeneralLogDAO
    {
        private readonly IMapper<GeneralLogDTO, GeneralLog> _mapper = new GeneralLogMapper();

        #region Methods

        #endregion

        public bool IdAlreadySet(long id)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                return context.GeneralLog.Any(gl => gl.LogId == id);
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
                return false;
            }
        }

        public DeleteResult Delete(IEnumerable<long> ids)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                var entities = context.GeneralLog.Where(s => ids.Contains(s.LogId));

                context.GeneralLog.BulkDelete(entities);
                return DeleteResult.Deleted;
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
                return DeleteResult.Error;
            }
        }

        public GeneralLogDTO Insert(GeneralLogDTO generalLog)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                var entity = _mapper.Map(generalLog);

                context.GeneralLog.Add(entity);
                context.SaveChanges();
                return _mapper.Map(entity);
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
                return null;
            }
        }

        public SaveResult Insert(IEnumerable<GeneralLogDTO> generalLog)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                context.GeneralLog.BulkInsert(_mapper.Map(generalLog));
                return SaveResult.Inserted;
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
                return SaveResult.Error;
            }
        }

        public SaveResult InsertOrUpdate(GeneralLogDTO generalLog)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                var entity = context.GeneralLog.FirstOrDefault(s => s.LogId == generalLog.LogId);

                var result = SaveResult.Unknown;
                if (entity == null)
                {
                    context.GeneralLog.Add(_mapper.Map(generalLog));
                    result = SaveResult.Inserted;
                }
                else
                {
                    context.Entry(entity).CurrentValues.SetValues(_mapper.Map(generalLog));
                    result = SaveResult.Updated;
                }

                return result;
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
                return SaveResult.Error;
            }
        }

        public SaveResult InsertOrUpdate(IEnumerable<GeneralLogDTO> logs)
        {
            var listInsert = new List<GeneralLog>();
            var listUpdate = new List<GeneralLog>();
            try
            {
                using var context = DataAccessHelper.CreateContext();
                var ids = logs.Select(s => s.LogId);
                var entities = context.GeneralLog.Where(s => ids.Contains(s.LogId));

                foreach (var log in logs)
                {
                    var entity = entities.FirstOrDefault(s => s.LogId == log.LogId);

                    if (entity == null)
                    {
                        listInsert.Add(_mapper.Map(log));
                    }
                    else
                    {
                        entity = _mapper.Map(log);
                        listUpdate.Add(entity);
                    }
                }

                context.GeneralLog.BulkInsert(listInsert);
                context.GeneralLog.BulkUpdate(listUpdate);
                return SaveResult.Inserted;
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
                return SaveResult.Error;
            }
        }

        public IEnumerable<GeneralLogDTO> LoadAll()
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                var entities = context.GeneralLog.ToList();

                return _mapper.Map(entities);
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
                return null;
            }
        }

        public IEnumerable<GeneralLogDTO> LoadByAccount(long? accountId)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();

                if (!accountId.HasValue)
                {
                    return null;
                }

                var entities = context.GeneralLog.Where(s => s.AccountId.Value == accountId.Value);
                return _mapper.Map(entities);
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
                return null;
            }
        }

        public IEnumerable<GeneralLogDTO> LoadByIp(string ip)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                var entities = context.GeneralLog.Where(s => s.IpAddress == ip);

                return _mapper.Map(entities);
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
                return null;
            }
        }

        public IEnumerable<GeneralLogDTO> LoadByLogType(string logType, long? characterId)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();

                var entities = context.GeneralLog.Where(s => s.LogType == logType && s.CharacterId == characterId);
                return _mapper.Map(entities);
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
                return null;
            }
        }

        public IEnumerable<GeneralLogDTO> LoadByLogTypeAndAccountId(string logType, long? accountId)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();

                var entities = context.GeneralLog.Where(s => s.LogType == logType && s.AccountId == accountId);
                return _mapper.Map(entities);
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
                return null;
            }
        }
    }
}