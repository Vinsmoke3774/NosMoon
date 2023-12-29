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
    public class InstantBattleLogDAO : IInstantBattleLogDAO
    {
        private readonly IMapper<InstantBattleLogDTO, InstantBattleLog> _mapper = new InstantBattleLogMapper();

        public IEnumerable<InstantBattleLogDTO> LoadByCharacterId(long characterId)
        {
            using var context = DataAccessHelper.CreateContext();
            var result = new List<InstantBattleLogDTO>();
            try
            {
                var foundEntities = context.InstantBattleLogs.Where(s => s.CharacterId == characterId);

                foreach (var entity in foundEntities)
                {
                    result.Add(_mapper.Map(entity));
                }
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
            }

            return result;
        }

        public SaveResult InsertOrUpdateFromList(IEnumerable<InstantBattleLogDTO> logs)
        {
            var listInsert = new List<InstantBattleLog>();
            var listUpdate = new List<InstantBattleLog>();
            try
            {
                using var context = DataAccessHelper.CreateContext();

                foreach (var log in logs)
                {
                    var entity = context.InstantBattleLogs.FirstOrDefault(s => s.Id == log.Id);

                    if (entity == null)
                    {
                        listInsert.Add(_mapper.Map(log));
                    }
                    else
                    {
                        listUpdate.Add(_mapper.Map(log));
                    }
                }

                context.InstantBattleLogs.BulkInsert(listInsert);
                context.InstantBattleLogs.BulkUpdate(listUpdate);
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
                var entities = context.InstantBattleLogs.Where(s => s.CharacterId == characterId);
                context.InstantBattleLogs.RemoveRange(entities);
                context.SaveChanges();
                return DeleteResult.Deleted;
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
                return DeleteResult.Error;
            }
        }
    }
}
