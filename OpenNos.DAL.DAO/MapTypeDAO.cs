﻿using OpenNos.Core.Logger;
using OpenNos.DAL.EF;
using OpenNos.DAL.EF.Helpers;
using OpenNos.DAL.Interface;
using OpenNos.Data;
using OpenNos.Mapper.Mappers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenNos.DAL.DAO
{
    public class MapTypeDAO : IMapTypeDAO
    {
        #region Methods

        public void Insert(List<MapTypeDTO> mapType)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    context.Configuration.AutoDetectChangesEnabled = false;
                    foreach (var RespawnMapType in mapType)
                    {
                        var entity = new MapType();
                        MapTypeMapper.ToMapType(RespawnMapType, entity);
                        context.MapType.Add(entity);
                    }

                    context.Configuration.AutoDetectChangesEnabled = true;
                    context.SaveChanges();
                }
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
            }
        }

        public MapTypeDTO Insert(ref MapTypeDTO mapType)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    var entity = new MapType();
                    MapTypeMapper.ToMapType(mapType, entity);
                    context.MapType.Add(entity);
                    context.SaveChanges();
                    if (MapTypeMapper.ToMapTypeDTO(entity, mapType)) return mapType;

                    return null;
                }
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
                return null;
            }
        }

        public IEnumerable<MapTypeDTO> LoadAll()
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                var result = new List<MapTypeDTO>();
                foreach (var MapType in context.MapType)
                {
                    var dto = new MapTypeDTO();
                    MapTypeMapper.ToMapTypeDTO(MapType, dto);
                    result.Add(dto);
                }

                return result;
            }
        }

        public MapTypeDTO LoadById(short maptypeId)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    var dto = new MapTypeDTO();
                    if (MapTypeMapper.ToMapTypeDTO(context.MapType.FirstOrDefault(s => s.MapTypeId.Equals(maptypeId)),
                        dto)) return dto;

                    return null;
                }
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