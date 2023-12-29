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
    public class RecipeItemDAO : IRecipeItemDAO
    {
        #region Methods

        public RecipeItemDTO Insert(RecipeItemDTO recipeItem)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                var entity = new RecipeItem();
                Mapper.Mappers.RecipeItemMapper.ToRecipeItem(recipeItem, entity);
                context.RecipeItem.Add(entity);
                context.SaveChanges();
                if (Mapper.Mappers.RecipeItemMapper.ToRecipeItemDTO(entity, recipeItem))
                {
                    return recipeItem;
                }

                return null;
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
                return null;
            }
        }

        public IEnumerable<RecipeItemDTO> LoadAll()
        {
            using var context = DataAccessHelper.CreateContext();
            var result = new List<RecipeItemDTO>();
            foreach (var recipeItem in context.RecipeItem)
            {
                var dto = new RecipeItemDTO();
                Mapper.Mappers.RecipeItemMapper.ToRecipeItemDTO(recipeItem, dto);
                result.Add(dto);
            }
            return result;
        }

        public RecipeItemDTO LoadById(short recipeItemId)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                var dto = new RecipeItemDTO();
                if (Mapper.Mappers.RecipeItemMapper.ToRecipeItemDTO(context.RecipeItem.FirstOrDefault(s => s.RecipeItemId.Equals(recipeItemId)), dto))
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

        public IEnumerable<RecipeItemDTO> LoadByRecipe(short recipeId)
        {
            using var context = DataAccessHelper.CreateContext();
            var result = new List<RecipeItemDTO>();
            foreach (var recipeItem in context.RecipeItem.Where(s => s.RecipeId.Equals(recipeId)))
            {
                var dto = new RecipeItemDTO();
                Mapper.Mappers.RecipeItemMapper.ToRecipeItemDTO(recipeItem, dto);
                result.Add(dto);
            }
            return result;
        }

        public IEnumerable<RecipeItemDTO> LoadByRecipeAndItem(short recipeId, short itemVNum)
        {
            using var context = DataAccessHelper.CreateContext();
            var result = new List<RecipeItemDTO>();
            foreach (var recipeItem in context.RecipeItem.Where(s => s.ItemVNum.Equals(itemVNum) && s.RecipeId.Equals(recipeId)))
            {
                var dto = new RecipeItemDTO();
                Mapper.Mappers.RecipeItemMapper.ToRecipeItemDTO(recipeItem, dto);
                result.Add(dto);
            }
            return result;
        }

        #endregion
    }
}