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
using OpenNos.Data.Comparers;
using OpenNos.Mapper;
using OpenNos.Mapper.Mappers;
using System;
using System.Collections.Generic;
using System.Linq;
using DeleteResult = OpenNos.Data.Enums.DeleteResult;

namespace OpenNos.DAL.DAO
{
    public class ShellEffectDAO : IShellEffectDAO
    {
        private readonly IMapper<ShellEffectDTO, ShellEffect> _mapper = new ShellEffectMapper();

        #region Methods

        public DeleteResult DeleteByEquipmentSerialId(Guid id, bool isRune)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                var ids = context.ShellEffect.Where(s => s.EquipmentSerialId == id && s.IsRune == isRune).ToList();

                context.ShellEffect.BulkDelete(ids);
                return DeleteResult.Deleted;
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
                return DeleteResult.Error;
            }
        }

        public ShellEffectDTO InsertOrUpdate(ShellEffectDTO shellEffect)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                var entity = context.ShellEffect.FirstOrDefault(s => s.ShellEffectId.Equals(shellEffect.ShellEffectId));

                if (entity == null)
                {
                    context.ShellEffect.Add(_mapper.Map(shellEffect));
                }
                else
                {
                    context.Entry(entity).CurrentValues.SetValues(_mapper.Map(shellEffect));
                }

                context.SaveChanges();
                return shellEffect;
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
                return null;
            }
        }

        public DeleteResult DeleteOption(Guid itemId, ShellEffectDTO option, bool isRune)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                var entity = context.ShellEffect.FirstOrDefault(s => s.EquipmentSerialId == itemId && s.IsRune == isRune && s.ShellEffectId == option.ShellEffectId);

                if (entity == null)
                {
                    return DeleteResult.NotFound;
                }

                context.ShellEffect.Remove(entity);
                context.SaveChanges();
                return DeleteResult.Deleted;
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
                return DeleteResult.Error;
            }
        }

        public void InsertFromList(List<ShellEffectDTO> shellEffects, Guid equipmentSerialId)
        {
            var listInsert = new List<ShellEffect>();
            try
            {
                using var context = DataAccessHelper.CreateContext();

                foreach (var effect in shellEffects)
                {
                    effect.EquipmentSerialId = equipmentSerialId;
                    listInsert.Add(_mapper.Map(effect));
                }

                context.ShellEffect.BulkInsert(listInsert);
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
            }
        }

        public void InsertOrUpdateFromList(List<ShellEffectDTO> shellEffects, Guid equipmentSerialId)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();

                void insert(ShellEffectDTO shelleffect)
                {
                    var _entity = _mapper.Map(shelleffect);
                    context.ShellEffect.Add(_entity);
                    context.SaveChanges();
                    shelleffect.ShellEffectId = _entity.ShellEffectId;
                }

                void update(ShellEffect _entity, ShellEffectDTO shelleffect)
                {
                    context.Entry(_entity).CurrentValues.SetValues(_mapper.Map(shelleffect));
                }

                foreach (var item in shellEffects)
                {
                    item.EquipmentSerialId = equipmentSerialId;
                    var entity = context.ShellEffect.FirstOrDefault(c => c.ShellEffectId == item.ShellEffectId);

                    if (entity == null)
                        insert(item);
                    else
                        update(entity, item);
                }

                context.SaveChanges();
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
            }
        }


        public void InsertOrUpdateFromList(IDictionary<Guid, IList<ShellEffectDTO>> effects)
        {
            var listInsert = new List<ShellEffect>();
            var listUpdate = new List<ShellEffect>();
            try
            {
                using var context = DataAccessHelper.CreateContext();
                var keys = effects.Keys.ToList();

                var shellEffects = context.ShellEffect.Where(s => keys.Contains(s.EquipmentSerialId)).ToList();

                foreach (var kvp in effects)
                {
                    var entities = shellEffects.Where(s => s.EquipmentSerialId == kvp.Key).ToList();

                    foreach (var effect in kvp.Value)
                    {
                        effect.EquipmentSerialId = kvp.Key;
                        var entity = entities.FirstOrDefault(s => s.ShellEffectId == effect.ShellEffectId);

                        if (entity == null)
                        {
                            listInsert.Add(_mapper.Map(effect));
                        }
                        else
                        {
                            entity = _mapper.Map(effect);
                            listUpdate.Add(entity);
                        }
                    }
                }

                context.ShellEffect.BulkInsert(listInsert);
                context.ShellEffect.BulkUpdate(listUpdate);
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
            }
        }

        public IEnumerable<ShellEffectDTO> LoadByEquipmentSerialId(Guid id, bool isRune)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();

                var entities = context.ShellEffect.Where(s => s.EquipmentSerialId == id && s.IsRune == isRune).ToList();

                return _mapper.Map(entities).Distinct(new ShellEffectDtoComparer()).ToList();
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
                return null;
            }
        }

        public List<ShellEffectDTO> LoadAll()
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();

                return _mapper.Map(context.ShellEffect.ToList()).ToList();
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
                return null;
            }
        }

        #endregion
    }
}