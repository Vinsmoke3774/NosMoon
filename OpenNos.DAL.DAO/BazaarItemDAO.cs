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
using System.Data.Entity;
using System.Linq;

namespace OpenNos.DAL.DAO
{
    public class BazaarItemDAO : IBazaarItemDAO
    {
        #region Methods

        public DeleteResult Delete(long bazaarItemId)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                var BazaarItem = context.BazaarItem.FirstOrDefault(c => c.BazaarItemId.Equals(bazaarItemId));

                if (BazaarItem != null)
                {
                    context.BazaarItem.Remove(BazaarItem);
                    context.SaveChanges();
                }

                return DeleteResult.Deleted;
            }
            catch (Exception e)
            {
                Logger.Log.Error(string.Format(Language.Instance.GetMessageFromKey("DELETE_ERROR"), bazaarItemId, e.Message), e);
                return DeleteResult.Error;
            }
        }

        public DeleteResult DeleteByItemId(Guid bazaarItemId)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                var BazaarItem = context.BazaarItem.FirstOrDefault(c => c.ItemInstanceId.Equals(bazaarItemId));

                if (BazaarItem != null)
                {
                    context.BazaarItem.Remove(BazaarItem);
                    context.SaveChanges();
                }

                return DeleteResult.Deleted;
            }
            catch (Exception e)
            {
                Logger.Log.Error(string.Format(Language.Instance.GetMessageFromKey("DELETE_ERROR"), bazaarItemId, e.Message), e);
                return DeleteResult.Error;
            }
        }

        public SaveResult InsertOrUpdate(ref BazaarItemDTO bazaarItem)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                var bazaarItemId = bazaarItem.BazaarItemId;
                var entity = context.BazaarItem.FirstOrDefault(c => c.BazaarItemId.Equals(bazaarItemId));

                if (entity == null)
                {
                    bazaarItem = insert(bazaarItem, context);
                    return SaveResult.Inserted;
                }

                bazaarItem = update(entity, bazaarItem, context);
                return SaveResult.Updated;
            }
            catch (Exception e)
            {
                Logger.Log.Error($"BazaarItemId: {bazaarItem.BazaarItemId} Message: {e.Message}", e);
                return SaveResult.Error;
            }
        }

        public IEnumerable<BazaarItemDTO> LoadAll()
        {
            using var context = DataAccessHelper.CreateContext();
            var result = new List<BazaarItemDTO>();
            foreach (var bazaarItem in context.BazaarItem)
            {
                var dto = new BazaarItemDTO();
                Mapper.Mappers.BazaarItemMapper.ToBazaarItemDTO(bazaarItem, dto);
                result.Add(dto);
            }
            return result;
        }

        public BazaarItemDTO LoadById(long bazaarItemId)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                var dto = new BazaarItemDTO();
                if (Mapper.Mappers.BazaarItemMapper.ToBazaarItemDTO(context.BazaarItem.FirstOrDefault(i => i.BazaarItemId.Equals(bazaarItemId)), dto))
                {
                    return dto;
                }

                return null;
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
                return null;
            }
        }

        public void RemoveOutDated()
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                foreach (var entity in context.BazaarItem.Where(e => DbFunctions.AddDays(DbFunctions.AddHours(e.DateStart, e.Duration), e.MedalUsed ? 30 : 7) < DateTime.Now))
                {
                    context.BazaarItem.Remove(entity);
                }
                context.SaveChanges();
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
            }
        }

        public IEnumerable<BazaarItemDTO> LoadByCharacterId(long characterId)
        {
            var result = new List<BazaarItemDTO>();
            try
            {
                using var context = DataAccessHelper.CreateContext();
                var bazaarItems = context.BazaarItem.Where(i => i.SellerId.Equals(characterId));

                var dto = new BazaarItemDTO();

                foreach (var item in bazaarItems)
                {
                    if (Mapper.Mappers.BazaarItemMapper.ToBazaarItemDTO(item, dto))
                    {
                        result.Add(dto);
                    }
                }

                return result;
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
                return null;
            }
        }

        private static BazaarItemDTO insert(BazaarItemDTO bazaarItem, OpenNosContext context)
        {
            var entity = new BazaarItem();
            Mapper.Mappers.BazaarItemMapper.ToBazaarItem(bazaarItem, entity);
            context.BazaarItem.Add(entity);
            context.SaveChanges();
            if (Mapper.Mappers.BazaarItemMapper.ToBazaarItemDTO(entity, bazaarItem))
            {
                return bazaarItem;
            }

            return null;
        }

        private static BazaarItemDTO update(BazaarItem entity, BazaarItemDTO bazaarItem, OpenNosContext context)
        {
            if (entity != null)
            {
                Mapper.Mappers.BazaarItemMapper.ToBazaarItem(bazaarItem, entity);
                context.SaveChanges();
            }
            if (Mapper.Mappers.BazaarItemMapper.ToBazaarItemDTO(entity, bazaarItem))
            {
                return bazaarItem;
            }

            return null;
        }

        #endregion
    }
}