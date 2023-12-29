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
    public class TwoFactorBackupsDAO : ITwoFactorBackupsDAO
    {
        private readonly IMapper<TwoFactorBackupDTO, TwoFactorBackup> _mapper = new TwoFactorBackupMapper();

        private void Insert(ref List<TwoFactorBackup> listInsert, TwoFactorBackupDTO dto)
        {
            var entity = _mapper.Map(dto);

            if (entity == null)
            {
                return;
            }

            listInsert.Add(entity);
        }

        private void Update(ref List<TwoFactorBackup> listUpdate, TwoFactorBackup entity, TwoFactorBackupDTO dto)
        {
            entity = _mapper.Map(dto);

            if (entity == null)
            {
                return;
            }

            listUpdate.Add(entity);
        }

        public SaveResult InsertOrUpdate(IEnumerable<TwoFactorBackupDTO> codes)
        {
            var listInsert = new List<TwoFactorBackup>();
            var listUpdate = new List<TwoFactorBackup>();

            try
            {
                using var context = DataAccessHelper.CreateContext();

                foreach (var code in codes)
                {
                    var foundEntity = context.TwoFactorBackups.FirstOrDefault(s => s.Id == code.Id);

                    if (foundEntity == null)
                    {
                        Insert(ref listInsert, code);
                    }
                    else
                    {
                        Update(ref listUpdate, foundEntity, code);
                    }
                }

                context.TwoFactorBackups.BulkInsert(listInsert);
                context.TwoFactorBackups.BulkUpdate(listUpdate);
                return SaveResult.Inserted;
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
                return SaveResult.Error;
            }
        }

        public DeleteResult DeleteByAccountId(long accountId)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                var foundEntities = context.TwoFactorBackups.Where(s => s.AccountId == accountId);
                context.TwoFactorBackups.RemoveRange(foundEntities);
                context.SaveChanges();

                return DeleteResult.Deleted;
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
                return DeleteResult.Error;
            }
        }

        public IEnumerable<TwoFactorBackupDTO> LoadByAccountId(long accountId)
        {
            try
            {
                var result = new List<TwoFactorBackupDTO>();

                using var context = DataAccessHelper.CreateContext();
                var foundEntities = context.TwoFactorBackups.Where(s => s.AccountId == accountId);

                foreach (var entity in foundEntities)
                {
                    var dto = _mapper.Map(entity);

                    if (dto == null)
                    {
                        continue;
                    }

                    result.Add(dto);
                }

                return result;
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
                return null;
            }
        }
    }
}
