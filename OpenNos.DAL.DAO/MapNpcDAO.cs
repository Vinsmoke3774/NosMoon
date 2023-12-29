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
    public class MapNpcDAO : IMapNpcDAO
    {
        #region Methods

        public DeleteResult DeleteById(int mapNpcId)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                var npc = context.MapNpc.First(i => i.MapNpcId.Equals(mapNpcId));

                if (npc != null)
                {
                    context.MapNpc.Remove(npc);
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

        public bool DoesNpcExist(int mapNpcId)
        {
            using var context = DataAccessHelper.CreateContext();
            return context.MapNpc.Any(i => i.MapNpcId.Equals(mapNpcId));
        }

        public void Insert(List<MapNpcDTO> npcs)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                context.Configuration.AutoDetectChangesEnabled = false;
                foreach (var Item in npcs)
                {
                    var entity = new MapNpc();
                    Mapper.Mappers.MapNpcMapper.ToMapNpc(Item, entity);
                    context.MapNpc.Add(entity);
                }
                context.Configuration.AutoDetectChangesEnabled = true;
                context.SaveChanges();
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
            }
        }

        public MapNpcDTO Insert(MapNpcDTO npc)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                var entity = new MapNpc();
                Mapper.Mappers.MapNpcMapper.ToMapNpc(npc, entity);
                context.MapNpc.Add(entity);
                context.SaveChanges();
                if (Mapper.Mappers.MapNpcMapper.ToMapNpcDTO(entity, npc))
                {
                    return npc;
                }

                return null;
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
                return null;
            }
        }

        public IEnumerable<MapNpcDTO> LoadAll()
        {
            using var context = DataAccessHelper.CreateContext();
            var result = new List<MapNpcDTO>();
            foreach (var entity in context.MapNpc)
            {
                var dto = new MapNpcDTO();
                Mapper.Mappers.MapNpcMapper.ToMapNpcDTO(entity, dto);
                result.Add(dto);
            }
            return result;
        }

        public MapNpcDTO LoadById(int mapNpcId)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                var dto = new MapNpcDTO();
                if (Mapper.Mappers.MapNpcMapper.ToMapNpcDTO(context.MapNpc.FirstOrDefault(i => i.MapNpcId.Equals(mapNpcId)), dto))
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

        public IEnumerable<MapNpcDTO> LoadFromMap(short mapId)
        {
            using var context = DataAccessHelper.CreateContext();
            var result = new List<MapNpcDTO>();
            foreach (var npcobject in context.MapNpc.Where(c => c.MapId.Equals(mapId)))
            {
                var dto = new MapNpcDTO();
                Mapper.Mappers.MapNpcMapper.ToMapNpcDTO(npcobject, dto);
                result.Add(dto);
            }
            return result;
        }

        public SaveResult Update(ref MapNpcDTO mapNpc)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                var mapNpcId = mapNpc.MapNpcId;
                var entity = context.MapNpc.FirstOrDefault(c => c.MapNpcId.Equals(mapNpcId));

                mapNpc = update(entity, mapNpc, context);
                return SaveResult.Updated;
            }
            catch (Exception e)
            {
                Logger.Log.Error(string.Format(Language.Instance.GetMessageFromKey("UPDATE_MAPNPC_ERROR"), mapNpc.MapNpcId, e.Message), e);
                return SaveResult.Error;
            }
        }

        private static MapNpcDTO update(MapNpc entity, MapNpcDTO mapNpc, OpenNosContext context)
        {
            if (entity != null)
            {
                Mapper.Mappers.MapNpcMapper.ToMapNpc(mapNpc, entity);
                context.Entry(entity).State = EntityState.Modified;
                context.SaveChanges();
            }
            if (Mapper.Mappers.MapNpcMapper.ToMapNpcDTO(entity, mapNpc))
            {
                return mapNpc;
            }

            return null;
        }

        #endregion
    }
}