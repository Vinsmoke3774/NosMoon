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
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenNos.DAL.DAO
{
    public class MapDAO : IMapDAO
    {
        #region Methods

        public void Insert(List<MapDTO> maps)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                context.Configuration.AutoDetectChangesEnabled = false;
                foreach (var Item in maps)
                {
                    var entity = new Map();
                    Mapper.Mappers.MapMapper.ToMap(Item, entity);
                    context.Map.Add(entity);
                }
                context.Configuration.AutoDetectChangesEnabled = true;
                context.SaveChanges();
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
            }
        }

        public MapDTO Insert(MapDTO map)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                if (context.Map.FirstOrDefault(c => c.MapId.Equals(map.MapId)) == null)
                {
                    var entity = new Map();
                    Mapper.Mappers.MapMapper.ToMap(map, entity);
                    context.Map.Add(entity);
                    context.SaveChanges();
                    if (Mapper.Mappers.MapMapper.ToMapDTO(entity, map))
                    {
                        return map;
                    }

                    return null;
                }
                return new MapDTO();
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
                return null;
            }
        }

        public IEnumerable<MapDTO> LoadAll()
        {
            using var context = DataAccessHelper.CreateContext();
            var result = new List<MapDTO>();
            foreach (var Map in context.Map)
            {
                var dto = new MapDTO();
                Mapper.Mappers.MapMapper.ToMapDTO(Map, dto);
                result.Add(dto);
            }
            return result;
        }

        public MapDTO LoadById(short mapId)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                var dto = new MapDTO();
                if (Mapper.Mappers.MapMapper.ToMapDTO(context.Map.FirstOrDefault(c => c.MapId.Equals(mapId)), dto))
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

        #endregion
    }
}