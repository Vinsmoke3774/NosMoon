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
    public class TitleLogDAO : ITitleLogDAO
    {
        private readonly IMapper<TitleLogDTO, TitleLog> _mapper = new TitleLogMapper();

        public IEnumerable<TitleLogDTO> GetByCharacterId(long characterId)
        {
            var result = new List<TitleLogDTO>();
            try
            {
                using var context = DataAccessHelper.CreateContext();

                var foundEntities = context.TitleLogs.Where(s => s.CharacterId == characterId);

                foreach (var entity in foundEntities)
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

        public SaveResult InsertOrUpdate(TitleLogDTO log)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();

                var entity = context.TitleLogs.FirstOrDefault(s => s.Id == log.Id);

                if (entity == null)
                {
                    context.TitleLogs.Add(_mapper.Map(log));
                }
                else
                {
                    entity = _mapper.Map(log);
                }

                context.SaveChanges();
                return SaveResult.Inserted;
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
                return SaveResult.Error;
            }
        }
    }
}
