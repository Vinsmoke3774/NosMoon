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
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenNos.DAL.DAO
{
    public class CardDAO : ICardDAO
    {
        #region Methods

        public CardDTO Insert(ref CardDTO card)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                var entity = new Card();
                Mapper.Mappers.CardMapper.ToCard(card, entity);
                context.Card.Add(entity);
                context.SaveChanges();
                if (Mapper.Mappers.CardMapper.ToCardDTO(entity, card))
                {
                    return card;
                }

                return null;
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
                return null;
            }
        }

        public void Insert(List<CardDTO> cards)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                context.Configuration.AutoDetectChangesEnabled = false;
                foreach (var card in cards)
                {
                    InsertOrUpdate(card);
                }
                context.Configuration.AutoDetectChangesEnabled = true;
                context.SaveChanges();
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
            }
        }

        public SaveResult InsertOrUpdate(CardDTO card)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                long CardId = card.CardId;
                var entity = context.Card.FirstOrDefault(c => c.CardId == CardId);

                if (entity == null)
                {
                    card = insert(card, context);
                    return SaveResult.Inserted;
                }

                card = update(entity, card, context);
                return SaveResult.Updated;
            }
            catch (Exception e)
            {
                Logger.Log.Error(string.Format(Language.Instance.GetMessageFromKey("UPDATE_CARD_ERROR"), card.CardId, e.Message), e);
                return SaveResult.Error;
            }
        }

        public IEnumerable<CardDTO> LoadAll()
        {
            using var context = DataAccessHelper.CreateContext();
            var result = new List<CardDTO>();
            foreach (var card in context.Card)
            {
                var dto = new CardDTO();
                Mapper.Mappers.CardMapper.ToCardDTO(card, dto);
                result.Add(dto);
            }
            return result;
        }

        public CardDTO LoadById(short cardId)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                var dto = new CardDTO();
                if (Mapper.Mappers.CardMapper.ToCardDTO(context.Card.FirstOrDefault(s => s.CardId.Equals(cardId)), dto))
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

        private static CardDTO insert(CardDTO card, OpenNosContext context)
        {
            var entity = new Card();
            Mapper.Mappers.CardMapper.ToCard(card, entity);
            context.Card.Add(entity);
            context.SaveChanges();
            if (Mapper.Mappers.CardMapper.ToCardDTO(entity, card))
            {
                return card;
            }

            return null;
        }

        private static CardDTO update(Card entity, CardDTO card, OpenNosContext context)
        {
            if (entity != null)
            {
                Mapper.Mappers.CardMapper.ToCard(card, entity);
                context.SaveChanges();
            }

            if (Mapper.Mappers.CardMapper.ToCardDTO(entity, card))
            {
                return card;
            }

            return null;
        }

        #endregion
    }
}