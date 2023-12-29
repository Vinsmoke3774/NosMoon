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
    public class MapTypeMapDAO : IMapTypeMapDAO
    {
        #region Methods

        public void Insert(List<MapTypeMapDTO> mapTypeMaps)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                context.Configuration.AutoDetectChangesEnabled = false;
                foreach (var mapTypeMap in mapTypeMaps)
                {
                    var entity = new MapTypeMap();
                    Mapper.Mappers.MapTypeMapMapper.ToMapTypeMap(mapTypeMap, entity);
                    context.MapTypeMap.Add(entity);
                }
                context.Configuration.AutoDetectChangesEnabled = true;
                context.SaveChanges();
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
            }
        }

        public IEnumerable<MapTypeMapDTO> LoadAll()
        {
            using var context = DataAccessHelper.CreateContext();
            var result = new List<MapTypeMapDTO>();
            foreach (var MapTypeMap in context.MapTypeMap)
            {
                var dto = new MapTypeMapDTO();
                Mapper.Mappers.MapTypeMapMapper.ToMapTypeMapDTO(MapTypeMap, dto);
                result.Add(dto);
            }
            return result;
        }

        public MapTypeMapDTO LoadByMapAndMapType(short mapId, short maptypeId)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                var dto = new MapTypeMapDTO();
                if (Mapper.Mappers.MapTypeMapMapper.ToMapTypeMapDTO(context.MapTypeMap.FirstOrDefault(i => i.MapId.Equals(mapId) && i.MapTypeId.Equals(maptypeId)), dto))
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

        public IEnumerable<MapTypeMapDTO> LoadByMapId(short mapId)
        {
            using var context = DataAccessHelper.CreateContext();
            var result = new List<MapTypeMapDTO>();
            foreach (var MapTypeMap in context.MapTypeMap.Where(c => c.MapId.Equals(mapId)))
            {
                var dto = new MapTypeMapDTO();
                Mapper.Mappers.MapTypeMapMapper.ToMapTypeMapDTO(MapTypeMap, dto);
                result.Add(dto);
            }
            return result;
        }

        public IEnumerable<MapTypeMapDTO> LoadByMapTypeId(short maptypeId)
        {
            using var context = DataAccessHelper.CreateContext();
            var result = new List<MapTypeMapDTO>();
            foreach (var MapTypeMap in context.MapTypeMap.Where(c => c.MapTypeId.Equals(maptypeId)))
            {
                var dto = new MapTypeMapDTO();
                Mapper.Mappers.MapTypeMapMapper.ToMapTypeMapDTO(MapTypeMap, dto);
                result.Add(dto);
            }
            return result;
        }

        #endregion
    }
}