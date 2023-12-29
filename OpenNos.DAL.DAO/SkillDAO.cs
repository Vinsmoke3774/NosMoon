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
using System.Linq;

namespace OpenNos.DAL.DAO
{
    public class SkillDAO : ISkillDAO
    {
        #region Methods

        public void Insert(List<SkillDTO> skills)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                context.Configuration.AutoDetectChangesEnabled = false;
                foreach (var skill in skills)
                {
                    InsertOrUpdate(skill);
                }
                context.Configuration.AutoDetectChangesEnabled = true;
                context.SaveChanges();
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
            }
        }

        public SkillDTO Insert(SkillDTO skill)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                var entity = new Skill();
                Mapper.Mappers.SkillMapper.ToSkill(skill, entity); context.Skill.Add(entity);
                context.SaveChanges();
                if (Mapper.Mappers.SkillMapper.ToSkillDTO(entity, skill))
                {
                    return skill;
                }

                return null;
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
                return null;
            }
        }

        public SaveResult InsertOrUpdate(SkillDTO skill)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                long SkillVNum = skill.SkillVNum;
                var entity = context.Skill.FirstOrDefault(c => c.SkillVNum == SkillVNum);

                if (entity == null)
                {
                    skill = insert(skill, context);
                    return SaveResult.Inserted;
                }

                skill = update(entity, skill, context);
                return SaveResult.Updated;
            }
            catch (Exception e)
            {
                Logger.Log.Error(string.Format(Language.Instance.GetMessageFromKey("UPDATE_SKILL_ERROR"), skill.SkillVNum, e.Message), e);
                return SaveResult.Error;
            }
        }

        public IEnumerable<SkillDTO> LoadAll()
        {
            using var context = DataAccessHelper.CreateContext();
            var result = new List<SkillDTO>();
            foreach (var Skill in context.Skill)
            {
                var dto = new SkillDTO();
                Mapper.Mappers.SkillMapper.ToSkillDTO(Skill, dto);
                result.Add(dto);
            }
            return result;
        }

        public SkillDTO LoadById(short skillId)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                var dto = new SkillDTO();
                if (Mapper.Mappers.SkillMapper.ToSkillDTO(context.Skill.FirstOrDefault(s => s.SkillVNum.Equals(skillId)), dto))
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

        private static SkillDTO insert(SkillDTO skill, OpenNosContext context)
        {
            var entity = new Skill();
            Mapper.Mappers.SkillMapper.ToSkill(skill, entity);
            context.Skill.Add(entity);
            context.SaveChanges();
            if (Mapper.Mappers.SkillMapper.ToSkillDTO(entity, skill))
            {
                return skill;
            }

            return null;
        }

        private static SkillDTO update(Skill entity, SkillDTO skill, OpenNosContext context)
        {
            if (entity != null)
            {
                Mapper.Mappers.SkillMapper.ToSkill(skill, entity);
                context.SaveChanges();
            }

            if (Mapper.Mappers.SkillMapper.ToSkillDTO(entity, skill))
            {
                return skill;
            }

            return null;
        }

        #endregion
    }
}