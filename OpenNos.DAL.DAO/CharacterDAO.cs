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
using OpenNos.Data.Enums;
using OpenNos.Domain;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenNos.DAL.DAO
{
    public class CharacterDAO : ICharacterDAO
    {
        #region Methods

        public DeleteResult DeleteByPrimaryKey(long accountId, byte characterSlot)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                // actually a Character wont be deleted, it just will be disabled for future traces
                var character = context.Character.SingleOrDefault(c => c.AccountId.Equals(accountId) && c.Slot.Equals(characterSlot) && c.State.Equals((byte)CharacterState.Active));

                if (character != null)
                {
                    if (character.Level <= 15)
                    {
                        character.State = (byte)CharacterState.Inactive;
                        character.Name = $"[DELETED]{character.Name + 1}";
                        character.Inventory.Clear();
                        character.Slot = 5;
                        context.SaveChanges();
                    }
                    character.State = (byte)CharacterState.Inactive;
                    character.Name = $"[DELETED]{character.Name + 1}";
                    character.Slot = 5;
                    context.SaveChanges();
                }

                return DeleteResult.Deleted;
            }
            catch (Exception e)
            {
                Logger.Log.Error(string.Format(Language.Instance.GetMessageFromKey("DELETE_CHARACTER_ERROR"), characterSlot, e.Message), e);
                return DeleteResult.Error;
            }
        }

        /// <summary>
        /// Returns first 30 occurences of highest Compliment
        /// </summary>
        /// <returns></returns>
        public List<CharacterDTO> GetTopCompliment()
        {
            using var context = DataAccessHelper.CreateContext();
            var result = new List<CharacterDTO>();
            foreach (var entity in context.Character.Where(c => c.State == (byte)CharacterState.Active && c.Account.Authority >= AuthorityType.User && c.Account.Authority <= AuthorityType.GS && !c.Account.PenaltyLog.Any(l => l.Penalty == PenaltyType.Banned && l.DateEnd > DateTime.Now)).OrderByDescending(c => c.Compliment).Take(30))
            {
                var dto = new CharacterDTO();
                Mapper.Mappers.CharacterMapper.ToCharacterDTO(entity, dto);
                result.Add(dto);
            }
            return result;
        }

        /// <summary>
        /// Returns first 30 occurences of highest Act4Points
        /// </summary>
        /// <returns></returns>
        public List<CharacterDTO> GetTopPoints()
        {
            using var context = DataAccessHelper.CreateContext();
            var result = new List<CharacterDTO>();
            foreach (var entity in context.Character.Where(c => c.State == (byte)CharacterState.Active && c.Account.Authority >= AuthorityType.User && c.Account.Authority <= AuthorityType.GS && !c.Account.PenaltyLog.Any(l => l.Penalty == PenaltyType.Banned && l.DateEnd > DateTime.Now)).OrderByDescending(c => c.Act4Points).Take(30))
            {
                var dto = new CharacterDTO();
                Mapper.Mappers.CharacterMapper.ToCharacterDTO(entity, dto);
                result.Add(dto);
            }
            return result;
        }

        /// <summary>
        /// Returns first 30 occurences of highest Reputation
        /// </summary>
        /// <returns></returns>
        public List<CharacterDTO> GetTopReputation()
        {
            using var context = DataAccessHelper.CreateContext();
            var result = new List<CharacterDTO>();
            foreach (var entity in context.Character.Where(c => c.State == (byte)CharacterState.Active && c.Account.Authority >= AuthorityType.User && c.Account.Authority <= AuthorityType.GS && !c.Account.PenaltyLog.Any(l => l.Penalty == PenaltyType.Banned && l.DateEnd > DateTime.Now)).OrderByDescending(c => c.Reputation).Take(43))
            {
                var dto = new CharacterDTO();
                Mapper.Mappers.CharacterMapper.ToCharacterDTO(entity, dto);
                result.Add(dto);
            }
            return result;
        }

        public SaveResult InsertOrUpdate(ref CharacterDTO character, bool allowStateUpdate = false)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                var characterId = character.CharacterId;
                var entity = context.Character.FirstOrDefault(c => c.CharacterId.Equals(characterId));
                if (entity == null)
                {
                    character = insert(character, context);
                    return SaveResult.Inserted;
                }
                character = update(entity, character, context, allowStateUpdate);
                return SaveResult.Updated;
            }
            catch (Exception e)
            {
                Logger.Log.Error(string.Format(Language.Instance.GetMessageFromKey("INSERT_ERROR"), character, e.Message), e);
                return SaveResult.Error;
            }
        }

        public IEnumerable<CharacterDTO> LoadAll()
        {
            using var context = DataAccessHelper.CreateContext();
            var result = new List<CharacterDTO>();
            foreach (var chara in context.Character)
            {
                var dto = new CharacterDTO();
                Mapper.Mappers.CharacterMapper.ToCharacterDTO(chara, dto);
                result.Add(dto);
            }
            return result;
        }

        public IEnumerable<CharacterDTO> LoadAllByAccount(long accountId)
        {
            using var context = DataAccessHelper.CreateContext();
            var result = new List<CharacterDTO>();
            foreach (var entity in context.Character.Where(c => c.AccountId.Equals(accountId)).OrderByDescending(c => c.Slot))
            {
                var dto = new CharacterDTO();
                Mapper.Mappers.CharacterMapper.ToCharacterDTO(entity, dto);
                result.Add(dto);
            }
            return result;
        }

        public IEnumerable<CharacterDTO> LoadByAccount(long accountId)
        {
            using var context = DataAccessHelper.CreateContext();
            var result = new List<CharacterDTO>();
            foreach (var entity in context.Character.Where(c => c.AccountId.Equals(accountId) && c.State.Equals((byte)CharacterState.Active)).OrderByDescending(c => c.Slot))
            {
                var dto = new CharacterDTO();
                Mapper.Mappers.CharacterMapper.ToCharacterDTO(entity, dto);
                result.Add(dto);
            }
            return result;
        }

        public CharacterDTO LoadById(long characterId)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                var dto = new CharacterDTO();
                if (Mapper.Mappers.CharacterMapper.ToCharacterDTO(context.Character.FirstOrDefault(c => c.CharacterId.Equals(characterId)), dto))
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

        public CharacterDTO LoadByName(string name)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                var dto = new CharacterDTO();
                if (Mapper.Mappers.CharacterMapper.ToCharacterDTO(context.Character.SingleOrDefault(c => c.Name.Equals(name)), dto))
                {
                    return dto;
                }

                return null;
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
            }
            return null;
        }

        public CharacterDTO LoadBySlot(long accountId, byte slot)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                var dto = new CharacterDTO();
                if (Mapper.Mappers.CharacterMapper.ToCharacterDTO(context.Character.SingleOrDefault(c => c.AccountId.Equals(accountId) && c.Slot.Equals(slot) && c.State.Equals((byte)CharacterState.Active)), dto))
                {
                    return dto;
                }

                return null;
            }
            catch (Exception e)
            {
                Logger.Log.Error($"There should be only 1 character per slot, AccountId: {accountId} Slot: {slot}", e);
                return null;
            }
        }

        private static CharacterDTO insert(CharacterDTO character, OpenNosContext context)
        {
            var entity = new Character();
            Mapper.Mappers.CharacterMapper.ToCharacter(character, entity);
            context.Character.Add(entity);
            context.SaveChanges();
            if (Mapper.Mappers.CharacterMapper.ToCharacterDTO(entity, character))
            {
                return character;
            }
            return null;
        }

        private static CharacterDTO update(Character entity, CharacterDTO character, OpenNosContext context, bool allowStateUpdate = false)
        {
            if (entity != null)
            {
                // State Updates should only occur upon deleting character, so outside of this method.
                var state = entity.State;
                Mapper.Mappers.CharacterMapper.ToCharacter(character, entity);

                if (!allowStateUpdate)
                {
                    entity.State = state;
                }

                context.SaveChanges();
            }

            if (Mapper.Mappers.CharacterMapper.ToCharacterDTO(entity, character))
            {
                return character;
            }

            return null;
        }

        #endregion
    }
}