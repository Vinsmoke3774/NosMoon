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
using OpenNos.DAL.EF.Entities;
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
    public class MateDAO : IMateDAO
    {
        private readonly IMapper<MateDTO, Mate> _mapper = new MateMapper();

        #region Methods

        #region Sync

        public DeleteResult DeleteFromList(IEnumerable<long> ids)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                var matesFound = ids.Select(id => context.Mate.FirstOrDefault(c => c.MateId.Equals(id))).Where(mate => mate != null).ToList();

                context.BulkDelete(matesFound);
                return DeleteResult.Deleted;
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
                return DeleteResult.Error;
            }
        }

        public IEnumerable<MateDTO> LoadAll()
        {
            using var context = DataAccessHelper.CreateContext();
            var mateList = new List<Mate>();
            mateList.AddRange(context.Mate);

            return mateList.Select(mate => _mapper.Map(mate)).ToList();
        }

        private void Insert(MateDTO mateDto, ref List<Mate> listInsert)
        {
            Mate ett = _mapper.Map(mateDto);
            listInsert.Add(ett);
        }

        private void Update(Mate entity, MateDTO mateDto, ref List<Mate> listUpdate)
        {
            if (entity != null)
            {
                entity = _mapper.Map(mateDto);
                listUpdate.Add(entity);
            }
        }

        public SaveResult InsertOrUpdateFromList(IEnumerable<MateDTO> mates)
        {
            var listInsert = new List<Mate>();
            var listUpdate = new List<Mate>();
            try
            {
                using var context = DataAccessHelper.CreateContext();

                foreach (var mate in mates)
                {
                    var entity = context.Mate.FirstOrDefault(s => s.MateId == mate.MateId);

                    if (entity == null)
                    {
                        Insert(mate, ref listInsert);
                    }
                    else
                    {
                        Update(entity, mate, ref listUpdate);
                    }
                }

                context.Mate.BulkInsert(listInsert);
                context.Mate.BulkUpdate(listUpdate);
                return SaveResult.Inserted;
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
                return SaveResult.Error;
            }
        }

        public IEnumerable<MateDTO> LoadByCharacterId(long characterId)
        {
            using var context = DataAccessHelper.CreateContext();
            context.Database.CommandTimeout = 1000;
            var result = new List<MateDTO>();
            foreach (var mate in context.Mate.Where(s => s.CharacterId == characterId))
            {
                var dto = new MateDTO();
                dto = _mapper.Map(mate);
                result.Add(dto);
            }
            return result;
        }

        #endregion

        #endregion
    }
}