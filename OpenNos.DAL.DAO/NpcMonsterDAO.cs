﻿using OpenNos.Core;
using OpenNos.Core.Logger;
using OpenNos.DAL.EF;
using OpenNos.DAL.EF.Helpers;
using OpenNos.DAL.Interface;
using OpenNos.Data;
using OpenNos.Data.Enums;
using OpenNos.Mapper.Mappers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenNos.DAL.DAO
{
    public class NpcMonsterDAO : INpcMonsterDAO
    {
        #region Methods

        public IEnumerable<NpcMonsterDTO> FindByName(string name)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                var result = new List<NpcMonsterDTO>();
                foreach (var npcMonster in context.NpcMonster.Where(s =>
                    string.IsNullOrEmpty(name) ? s.Name.Equals("") : s.Name.Contains(name)))
                {
                    var dto = new NpcMonsterDTO();
                    NpcMonsterMapper.ToNpcMonsterDTO(npcMonster, dto);
                    result.Add(dto);
                }

                return result;
            }
        }

        public void Insert(List<NpcMonsterDTO> npcMonsters)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    context.Configuration.AutoDetectChangesEnabled = false;
                    foreach (var Item in npcMonsters)
                    {
                        var entity = new NpcMonster();
                        NpcMonsterMapper.ToNpcMonster(Item, entity);
                        context.NpcMonster.Add(entity);
                    }

                    context.Configuration.AutoDetectChangesEnabled = true;
                    context.SaveChanges();
                }
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
            }
        }

        public NpcMonsterDTO Insert(NpcMonsterDTO npc)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    var entity = new NpcMonster();
                    NpcMonsterMapper.ToNpcMonster(npc, entity);
                    context.NpcMonster.Add(entity);
                    context.SaveChanges();
                    if (NpcMonsterMapper.ToNpcMonsterDTO(entity, npc)) return npc;

                    return null;
                }
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
                return null;
            }
        }

        public SaveResult InsertOrUpdate(ref NpcMonsterDTO npcMonster)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    var npcMonsterVNum = npcMonster.NpcMonsterVNum;
                    var entity = context.NpcMonster.FirstOrDefault(c => c.NpcMonsterVNum.Equals(npcMonsterVNum));

                    if (entity == null)
                    {
                        npcMonster = insert(npcMonster, context);
                        return SaveResult.Inserted;
                    }

                    npcMonster = update(entity, npcMonster, context);
                    return SaveResult.Updated;
                }
            }
            catch (Exception e)
            {
                Logger.Log.Error(
                    string.Format(Language.Instance.GetMessageFromKey("UPDATE_NPCMONSTER_ERROR"),
                        npcMonster.NpcMonsterVNum, e.Message), e);
                return SaveResult.Error;
            }
        }

        public IEnumerable<NpcMonsterDTO> LoadAll()
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                var result = new List<NpcMonsterDTO>();
                foreach (var NpcMonster in context.NpcMonster)
                {
                    var dto = new NpcMonsterDTO();
                    NpcMonsterMapper.ToNpcMonsterDTO(NpcMonster, dto);
                    result.Add(dto);
                }

                return result;
            }
        }

        public NpcMonsterDTO LoadByVNum(short npcMonsterVNum)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    var dto = new NpcMonsterDTO();
                    if (NpcMonsterMapper.ToNpcMonsterDTO(
                        context.NpcMonster.FirstOrDefault(i => i.NpcMonsterVNum.Equals(npcMonsterVNum)), dto))
                        return dto;

                    return null;
                }
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
                return null;
            }
        }

        private static NpcMonsterDTO insert(NpcMonsterDTO npcMonster, OpenNosContext context)
        {
            var entity = new NpcMonster();
            NpcMonsterMapper.ToNpcMonster(npcMonster, entity);
            context.NpcMonster.Add(entity);
            context.SaveChanges();
            if (NpcMonsterMapper.ToNpcMonsterDTO(entity, npcMonster)) return npcMonster;

            return null;
        }

        private static NpcMonsterDTO update(NpcMonster entity, NpcMonsterDTO npcMonster, OpenNosContext context)
        {
            if (entity != null)
            {
                NpcMonsterMapper.ToNpcMonster(npcMonster, entity);
                context.SaveChanges();
            }

            if (NpcMonsterMapper.ToNpcMonsterDTO(entity, npcMonster)) return npcMonster;

            return null;
        }

        #endregion
    }
}