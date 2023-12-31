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
    public class ComboDAO : IComboDAO
    {
        #region Methods

        public void Insert(List<ComboDTO> combos)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                context.Configuration.AutoDetectChangesEnabled = false;
                foreach (var combo in combos)
                {
                    var entity = new Combo();
                    Mapper.Mappers.ComboMapper.ToCombo(combo, entity);
                    context.Combo.Add(entity);
                }
                context.Configuration.AutoDetectChangesEnabled = true;
                context.SaveChanges();
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
            }
        }

        public ComboDTO Insert(ComboDTO combo)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                var entity = new Combo();
                Mapper.Mappers.ComboMapper.ToCombo(combo, entity);
                context.Combo.Add(entity);
                context.SaveChanges();
                if (Mapper.Mappers.ComboMapper.ToComboDTO(entity, combo))
                {
                    return combo;
                }

                return null;
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
                return null;
            }
        }

        public IEnumerable<ComboDTO> LoadAll()
        {
            using var context = DataAccessHelper.CreateContext();
            var result = new List<ComboDTO>();
            foreach (var combo in context.Combo)
            {
                var dto = new ComboDTO();
                Mapper.Mappers.ComboMapper.ToComboDTO(combo, dto);
                result.Add(dto);
            }
            return result;
        }

        public ComboDTO LoadById(short comboId)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                var dto = new ComboDTO();
                if (Mapper.Mappers.ComboMapper.ToComboDTO(context.Combo.FirstOrDefault(s => s.SkillVNum.Equals(comboId)), dto))
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

        public IEnumerable<ComboDTO> LoadBySkillVnum(short skillVNum)
        {
            using var context = DataAccessHelper.CreateContext();
            var result = new List<ComboDTO>();
            foreach (var combo in context.Combo.Where(c => c.SkillVNum == skillVNum))
            {
                var dto = new ComboDTO();
                Mapper.Mappers.ComboMapper.ToComboDTO(combo, dto);
                result.Add(dto);
            }
            return result;
        }

        public IEnumerable<ComboDTO> LoadByVNumHitAndEffect(short skillVNum, short hit, short effect)
        {
            using var context = DataAccessHelper.CreateContext();
            var result = new List<ComboDTO>();
            foreach (var combo in context.Combo.Where(s => s.SkillVNum == skillVNum && s.Hit == hit && s.Effect == effect))
            {
                var dto = new ComboDTO();
                Mapper.Mappers.ComboMapper.ToComboDTO(combo, dto);
                result.Add(dto);
            }
            return result;
        }

        #endregion
    }
}