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
    public class NpcMonsterSkillDAO : INpcMonsterSkillDAO
    {
        private readonly IMapper<NpcMonsterSkillDTO, NpcMonsterSkill> _mapper = new NpcMonsterSkillMapper();

        #region Methods

        public NpcMonsterSkillDTO Insert(NpcMonsterSkillDTO npcMonsterSkill)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                var entity = _mapper.Map(npcMonsterSkill);
                context.NpcMonsterSkill.Add(entity);
                context.SaveChanges();
                return _mapper.Map(entity);
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
                return null;
            }
        }

        public void Insert(List<NpcMonsterSkillDTO> skills)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                var entities = _mapper.Map(skills);
                context.NpcMonsterSkill.BulkInsert(entities);
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
            }
        }

        public List<NpcMonsterSkillDTO> LoadAll()
        {
            using var context = DataAccessHelper.CreateContext();
            var entities = context.NpcMonsterSkill.ToList();
            return _mapper.Map(entities).ToList();
        }

        public IEnumerable<NpcMonsterSkillDTO> LoadByNpcMonster(short npcId)
        {
            using var context = DataAccessHelper.CreateContext();
            var entities = context.NpcMonsterSkill.Where(s => s.NpcMonsterVNum == npcId).ToList();
            return _mapper.Map(entities);
        }

        #endregion
    }
}