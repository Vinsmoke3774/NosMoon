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

using OpenNos.Core;
using OpenNos.Core.Logger;
using OpenNos.DAL.EF;
using OpenNos.DAL.EF.Helpers;
using OpenNos.DAL.Interface;
using OpenNos.Data;
using OpenNos.Data.Comparers;
using OpenNos.Data.Enums;
using OpenNos.Mapper.Mappers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenNos.DAL.DAO
{
    public class CellonOptionDAO : ICellonOptionDAO
    {
        #region Methods

        public DeleteResult DeleteByEquipmentSerialId(Guid id)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                var deleteentities = context.CellonOption.Where(s => s.EquipmentSerialId == id).ToList();
                if (deleteentities.Count != 0)
                {
                    context.CellonOption.RemoveRange(deleteentities);
                    context.SaveChanges();
                }

                return DeleteResult.Deleted;
            }
            catch (Exception e)
            {
                Logger.Log.Error(string.Format(Language.Instance.GetMessageFromKey("DELETE_ERROR"), id, e.Message), e);
                return DeleteResult.Error;
            }
        }

        public IEnumerable<CellonOptionDTO> GetOptionsByWearableInstanceId(Guid wearableInstanceId)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                var result = new List<CellonOptionDTO>();
                foreach (var entity in context.CellonOption.Where(c => c.EquipmentSerialId == wearableInstanceId))
                {
                    var dto = new CellonOptionDTO();
                    CellonOptionMapper.ToCellonOptionDTO(entity, dto);
                    result.Add(dto);
                }
                return result.Distinct(new CellonOptionDtoComparer()).ToList();
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
                return null;
            }
        }

        public CellonOptionDTO InsertOrUpdate(CellonOptionDTO cellonOption)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                var cellonOptionId = cellonOption.CellonOptionId;
                var entity = context.CellonOption.FirstOrDefault(c => c.CellonOptionId.Equals(cellonOptionId));

                if (entity == null)
                {
                    return insert(cellonOption, context);
                }
                return update(entity, cellonOption, context);
            }
            catch (Exception e)
            {
                Logger.Log.Error(string.Format(Language.Instance.GetMessageFromKey("INSERT_ERROR"), cellonOption, e.Message), e);
                return cellonOption;
            }
        }

        public void InsertOrUpdateFromList(List<CellonOptionDTO> cellonOption, Guid equipmentSerialId)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();

                void insert(CellonOptionDTO cellonoption)
                {
                    var _entity = new CellonOption();
                    CellonOptionMapper.ToCellonOption(cellonoption, _entity);
                    context.CellonOption.Add(_entity);
                    context.SaveChanges();
                    cellonoption.CellonOptionId = _entity.CellonOptionId;
                }

                void update(CellonOption _entity, CellonOptionDTO cellonoption)
                {
                    if (_entity != null)
                    {
                        CellonOptionMapper.ToCellonOption(cellonoption, _entity);
                    }
                }

                foreach (var item in cellonOption)
                {
                    item.EquipmentSerialId = equipmentSerialId;
                    var entity = context.CellonOption.FirstOrDefault(c => c.CellonOptionId == item.CellonOptionId);

                    if (entity == null)
                    {
                        insert(item);
                    }
                    else
                    {
                        update(entity, item);
                    }
                }

                context.SaveChanges();
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
            }
        }

        public IEnumerable<CellonOptionDTO> LoadAll()
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                var entities = context.CellonOption.ToList();
                var result = new List<CellonOptionDTO>();

                foreach (var entity in entities)
                {
                    var dto = new CellonOptionDTO();
                    CellonOptionMapper.ToCellonOptionDTO(entity, dto);
                    result.Add(dto);
                }

                return result;
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
                return null;
            }
        }

        private static CellonOptionDTO insert(CellonOptionDTO cellonOption, OpenNosContext context)
        {
            var entity = new CellonOption();
            CellonOptionMapper.ToCellonOption(cellonOption, entity);
            context.CellonOption.Add(entity);
            context.SaveChanges();
            if (CellonOptionMapper.ToCellonOptionDTO(entity, cellonOption))
            {
                return cellonOption;
            }

            return null;
        }

        private static CellonOptionDTO update(CellonOption entity, CellonOptionDTO cellonOption, OpenNosContext context)
        {
            if (entity != null)
            {
                CellonOptionMapper.ToCellonOption(cellonOption, entity);
                context.SaveChanges();
            }

            if (CellonOptionMapper.ToCellonOptionDTO(entity, cellonOption))
            {
                return cellonOption;
            }

            return null;
        }

        #endregion
    }
}
