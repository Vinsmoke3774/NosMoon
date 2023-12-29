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
using OpenNos.Data.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenNos.DAL.DAO
{
    public class CharacterSkillDAO : ICharacterSkillDAO
    {
        #region Methods

        public DeleteResult Delete(long characterId, short skillVNum)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                var invItem = context.CharacterSkill.FirstOrDefault(i => i.CharacterId == characterId && i.SkillVNum == skillVNum);
                if (invItem != null)
                {
                    context.CharacterSkill.Remove(invItem);
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

        public DeleteResult Delete(Guid id)
        {
            using var context = DataAccessHelper.CreateContext();
            var entity = context.Set<CharacterSkill>().FirstOrDefault(i => i.Id == id);
            if (entity != null)
            {
                context.Set<CharacterSkill>().Remove(entity);
                context.SaveChanges();
            }
            return DeleteResult.Deleted;
        }

        public IEnumerable<CharacterSkillDTO> InsertOrUpdate(IEnumerable<CharacterSkillDTO> dtos)
        {
            try
            {
                IList<CharacterSkillDTO> results = new List<CharacterSkillDTO>();
                using (var context = DataAccessHelper.CreateContext())
                {
                    foreach (var dto in dtos)
                    {
                        results.Add(InsertOrUpdate(context, dto));
                    }
                }
                return results;
            }
            catch (Exception e)
            {
                Logger.Log.Error($"Message: {e.Message}", e);
                return Enumerable.Empty<CharacterSkillDTO>();
            }
        }

        public CharacterSkillDTO InsertOrUpdate(CharacterSkillDTO dto)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                return InsertOrUpdate(context, dto);
            }
            catch (Exception e)
            {
                Logger.Log.Error($"Message: {e.Message}", e);
                return null;
            }
        }

        public IEnumerable<CharacterSkillDTO> LoadByCharacterId(long characterId)
        {
            using var context = DataAccessHelper.CreateContext();
            var result = new List<CharacterSkillDTO>();
            foreach (var entity in context.CharacterSkill.Where(i => i.CharacterId == characterId))
            {
                var output = new CharacterSkillDTO();
                Mapper.Mappers.CharacterSkillMapper.ToCharacterSkillDTO(entity, output);
                result.Add(output);
            }
            return result;
        }

        public CharacterSkillDTO LoadById(Guid id)
        {
            using var context = DataAccessHelper.CreateContext();
            var characterSkillDTO = new CharacterSkillDTO();
            if (Mapper.Mappers.CharacterSkillMapper.ToCharacterSkillDTO(context.CharacterSkill.FirstOrDefault(i => i.Id.Equals(id)), characterSkillDTO))
            {
                return characterSkillDTO;
            }

            return null;
        }

        public IEnumerable<Guid> LoadKeysByCharacterId(long characterId)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                return context.CharacterSkill.Where(i => i.CharacterId == characterId).Select(c => c.Id).ToList();
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
                return null;
            }
        }

        protected static CharacterSkillDTO Insert(CharacterSkillDTO dto, OpenNosContext context)
        {
            var entity = new CharacterSkill();
            Mapper.Mappers.CharacterSkillMapper.ToCharacterSkill(dto, entity);
            context.Set<CharacterSkill>().Add(entity);
            context.SaveChanges();
            if (Mapper.Mappers.CharacterSkillMapper.ToCharacterSkillDTO(entity, dto))
            {
                return dto;
            }

            return null;
        }

        protected static CharacterSkillDTO InsertOrUpdate(OpenNosContext context, CharacterSkillDTO dto)
        {
            var primaryKey = dto.Id;
            var entity = context.Set<CharacterSkill>().FirstOrDefault(c => c.Id == primaryKey);
            if (entity == null)
            {
                return Insert(dto, context);
            }
            else
            {
                return Update(entity, dto, context);
            }
        }

        protected static CharacterSkillDTO Update(CharacterSkill entity, CharacterSkillDTO inventory, OpenNosContext context)
        {
            if (entity != null)
            {
                Mapper.Mappers.CharacterSkillMapper.ToCharacterSkill(inventory, entity);
                context.SaveChanges();
            }
            if (Mapper.Mappers.CharacterSkillMapper.ToCharacterSkillDTO(entity, inventory))
            {
                return inventory;
            }

            return null;
        }

        #endregion
    }
}