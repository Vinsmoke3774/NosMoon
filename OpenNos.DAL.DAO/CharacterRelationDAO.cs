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
    public class CharacterRelationDAO : ICharacterRelationDAO
    {
        #region Methods

        public DeleteResult Delete(long characterRelationId)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                var relation = context.CharacterRelation.SingleOrDefault(c => c.CharacterRelationId.Equals(characterRelationId));

                if (relation != null)
                {
                    context.CharacterRelation.Remove(relation);
                    context.SaveChanges();
                }

                return DeleteResult.Deleted;
            }
            catch (Exception e)
            {
                Logger.Log.Error(string.Format(Language.Instance.GetMessageFromKey("DELETE_CHARACTER_ERROR"), characterRelationId, e.Message), e);
                return DeleteResult.Error;
            }
        }

        public SaveResult InsertOrUpdate(ref CharacterRelationDTO characterRelation)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                var characterId = characterRelation.CharacterId;
                var relatedCharacterId = characterRelation.RelatedCharacterId;
                var entity = context.CharacterRelation.FirstOrDefault(c => c.CharacterId.Equals(characterId) && c.RelatedCharacterId.Equals(relatedCharacterId));

                if (entity == null)
                {
                    characterRelation = insert(characterRelation, context);
                    return SaveResult.Inserted;
                }
                characterRelation.CharacterRelationId = entity.CharacterRelationId;
                characterRelation = update(entity, characterRelation, context);
                return SaveResult.Updated;
            }
            catch (Exception e)
            {
                Logger.Log.Error(string.Format(Language.Instance.GetMessageFromKey("UPDATE_CHARACTERRELATION_ERROR"), characterRelation.CharacterRelationId, e.Message), e);
                return SaveResult.Error;
            }
        }

        public IEnumerable<CharacterRelationDTO> LoadAll()
        {
            using var context = DataAccessHelper.CreateContext();
            var result = new List<CharacterRelationDTO>();
            foreach (var entity in context.CharacterRelation)
            {
                var dto = new CharacterRelationDTO();
                Mapper.Mappers.CharacterRelationMapper.ToCharacterRelationDTO(entity, dto);
                result.Add(dto);
            }
            return result;
        }

        public CharacterRelationDTO LoadById(long characterId)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                var dto = new CharacterRelationDTO();
                if (Mapper.Mappers.CharacterRelationMapper.ToCharacterRelationDTO(context.CharacterRelation.FirstOrDefault(s => s.CharacterRelationId.Equals(characterId)), dto))
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

        private static CharacterRelationDTO insert(CharacterRelationDTO relation, OpenNosContext context)
        {
            var entity = new CharacterRelation();
            Mapper.Mappers.CharacterRelationMapper.ToCharacterRelation(relation, entity);
            context.CharacterRelation.Add(entity);
            context.SaveChanges();
            if (Mapper.Mappers.CharacterRelationMapper.ToCharacterRelationDTO(entity, relation))
            {
                return relation;
            }

            return null;
        }

        private static CharacterRelationDTO update(CharacterRelation entity, CharacterRelationDTO relation, OpenNosContext context)
        {
            if (entity != null)
            {
                Mapper.Mappers.CharacterRelationMapper.ToCharacterRelation(relation, entity);
                context.SaveChanges();
            }

            if (Mapper.Mappers.CharacterRelationMapper.ToCharacterRelationDTO(entity, relation))
            {
                return relation;
            }

            return null;
        }

        #endregion
    }
}