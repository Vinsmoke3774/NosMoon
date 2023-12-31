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
using OpenNos.Domain;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenNos.DAL.DAO
{
    public class StaticBonusDAO : IStaticBonusDAO
    {
        #region Methods

        public static StaticBonusDTO LoadById(long sbId)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                var dto = new StaticBonusDTO();
                if (Mapper.Mappers.StaticBonusMapper.ToStaticBonusDTO(context.StaticBonus.FirstOrDefault(s => s.StaticBonusId.Equals(sbId)), dto))
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

        public void Delete(short bonusToDelete, long characterId)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                var bon = context.StaticBonus.FirstOrDefault(c => c.StaticBonusType == (StaticBonusType)bonusToDelete && c.CharacterId == characterId);

                if (bon != null)
                {
                    context.StaticBonus.Remove(bon);
                    context.SaveChanges();
                }
            }
            catch (Exception e)
            {
                Logger.Log.Error(string.Format(Language.Instance.GetMessageFromKey("DELETE_ERROR"), bonusToDelete, e.Message), e);
            }
        }

        public SaveResult InsertOrUpdate(ref StaticBonusDTO staticBonus)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                var id = staticBonus.CharacterId;
                var cardid = staticBonus.StaticBonusType;
                var entity = context.StaticBonus.FirstOrDefault(c => c.StaticBonusType == cardid && c.CharacterId == id);

                if (entity == null)
                {
                    staticBonus = insert(staticBonus, context);
                    return SaveResult.Inserted;
                }
                staticBonus.StaticBonusId = entity.StaticBonusId;
                staticBonus = update(entity, staticBonus, context);
                return SaveResult.Updated;
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
                return SaveResult.Error;
            }
        }

        public IEnumerable<StaticBonusDTO> LoadByCharacterId(long characterId)
        {
            using var context = DataAccessHelper.CreateContext();
            var result = new List<StaticBonusDTO>();
            foreach (var entity in context.StaticBonus.Where(i => i.CharacterId == characterId && i.DateEnd > DateTime.Now))
            {
                var dto = new StaticBonusDTO();
                Mapper.Mappers.StaticBonusMapper.ToStaticBonusDTO(entity, dto);
                result.Add(dto);
            }
            return result;
        }

        public IEnumerable<short> LoadTypeByCharacterId(long characterId)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                return context.StaticBonus.Where(i => i.CharacterId == characterId).Select(qle => (short)qle.StaticBonusType).ToList();
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
                return null;
            }
        }

        private static StaticBonusDTO insert(StaticBonusDTO sb, OpenNosContext context)
        {
            try
            {
                var entity = new StaticBonus();
                Mapper.Mappers.StaticBonusMapper.ToStaticBonus(sb, entity);
                context.StaticBonus.Add(entity);
                context.SaveChanges();
                if (Mapper.Mappers.StaticBonusMapper.ToStaticBonusDTO(entity, sb))
                {
                    return sb;
                }

                return null;
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
                return null;
            }
        }

        private static StaticBonusDTO update(StaticBonus entity, StaticBonusDTO sb, OpenNosContext context)
        {
            if (entity != null)
            {
                Mapper.Mappers.StaticBonusMapper.ToStaticBonus(sb, entity);
                context.SaveChanges();
            }
            if (Mapper.Mappers.StaticBonusMapper.ToStaticBonusDTO(entity, sb))
            {
                return sb;
            }

            return null;
        }

        #endregion
    }
}