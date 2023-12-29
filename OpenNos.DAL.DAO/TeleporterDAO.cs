﻿using OpenNos.Core.Logger;
using OpenNos.DAL.EF;
using OpenNos.DAL.EF.Helpers;
using OpenNos.DAL.Interface;
using OpenNos.Data;
using OpenNos.Mapper.Mappers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenNos.DAL.DAO
{
    public class TeleporterDAO : ITeleporterDAO
    {
        #region Methods

        public void Insert(IEnumerable<TeleporterDTO> teleporters)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    context.Configuration.AutoDetectChangesEnabled = false;
                    foreach (var teleporter in teleporters)
                    {
                        var entity = new Teleporter();
                        TeleporterMapper.ToTeleporter(teleporter, entity);
                        context.Teleporter.Add(entity);
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

        public TeleporterDTO Insert(TeleporterDTO teleporter)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    var entity = new Teleporter();
                    TeleporterMapper.ToTeleporter(teleporter, entity);
                    context.Teleporter.Add(entity);
                    context.SaveChanges();
                    if (TeleporterMapper.ToTeleporterDTO(entity, teleporter)) return teleporter;

                    return null;
                }
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
                return null;
            }
        }

        public IEnumerable<TeleporterDTO> LoadAll()
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                var result = new List<TeleporterDTO>();
                foreach (var entity in context.Teleporter)
                {
                    var dto = new TeleporterDTO();
                    TeleporterMapper.ToTeleporterDTO(entity, dto);
                    result.Add(dto);
                }

                return result;
            }
        }

        public TeleporterDTO LoadById(short teleporterId)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    var dto = new TeleporterDTO();
                    if (TeleporterMapper.ToTeleporterDTO(
                        context.Teleporter.FirstOrDefault(i => i.TeleporterId.Equals(teleporterId)), dto)) return dto;

                    return null;
                }
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
                return null;
            }
        }

        public IEnumerable<TeleporterDTO> LoadFromNpc(int npcId)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                var result = new List<TeleporterDTO>();
                foreach (var entity in context.Teleporter.Where(c => c.MapNpcId.Equals(npcId)))
                {
                    var dto = new TeleporterDTO();
                    TeleporterMapper.ToTeleporterDTO(entity, dto);
                    result.Add(dto);
                }

                return result;
            }
        }

        #endregion
    }
}