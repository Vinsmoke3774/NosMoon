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
using OpenNos.Domain;
using OpenNos.Mapper;
using OpenNos.Mapper.Mappers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenNos.DAL.DAO
{
    public class ItemInstanceDAO : IItemInstanceDAO
    {
        private readonly IMapper<ItemInstanceDTO, ItemInstance> _mapper;

        public ItemInstanceDAO()
        {
            _mapper = new ItemInstanceMapper();
        }

        public DeleteResult Delete(Guid id)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                var entity = context.ItemInstance.FirstOrDefault(s => s.Id == id);

                if (entity == null)
                {
                    return DeleteResult.NotFound;
                }

                context.ItemInstance.Remove(entity);
                context.SaveChanges();
                return DeleteResult.Deleted;
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
                return DeleteResult.Error;
            }
        }

        public DeleteResult DeleteFromSlotAndType(long characterId, short slot, InventoryType type)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                var entity = context.ItemInstance.FirstOrDefault(s =>
                    s.CharacterId == characterId && s.Slot == slot && s.Type == type);

                if (entity == null)
                {
                    return DeleteResult.NotFound;
                }

                context.ItemInstance.Remove(entity);
                context.SaveChanges();
                return DeleteResult.Deleted;
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
                return DeleteResult.Error;
            }
        }

        public DeleteResult DeleteGuidList(IEnumerable<Guid> guids)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                var entities = context.ItemInstance.Where(s => guids.Contains(s.Id) && s.Type != InventoryType.FamilyWareHouse).ToList();
                context.ItemInstance.BulkDelete(entities);
                return DeleteResult.Deleted;
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
                return DeleteResult.Error;
            }
        }

        public DeleteResult DeleteMinilandBazzar(Guid id)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                var entity = context.Set<BazaarItem>().FirstOrDefault(i => i.ItemInstanceId == id);
                if (entity != null)
                {
                    context.Set<BazaarItem>().Remove(entity);
                    context.SaveChanges();
                }
                var entity2 = context.Set<MinilandObject>().FirstOrDefault(i => i.ItemInstanceId == id);
                if (entity != null)
                {
                    context.Set<MinilandObject>().Remove(entity2);
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

        public ItemInstanceDTO InsertOrUpdate(ItemInstanceDTO dto)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                var entity = context.ItemInstance.FirstOrDefault(s => s.Id == dto.Id);

                if (entity == null)
                {
                    context.ItemInstance.Add(_mapper.Map(dto));
                }
                else
                {
                    context.Entry(entity).CurrentValues.SetValues(_mapper.Map(dto));
                }

                context.SaveChanges();
                return dto;
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
                return null;
            }
        }

        public IEnumerable<ItemInstanceDTO> BulkInsertOrUpdate(IEnumerable<ItemInstanceDTO> dtos)
        {
            var listInsert = new List<ItemInstance>();
            var listUpdate = new List<ItemInstance>();
            try
            {
                using var context = DataAccessHelper.CreateContext();
                var dtoIds = dtos.Select(s => s.Id);
                var entities = context.ItemInstance.Where(s => dtoIds.Contains(s.Id));

                foreach (var dto in dtos)
                {
                    var entity = entities.FirstOrDefault(s => s.Id == dto.Id);

                    if (entity == null)
                    {
                        listInsert.Add(_mapper.Map(dto));
                    }
                    else
                    {
                        entity = _mapper.Map(dto);
                        listUpdate.Add(entity);
                    }
                }

                context.ItemInstance.BulkInsert(listInsert);
                context.ItemInstance.BulkUpdate(listUpdate);
                return dtos;
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
                return null;
            }
        }

        public bool InsertOrUpdate(IEnumerable<ItemInstanceDTO> items)
        {
            var listInsert = new List<ItemInstance>();
            try
            {
                using var context = DataAccessHelper.CreateContext();
                var itemIds = items.Select(s => s.Id);

                foreach (var dto in items)
                {
                    var entity = context.ItemInstance.FirstOrDefault(s => s.Id == dto.Id);

                    if (entity == null)
                    {
                        listInsert.Add(_mapper.Map(dto));
                    }
                    else
                    {
                        context.Entry(entity).CurrentValues.SetValues(_mapper.Map(dto));
                    }
                }

                context.ItemInstance.AddRange(listInsert);
                context.SaveChanges();
                return true;
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
                return false;
            }
        }

        public IEnumerable<ItemInstanceDTO> LoadAll()
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();

                return _mapper.Map(context.ItemInstance.ToList());
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
                return null;
            }
        }

        public IEnumerable<ItemInstanceDTO> LoadByCharacterId(long characterId)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                var entities = context.ItemInstance.Where(s => s.CharacterId == characterId).ToList();

                return _mapper.Map(entities);
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
                return null;
            }
        }

        public ItemInstanceDTO LoadById(Guid id)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                var entity = context.ItemInstance.FirstOrDefault(s => s.Id == id);

                return _mapper.Map(entity);
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
                return null;
            }
        }

        public IEnumerable<ItemInstanceDTO> LoadByType(long characterId, InventoryType type)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                var entities = context.ItemInstance.Where(s => s.CharacterId == characterId && s.Type == type).ToList();

                return _mapper.Map(entities);
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
                return null;
            }
        }

        public IEnumerable<Guid> LoadSlotAndTypeByCharacterId(long characterId)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                return context.ItemInstance.Where(s => s.CharacterId == characterId).Select(s => s.Id).ToList();
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
                return null;
            }
        }
    }
}