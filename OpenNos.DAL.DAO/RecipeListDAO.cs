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
    public class RecipeListDAO : IRecipeListDAO
    {
        #region Methods

        public RecipeListDTO Insert(RecipeListDTO recipeList)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                var entity = new RecipeList();
                Mapper.Mappers.RecipeListMapper.ToRecipeList(recipeList, entity);
                context.RecipeList.Add(entity);
                context.SaveChanges();
                if (Mapper.Mappers.RecipeListMapper.ToRecipeListDTO(entity, recipeList))
                {
                    return recipeList;
                }

                return null;
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
                return null;
            }
        }

        public IEnumerable<RecipeListDTO> LoadAll()
        {
            using var context = DataAccessHelper.CreateContext();
            var result = new List<RecipeListDTO>();
            foreach (var recipeList in context.RecipeList)
            {
                var dto = new RecipeListDTO();
                Mapper.Mappers.RecipeListMapper.ToRecipeListDTO(recipeList, dto);
                result.Add(dto);
            }
            return result;
        }

        public RecipeListDTO LoadById(int recipeListId)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                var dto = new RecipeListDTO();
                if (Mapper.Mappers.RecipeListMapper.ToRecipeListDTO(context.RecipeList.SingleOrDefault(s => s.RecipeListId.Equals(recipeListId)), dto))
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

        public IEnumerable<RecipeListDTO> LoadByItemVNum(short itemVNum)
        {
            using var context = DataAccessHelper.CreateContext();
            var result = new List<RecipeListDTO>();
            foreach (var recipeList in context.RecipeList.Where(r => r.ItemVNum == itemVNum))
            {
                var dto = new RecipeListDTO();
                Mapper.Mappers.RecipeListMapper.ToRecipeListDTO(recipeList, dto);
                result.Add(dto);
            }
            return result;
        }

        public IEnumerable<RecipeListDTO> LoadByMapNpcId(int mapNpcId)
        {
            using var context = DataAccessHelper.CreateContext();
            var result = new List<RecipeListDTO>();
            foreach (var recipeList in context.RecipeList.Where(r => r.MapNpcId == mapNpcId))
            {
                var dto = new RecipeListDTO();
                Mapper.Mappers.RecipeListMapper.ToRecipeListDTO(recipeList, dto);
                result.Add(dto);
            }
            return result;
        }

        public IEnumerable<RecipeListDTO> LoadByRecipeId(short recipeId)
        {
            using var context = DataAccessHelper.CreateContext();
            var result = new List<RecipeListDTO>();
            foreach (var recipeList in context.RecipeList.Where(r => r.RecipeId.Equals(recipeId)))
            {
                var dto = new RecipeListDTO();
                Mapper.Mappers.RecipeListMapper.ToRecipeListDTO(recipeList, dto);
                result.Add(dto);
            }
            return result;
        }

        public void Update(RecipeListDTO recipe)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                var result = context.RecipeList.FirstOrDefault(r => r.RecipeListId.Equals(recipe.RecipeListId));
                if (result != null)
                {
                    Mapper.Mappers.RecipeListMapper.ToRecipeList(recipe, result);
                    context.SaveChanges();
                }
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
            }
        }

        #endregion
    }
}