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
using OpenNos.Mapper;
using OpenNos.Mapper.Mappers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenNos.DAL.DAO
{
    public class ItemDAO : IItemDAO
    {
        private readonly IMapper<ItemDTO, Item> _mapper = new ItemMapper();

        #region Methods

        public IEnumerable<ItemDTO> FindByName(string name)
        {
            using var context = DataAccessHelper.CreateContext();
            var entities = context.Item.Where(s => string.IsNullOrEmpty(name) ? s.Name.Equals("") : s.Name.Contains(name)).ToList();
            return _mapper.Map(entities);
        }

        public void Insert(IEnumerable<ItemDTO> items)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                context.Item.BulkInsert(_mapper.Map(items));
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
            }
        }

        public ItemDTO Insert(ItemDTO item)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                var entity = _mapper.Map(item);
                context.Item.Add(entity);
                context.SaveChanges();
                return item;
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
                return null;
            }
        }

        public IEnumerable<ItemDTO> LoadAll()
        {
            using var context = DataAccessHelper.CreateContext();
            var entities = context.Item.ToList();
            return _mapper.Map(entities);
        }

        public ItemDTO LoadById(short vNum)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                var entity = context.Item.FirstOrDefault(i => i.VNum.Equals(vNum));
                return _mapper.Map(entity);
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
                return null;
            }
        }

        #endregion
    }
}