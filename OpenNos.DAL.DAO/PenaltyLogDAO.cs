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

using OpenNos.Core;
using OpenNos.Core.Logger;
using OpenNos.DAL.EF;
using OpenNos.DAL.EF.Helpers;
using OpenNos.DAL.Interface;
using OpenNos.Data;
using OpenNos.Data.Enums;
using OpenNos.Domain;
using OpenNos.Mapper;
using OpenNos.Mapper.Mappers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenNos.DAL.DAO
{
    public class PenaltyLogDAO : IPenaltyLogDAO
    {
        private readonly IMapper<PenaltyLogDTO, PenaltyLog> _mapper = new PenaltyLogMapper();

        #region Methods

        public DeleteResult Delete(int penaltyLogId)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                var PenaltyLog = context.PenaltyLog.FirstOrDefault(c => c.PenaltyLogId.Equals(penaltyLogId));

                if (PenaltyLog != null)
                {
                    context.PenaltyLog.Remove(PenaltyLog);
                    context.SaveChanges();
                }

                return DeleteResult.Deleted;
            }
            catch (Exception e)
            {
                Logger.Log.Error(string.Format(Language.Instance.GetMessageFromKey("DELETE_PENALTYLOG_ERROR"), penaltyLogId, e.Message), e);
                return DeleteResult.Error;
            }
        }

        public SaveResult InsertOrUpdate(ref PenaltyLogDTO log)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                var id = log.PenaltyLogId;
                var entity = context.PenaltyLog.FirstOrDefault(c => c.PenaltyLogId.Equals(id));

                if (entity == null)
                {
                    log = insert(log, context);
                    return SaveResult.Inserted;
                }

                log = update(entity, log, context);
                return SaveResult.Updated;
            }
            catch (Exception e)
            {
                Logger.Log.Error(string.Format(Language.Instance.GetMessageFromKey("UPDATE_PENALTYLOG_ERROR"), log.PenaltyLogId, e.Message), e);
                return SaveResult.Error;
            }
        }

        public IEnumerable<PenaltyLogDTO> LoadAll()
        {
            using var context = DataAccessHelper.CreateContext();
            var result = new List<PenaltyLogDTO>();
            foreach (var entity in context.PenaltyLog)
            {
                var dto = _mapper.Map(entity);
                result.Add(dto);
            }
            return result;
        }

        public IEnumerable<PenaltyLogDTO> LoadByAccount(long accountId)
        {
            using var context = DataAccessHelper.CreateContext();
            var result = new List<PenaltyLogDTO>();
            foreach (var PenaltyLog in context.PenaltyLog.Where(s => s.AccountId.Equals(accountId)))
            {
                var dto = _mapper.Map(PenaltyLog);
                result.Add(dto);
            }
            return result;
        }

        public IEnumerable<PenaltyLogDTO> LoadExpiredBans()
        {
            var result = new List<PenaltyLogDTO>();
            try
            {
                using var context = DataAccessHelper.CreateContext();
                var currentTime = DateTime.Now;

                var entities = context.PenaltyLog.Where(s => s.DateEnd > DateTime.Now && s.Penalty == PenaltyType.Banned);

                foreach (var entity in entities)
                {
                    result.Add(_mapper.Map(entity));
                }


                return result;
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
                return result;
            }
        }

        public PenaltyLogDTO LoadById(int penaltyLogId)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                var dto = _mapper.Map(context.PenaltyLog.FirstOrDefault(s => s.PenaltyLogId.Equals(penaltyLogId)));
                return dto;
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
                return null;
            }
        }

        public IEnumerable<PenaltyLogDTO> LoadByIp(string ip)
        {
            using var context = DataAccessHelper.CreateContext();
            var cleanIp = ip.Replace("tcp://", "");
            cleanIp = cleanIp.Substring(0, cleanIp.LastIndexOf(":") > 0 ? cleanIp.LastIndexOf(":") : cleanIp.Length);
            var result = new List<PenaltyLogDTO>();
            foreach (var PenaltyLog in context.PenaltyLog.Where(s => s.IP.Contains(cleanIp)))
            {
                var dto = _mapper.Map(PenaltyLog);
                result.Add(dto);
            }
            return result;
        }

        private PenaltyLogDTO insert(PenaltyLogDTO penaltylog, OpenNosContext context)
        {
            var entity = _mapper.Map(penaltylog);
            context.PenaltyLog.Add(entity);
            context.SaveChanges();

            return _mapper.Map(entity);
        }

        private PenaltyLogDTO update(PenaltyLog entity, PenaltyLogDTO penaltylog, OpenNosContext context)
        {
            if (entity != null)
            {
                context.Entry(entity).CurrentValues.SetValues(_mapper.Map(penaltylog));
                context.SaveChanges();
            }

            return _mapper.Map(entity);
        }

        #endregion
    }
}