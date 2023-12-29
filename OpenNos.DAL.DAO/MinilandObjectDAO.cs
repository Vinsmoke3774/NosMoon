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
using System.Linq;

namespace OpenNos.DAL.DAO
{
    public class MinilandObjectDAO : IMinilandObjectDAO
    {
        #region Methods

        public DeleteResult DeleteById(long id)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                var item = context.MinilandObject.First(i => i.MinilandObjectId.Equals(id));

                if (item != null)
                {
                    context.MinilandObject.Remove(item);
                    context.SaveChanges();
                }

                return DeleteResult.Deleted;
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
                return DeleteResult.Error;
            }
        }

        public DeleteResult DeleteByItemId(Guid id)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                var item = context.MinilandObject.First(i => i.ItemInstanceId.Equals(id));

                if (item != null)
                {
                    context.MinilandObject.Remove(item);
                    context.SaveChanges();
                }

                return DeleteResult.Deleted;
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
                return DeleteResult.Error;
            }
        }

        public SaveResult InsertOrUpdate(ref MinilandObjectDTO obj)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                var id = obj.MinilandObjectId;
                var entity = context.MinilandObject.FirstOrDefault(c => c.MinilandObjectId.Equals(id));

                if (entity == null)
                {
                    obj = insert(obj, context);
                    return SaveResult.Inserted;
                }

                obj.MinilandObjectId = entity.MinilandObjectId;
                obj = update(entity, obj, context);
                return SaveResult.Updated;
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
                return SaveResult.Error;
            }
        }

        public IEnumerable<MinilandObjectDTO> LoadAll()
        {
            using var context = DataAccessHelper.CreateContext();
            var result = new List<MinilandObjectDTO>();
            foreach (var bazaarItem in context.MinilandObject)
            {
                var dto = new MinilandObjectDTO();
                Mapper.Mappers.MinilandObjectMapper.ToMinilandObjectDTO(bazaarItem, dto);
                result.Add(dto);
            }
            return result;
        }

        public IEnumerable<MinilandObjectDTO> LoadByCharacterId(long characterId)
        {
            using var context = DataAccessHelper.CreateContext();
            var result = new List<MinilandObjectDTO>();
            foreach (var obj in context.MinilandObject.Where(s => s.CharacterId == characterId))
            {
                var dto = new MinilandObjectDTO();
                Mapper.Mappers.MinilandObjectMapper.ToMinilandObjectDTO(obj, dto);
                result.Add(dto);
            }
            return result;
        }

        private static MinilandObjectDTO insert(MinilandObjectDTO obj, OpenNosContext context)
        {
            try
            {
                var entity = new MinilandObject();
                Mapper.Mappers.MinilandObjectMapper.ToMinilandObject(obj, entity);
                context.MinilandObject.Add(entity);
                context.SaveChanges();
                if (Mapper.Mappers.MinilandObjectMapper.ToMinilandObjectDTO(entity, obj))
                {
                    return obj;
                }

                return null;
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
                return null;
            }
        }

        private static MinilandObjectDTO update(MinilandObject entity, MinilandObjectDTO respawn, OpenNosContext context)
        {
            if (entity != null)
            {
                Mapper.Mappers.MinilandObjectMapper.ToMinilandObject(respawn, entity);
                context.SaveChanges();
            }
            if (Mapper.Mappers.MinilandObjectMapper.ToMinilandObjectDTO(entity, respawn))
            {
                return respawn;
            }

            return null;
        }

        #endregion
    }
}