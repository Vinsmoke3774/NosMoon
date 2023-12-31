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
    public class ShopItemDAO : IShopItemDAO
    {
        #region Methods

        public DeleteResult DeleteById(int itemId)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                var Item = context.ShopItem.FirstOrDefault(i => i.ShopItemId.Equals(itemId));

                if (Item != null)
                {
                    context.ShopItem.Remove(Item);
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

        public ShopItemDTO Insert(ShopItemDTO item)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                var entity = new ShopItem();
                Mapper.Mappers.ShopItemMapper.ToShopItem(item, entity);
                context.ShopItem.Add(entity);
                context.SaveChanges();
                if (Mapper.Mappers.ShopItemMapper.ToShopItemDTO(entity, item))
                {
                    return item;
                }

                return null;
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
                return null;
            }
        }

        public void Insert(List<ShopItemDTO> items)
        {
            foreach (var item in items)
            {
                Insert(item);
            }
        }

        public IEnumerable<ShopItemDTO> LoadAll()
        {
            using var context = DataAccessHelper.CreateContext();
            var result = new List<ShopItemDTO>();
            foreach (var entity in context.ShopItem)
            {
                var dto = new ShopItemDTO();
                Mapper.Mappers.ShopItemMapper.ToShopItemDTO(entity, dto);
                result.Add(dto);
            }
            return result;
        }

        public ShopItemDTO LoadById(int itemId)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                var dto = new ShopItemDTO();
                if (Mapper.Mappers.ShopItemMapper.ToShopItemDTO(context.ShopItem.FirstOrDefault(i => i.ShopItemId.Equals(itemId)), dto))
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

        public IEnumerable<ShopItemDTO> LoadByShopId(int shopId)
        {
            using var context = DataAccessHelper.CreateContext();
            var result = new List<ShopItemDTO>();
            foreach (var ShopItem in context.ShopItem.Where(i => i.ShopId.Equals(shopId)))
            {
                var dto = new ShopItemDTO();
                Mapper.Mappers.ShopItemMapper.ToShopItemDTO(ShopItem, dto);
                result.Add(dto);
            }
            return result;
        }

        #endregion
    }
}