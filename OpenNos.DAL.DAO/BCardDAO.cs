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
using OpenNos.Mapper;
using OpenNos.Mapper.Mappers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenNos.DAL.DAO
{
    public class BCardDAO : IBCardDAO
    {
        private readonly IMapper<BCardDTO, BCard> _mapper = new BCardMapper();

        #region Methods

        public DeleteResult DeleteByCardId(short cardId)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                var entities = context.BCard.Where(s => s.CardId == cardId).ToList();
                context.BCard.BulkDelete(entities);
                return DeleteResult.Deleted;
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
                return DeleteResult.Error;
            }
        }

        public DeleteResult DeleteByItemVNum(short itemVNum)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                var entities = context.BCard.Where(s => s.ItemVNum == itemVNum).ToList();
                context.BCard.BulkDelete(entities);
                return DeleteResult.Deleted;
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
                return DeleteResult.Error;
            }
        }

        public DeleteResult DeleteByMonsterVNum(short monsterVNum)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                var entities = context.BCard.Where(s => s.NpcMonsterVNum == monsterVNum).ToList();
                context.BCard.BulkDelete(entities);
                return DeleteResult.Deleted;
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
                return DeleteResult.Error;
            }
        }

        public DeleteResult DeleteBySkillVNum(short skillVNum)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                var entities = context.BCard.Where(s => s.SkillVNum == skillVNum);
                context.BCard.BulkDelete(entities);
                return DeleteResult.Deleted;
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
                return DeleteResult.Error;
            }
        }

        public BCardDTO Insert(BCardDTO cardObject)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                var entity = _mapper.Map(cardObject);
                context.BCard.Add(entity);
                context.SaveChanges();
                return _mapper.Map(entity);
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
                return null;
            }
        }

        public void Insert(List<BCardDTO> cards)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                context.BCard.BulkInsert(_mapper.Map(cards));
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
            }
        }

        public IEnumerable<BCardDTO> LoadAll()
        {
            using var context = DataAccessHelper.CreateContext();
            var entities = context.BCard.ToList();
            return _mapper.Map(entities);
        }

        public IEnumerable<BCardDTO> LoadByCardId(short cardId)
        {
            using var context = DataAccessHelper.CreateContext();
            var entities = context.BCard.Where(s => s.CardId == cardId).ToList();
            return _mapper.Map(entities);
        }

        public BCardDTO LoadById(short cardId)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                var entity = context.BCard.FirstOrDefault(s => s.BCardId.Equals(cardId));
               return _mapper.Map(entity);
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
                return null;
            }
        }

        public IEnumerable<BCardDTO> LoadByItemVNum(short vNum)
        {
            using var context = DataAccessHelper.CreateContext();
            var entities = context.BCard.Where(s => s.ItemVNum == vNum).ToList();
            return _mapper.Map(entities);
        }

        public IEnumerable<BCardDTO> LoadByNpcMonsterVNum(short vNum)
        {
            using var context = DataAccessHelper.CreateContext();
            var entities = context.BCard.Where(s => s.NpcMonsterVNum == vNum).ToList();
            return _mapper.Map(entities);
        }

        public IEnumerable<BCardDTO> LoadBySkillVNum(short vNum)
        {
            using var context = DataAccessHelper.CreateContext();
            var entities = context.BCard.Where(s => s.SkillVNum == vNum).ToList();
            return _mapper.Map(entities);
        }

        #endregion
    }
}