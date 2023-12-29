using OpenNos.Core.Logger;
using OpenNos.DAL;
using OpenNos.Data;
using OpenNos.Domain;
using System.Collections.Generic;
using System.Linq;

namespace NosTale.Parser.Import
{
    public class ImportMapType : IImport
    {
        #region Methods

        public void Import()
        {
            var existingMapTypes = DAOFactory.MapTypeDAO.LoadAll().ToList();

            var mapTypes = new List<MapTypeDTO>
            {
                new MapTypeDTO
                {
                    MapTypeId = (short) MapTypeEnum.Act1,
                    MapTypeName = "Act1",
                    PotionDelay = 300,
                    RespawnMapTypeId = (long) RespawnType.DefaultAct1,
                    ReturnMapTypeId = (long) RespawnType.ReturnAct1
                },
                new MapTypeDTO
                {
                    MapTypeId = (short) MapTypeEnum.Act2,
                    MapTypeName = "Act2",
                    PotionDelay = 300,
                    RespawnMapTypeId = (long) RespawnType.DefaultAct1,
                    ReturnMapTypeId = (long) RespawnType.ReturnAct1
                },
                new MapTypeDTO
                {
                    MapTypeId = (short) MapTypeEnum.Act3,
                    MapTypeName = "Act3",
                    PotionDelay = 300,
                    RespawnMapTypeId = (long) RespawnType.DefaultAct1,
                    ReturnMapTypeId = (long) RespawnType.ReturnAct1
                },
                new MapTypeDTO
                {
                    MapTypeId = (short) MapTypeEnum.Act4,
                    MapTypeName = "Act4",
                    PotionDelay = 5000
                },
                new MapTypeDTO
                {
                    MapTypeId = (short) MapTypeEnum.Act42,
                    MapTypeName = "Act4",
                    PotionDelay = 5000
                },
                new MapTypeDTO
                {
                    MapTypeId = (short) MapTypeEnum.Act51,
                    MapTypeName = "Act5.1",
                    PotionDelay = 300,
                    RespawnMapTypeId = (long) RespawnType.DefaultAct5,
                    ReturnMapTypeId = (long) RespawnType.ReturnAct5
                },
                new MapTypeDTO
                {
                    MapTypeId = (short) MapTypeEnum.Act52,
                    MapTypeName = "Act5.2",
                    PotionDelay = 300,
                    RespawnMapTypeId = (long) RespawnType.DefaultAct5,
                    ReturnMapTypeId = (long) RespawnType.ReturnAct5
                },
                new MapTypeDTO
                {
                    MapTypeId = (short) MapTypeEnum.Act61,
                    MapTypeName = "Act6.1",
                    PotionDelay = 300,
                    RespawnMapTypeId = (long) RespawnType.DefaultAct6,
                    ReturnMapTypeId = (long) RespawnType.ReturnAct1
                },
                new MapTypeDTO
                {
                    MapTypeId = (short) MapTypeEnum.Act61A,
                    MapTypeName = "Act6.1A", // ANGEL CAMP
                    PotionDelay = 300,
                    RespawnMapTypeId = (long) RespawnType.DefaultAct61A,
                    ReturnMapTypeId = (long) RespawnType.ReturnAct1
                },
                new MapTypeDTO
                {
                    MapTypeId = (short) MapTypeEnum.Act61D,
                    MapTypeName = "Act6.1D", // DEMON CAMP
                    PotionDelay = 300,
                    RespawnMapTypeId = (long) RespawnType.DefaultAct61D,
                    ReturnMapTypeId = (long) RespawnType.ReturnAct1
                },
                new MapTypeDTO
                {
                    MapTypeId = (short) MapTypeEnum.Act62,
                    MapTypeName = "Act6.2",
                    PotionDelay = 300,
                    RespawnMapTypeId = (long) RespawnType.DefaultAct1,
                    ReturnMapTypeId = (long) RespawnType.ReturnAct1
                },
                new MapTypeDTO
                {
                    MapTypeId = (short) MapTypeEnum.CometPlain,
                    MapTypeName = "CometPlain",
                    PotionDelay = 300,
                    RespawnMapTypeId = (long) RespawnType.DefaultAct1,
                    ReturnMapTypeId = (long) RespawnType.ReturnAct1
                },
                new MapTypeDTO
                {
                    MapTypeId = (short) MapTypeEnum.Mine1,
                    MapTypeName = "Mine1",
                    PotionDelay = 300,
                    RespawnMapTypeId = (long) RespawnType.DefaultAct1,
                    ReturnMapTypeId = (long) RespawnType.ReturnAct1
                },
                new MapTypeDTO
                {
                    MapTypeId = (short) MapTypeEnum.Mine2,
                    MapTypeName = "Mine2",
                    PotionDelay = 300,
                    RespawnMapTypeId = (long) RespawnType.DefaultAct1,
                    ReturnMapTypeId = (long) RespawnType.ReturnAct1
                },
                new MapTypeDTO
                {
                    MapTypeId = (short) MapTypeEnum.MeadowOfMine,
                    MapTypeName = "MeadownOfPlain",
                    PotionDelay = 300,
                    RespawnMapTypeId = (long) RespawnType.DefaultAct1,
                    ReturnMapTypeId = (long) RespawnType.ReturnAct1
                },
                new MapTypeDTO
                {
                    MapTypeId = (short) MapTypeEnum.SunnyPlain,
                    MapTypeName = "SunnyPlain",
                    PotionDelay = 300,
                    RespawnMapTypeId = (long) RespawnType.DefaultAct1,
                    ReturnMapTypeId = (long) RespawnType.ReturnAct1
                },
                new MapTypeDTO
                {
                    MapTypeId = (short) MapTypeEnum.Fernon,
                    MapTypeName = "Fernon",
                    PotionDelay = 300,
                    RespawnMapTypeId = (long) RespawnType.DefaultAct1,
                    ReturnMapTypeId = (long) RespawnType.ReturnAct1
                },
                new MapTypeDTO
                {
                    MapTypeId = (short) MapTypeEnum.FernonF,
                    MapTypeName = "FernonF",
                    PotionDelay = 300,
                    RespawnMapTypeId = (long) RespawnType.DefaultAct1,
                    ReturnMapTypeId = (long) RespawnType.ReturnAct1
                },
                new MapTypeDTO
                {
                    MapTypeId = (short) MapTypeEnum.Cliff,
                    MapTypeName = "Cliff",
                    PotionDelay = 300,
                    RespawnMapTypeId = (long) RespawnType.DefaultAct1,
                    ReturnMapTypeId = (long) RespawnType.ReturnAct1
                },
                new MapTypeDTO
                {
                    MapTypeId = (short) MapTypeEnum.LandOfTheDead,
                    MapTypeName = "LandOfTheDead",
                    PotionDelay = 300
                },
                new MapTypeDTO
                {
                    MapTypeId = (short) MapTypeEnum.Act32,
                    MapTypeName = "Act 3.2",
                    PotionDelay = 300
                },
                new MapTypeDTO
                {
                    MapTypeId = (short) MapTypeEnum.CleftOfDarkness,
                    MapTypeName = "Cleft of Darkness",
                    PotionDelay = 300
                },
                new MapTypeDTO
                {
                    MapTypeId = (short) MapTypeEnum.PVPMap,
                    MapTypeName = "PVPMap",
                    PotionDelay = 300
                },
                new MapTypeDTO
                {
                    MapTypeId = (short) MapTypeEnum.Citadel,
                    MapTypeName = "Citadel",
                    PotionDelay = 300
                },
                new MapTypeDTO
                {
                    MapTypeId = (short) MapTypeEnum.Act7,
                    MapTypeName = "Act7",
                    PotionDelay = 300,
                    RespawnMapTypeId = (long) RespawnType.DefaultAct7,
                    ReturnMapTypeId = (long) RespawnType.ReturnAct1
                },
                new MapTypeDTO
                {
                    MapTypeId = (short) MapTypeEnum.Oasis,
                    MapTypeName = "Oasis",
                    PotionDelay = 300,
                    RespawnMapTypeId = (long) RespawnType.DefaultOasis,
                    ReturnMapTypeId = (long) RespawnType.ReturnAct1
                }
            };

            mapTypes.RemoveAll(x => existingMapTypes.Any(s => s.MapTypeName == x.MapTypeName));
            DAOFactory.MapTypeDAO.Insert(mapTypes);
            Logger.Log.Info($"{mapTypes.Count} MapTypes parsed");
        }

        #endregion
    }
}