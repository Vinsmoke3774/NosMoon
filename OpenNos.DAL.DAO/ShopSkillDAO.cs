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
    public class ShopSkillDAO : IShopSkillDAO
    {
        #region Methods

        public ShopSkillDTO Insert(ShopSkillDTO shopSkill)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                var entity = new ShopSkill();
                Mapper.Mappers.ShopSkillMapper.ToShopSkill(shopSkill, entity);
                context.ShopSkill.Add(entity);
                context.SaveChanges();
                if (Mapper.Mappers.ShopSkillMapper.ToShopSkillDTO(entity, shopSkill))
                {
                    return shopSkill;
                }

                return null;
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
                return null;
            }
        }

        public void Insert(List<ShopSkillDTO> skills)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                context.Configuration.AutoDetectChangesEnabled = false;
                foreach (var Skill in skills)
                {
                    var entity = new ShopSkill();
                    Mapper.Mappers.ShopSkillMapper.ToShopSkill(Skill, entity);
                    context.ShopSkill.Add(entity);
                }
                context.Configuration.AutoDetectChangesEnabled = true;
                context.SaveChanges();
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
            }
        }

        public IEnumerable<ShopSkillDTO> LoadAll()
        {
            using var context = DataAccessHelper.CreateContext();
            var result = new List<ShopSkillDTO>();
            foreach (var entity in context.ShopSkill)
            {
                var dto = new ShopSkillDTO();
                Mapper.Mappers.ShopSkillMapper.ToShopSkillDTO(entity, dto);
                result.Add(dto);
            }
            return result;
        }

        public IEnumerable<ShopSkillDTO> LoadByShopId(int shopId)
        {
            using var context = DataAccessHelper.CreateContext();
            var result = new List<ShopSkillDTO>();
            foreach (var ShopSkill in context.ShopSkill.Where(s => s.ShopId.Equals(shopId)))
            {
                var dto = new ShopSkillDTO();
                Mapper.Mappers.ShopSkillMapper.ToShopSkillDTO(ShopSkill, dto);
                result.Add(dto);
            }
            return result;
        }

        #endregion
    }
}