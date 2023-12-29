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
    public class DropDAO : IDropDAO
    {
        private readonly IMapper<DropDTO, Drop> _mapper = new DropMapper();

        #region Methods

        public void Insert(List<DropDTO> drops)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                context.Drop.BulkInsert(_mapper.Map(drops));
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
            }
        }

        public DropDTO Insert(DropDTO drop)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                context.Drop.Add(_mapper.Map(drop));
                context.SaveChanges();

                return drop;
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
                return null;
            }
        }

        public List<DropDTO> LoadAll()
        {
            using var context = DataAccessHelper.CreateContext();
            var entities = context.Drop.ToList();

            return _mapper.Map(entities).ToList();
        }

        public IEnumerable<DropDTO> LoadByMonster(short monsterVNum)
        {
            using var context = DataAccessHelper.CreateContext();
            var entities = context.Drop.Where(s => s.MonsterVNum == monsterVNum || s.MonsterVNum == null).ToList();

            return _mapper.Map(entities);
        }

        #endregion
    }
}