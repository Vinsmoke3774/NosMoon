using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNos.Core;
using OpenNos.Core.Logger;
using OpenNos.DAL.EF;
using OpenNos.DAL.EF.Helpers;
using OpenNos.DAL.Interface;
using OpenNos.Data;
using OpenNos.Data.Enums;
using OpenNos.Mapper;
using OpenNos.Mapper.Mappers;

namespace OpenNos.DAL.DAO
{
    public class WhitelistedCharacterDao : IWhitelistedCharacterDAO
    {
        private readonly IMapper<WhitelistedPlayerDTO, WhitelistedPlayerEntity> _mapper = new WhitelistedPlayerMapper();

        public SaveResult InsertOrUpdate(WhitelistedPlayerDTO player)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                var entity = context.WhitelistedPlayers.FirstOrDefault(s => s.IpAddress == player.IpAddress);

                if (entity == null)
                {
                    context.WhitelistedPlayers.Add(_mapper.Map(player));
                    context.SaveChanges();
                    return SaveResult.Inserted;
                }

                context.Entry(entity).CurrentValues.SetValues(_mapper.Map(player));
                context.SaveChanges();
                return SaveResult.Updated;
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
                return SaveResult.Error;
            }
        }

        public DeleteResult Delete(WhitelistedPlayerDTO player)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                var entity = context.WhitelistedPlayers.FirstOrDefault(s => s.IpAddress == player.IpAddress);

                if (entity != null)
                {
                    context.WhitelistedPlayers.Remove(entity);
                    context.SaveChanges();
                    return DeleteResult.Deleted;
                }

                return DeleteResult.NotFound;
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
                return DeleteResult.Error;
            }
        }

        public WhitelistedPlayerDTO LoadByIpAddress(string ip)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();

                var entity = context.WhitelistedPlayers.FirstOrDefault(s => s.IpAddress.Equals(ip));

                return _mapper.Map(entity);
            }
            catch (Exception e)
            {
                return null;
            }
        }
    }
}
