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
    public class ScriptedInstanceDAO : IScriptedInstanceDAO
    {
        #region Methods

        public void Insert(List<ScriptedInstanceDTO> scriptedInstances)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                context.Configuration.AutoDetectChangesEnabled = false;
                foreach (var scriptedInstance in scriptedInstances)
                {
                    var entity = new ScriptedInstance();
                    Mapper.Mappers.ScriptedInstanceMapper.ToScriptedInstance(scriptedInstance, entity);
                    context.ScriptedInstance.Add(entity);
                }
                context.Configuration.AutoDetectChangesEnabled = true;
                context.SaveChanges();
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
            }
        }

        public ScriptedInstanceDTO Insert(ScriptedInstanceDTO scriptedInstance)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                var entity = new ScriptedInstance();
                Mapper.Mappers.ScriptedInstanceMapper.ToScriptedInstance(scriptedInstance, entity);
                context.ScriptedInstance.Add(entity);
                context.SaveChanges();
                if (Mapper.Mappers.ScriptedInstanceMapper.ToScriptedInstanceDTO(entity, scriptedInstance))
                {
                    return scriptedInstance;
                }

                return null;
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
                return null;
            }
        }

        public IEnumerable<ScriptedInstanceDTO> LoadByMap(short mapId)
        {
            using var context = DataAccessHelper.CreateContext();
            var result = new List<ScriptedInstanceDTO>();
            foreach (var timespaceObject in context.ScriptedInstance.Where(c => c.MapId.Equals(mapId)))
            {
                var dto = new ScriptedInstanceDTO();
                Mapper.Mappers.ScriptedInstanceMapper.ToScriptedInstanceDTO(timespaceObject, dto);
                result.Add(dto);
            }
            return result;
        }

        #endregion
    }
}