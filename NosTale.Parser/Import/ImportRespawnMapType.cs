﻿using OpenNos.Core.Logger;
using OpenNos.DAL;
using OpenNos.Data;
using OpenNos.Domain;
using System.Collections.Generic;
using System.Linq;

namespace NosTale.Parser.Import
{
    public class ImportRespawnMapType : IImport
    {
        #region Methods

        public void Import()
        {
            var existingRespawnMapTypes = DAOFactory.RespawnMapTypeDAO.LoadAll().Select(x => x.Name).ToHashSet();

            var respawnmaptypemaps = new List<RespawnMapTypeDTO>
            {
                new RespawnMapTypeDTO
                {
                    RespawnMapTypeId = (long) RespawnType.DefaultAct1,
                    DefaultMapId = 1,
                    DefaultX = 80,
                    DefaultY = 116,
                    Name = "Default"
                },
                new RespawnMapTypeDTO
                {
                    RespawnMapTypeId = (long) RespawnType.ReturnAct1,
                    DefaultMapId = 0,
                    DefaultX = 0,
                    DefaultY = 0,
                    Name = "Return"
                },
                new RespawnMapTypeDTO
                {
                    RespawnMapTypeId = (long) RespawnType.DefaultAct3,
                    DefaultMapId = 0,
                    DefaultX = 0,
                    DefaultY = 0,
                    Name = "DefaultAct3"
                },
                new RespawnMapTypeDTO
                {
                    RespawnMapTypeId = (long) RespawnType.DefaultAct5,
                    DefaultMapId = 170,
                    DefaultX = 86,
                    DefaultY = 48,
                    Name = "DefaultAct5"
                },
                new RespawnMapTypeDTO
                {
                    RespawnMapTypeId = (long) RespawnType.ReturnAct5,
                    DefaultMapId = 0,
                    DefaultX = 0,
                    DefaultY = 0,
                    Name = "ReturnAct5"
                },
                new RespawnMapTypeDTO
                {
                    RespawnMapTypeId = (long) RespawnType.DefaultAct6,
                    DefaultMapId = 228,
                    DefaultX = 72,
                    DefaultY = 102,
                    Name = "DefaultAct6"
                },
                new RespawnMapTypeDTO
                {
                    RespawnMapTypeId = (long) RespawnType.DefaultAct61A,
                    DefaultMapId = 228,
                    DefaultX = 72,
                    DefaultY = 102,
                    Name = "DefaultAct6.1A"
                },
                new RespawnMapTypeDTO
                {
                    RespawnMapTypeId = (long) RespawnType.DefaultAct61D,
                    DefaultMapId = 228,
                    DefaultX = 72,
                    DefaultY = 102,
                    Name = "DefaultAct6.1D"
                },
                new RespawnMapTypeDTO
                {
                    RespawnMapTypeId = (long) RespawnType.DefaultAct62,
                    DefaultMapId = 228,
                    DefaultX = 72,
                    DefaultY = 102,
                    Name = "DefaultAct62"
                },
                new RespawnMapTypeDTO
                {
                    RespawnMapTypeId = (long) RespawnType.DefaultOasis,
                    DefaultMapId = 261,
                    DefaultX = 66,
                    DefaultY = 70,
                    Name = "DefaultOasis"
                },
                new RespawnMapTypeDTO
                {
                    RespawnMapTypeId = (long) RespawnType.DefaultAct7,
                    DefaultMapId = 2628,
                    DefaultX = 76,
                    DefaultY = 66,
                    Name = "DefaultAct7"
                }
            };

            var toImport = respawnmaptypemaps.Where(x => !existingRespawnMapTypes.Contains(x.Name)).ToList();
            DAOFactory.RespawnMapTypeDAO.Insert(toImport);

            Logger.Log.Info($"{toImport.Count} RespawnMapType parsed");
        }

        #endregion
    }
}