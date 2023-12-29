﻿using OpenNos.Core.Logger;
using OpenNos.DAL;
using OpenNos.Data;
using OpenNos.Domain;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace NosTale.Parser.Import
{
    public class ImportItems : IImport
    {
        private const string FileName = "Item.dat";

        private readonly ImportConfiguration _configuration;

        public ImportItems(ImportConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void Import()
        {
            Generate();
        }

        public List<ItemDTO> Generate()
        {
            var filePath = Path.Combine(_configuration.DatFolder, FileName);

            var fileLangPath = Path.Combine(_configuration.LangFolder, $"_code_{_configuration.Lang}_Item.txt");

            var existingItems = DAOFactory.ItemDAO.LoadAll().Select(x => x.VNum).ToHashSet();

            var items = new List<ItemDTO>();
            var itemCards = new List<BCardDTO>();
            var dictionaryName = new Dictionary<string, string>();

            using (var mapIdLangStream = new StreamReader(fileLangPath, Encoding.GetEncoding(1252)))
            {
                string line;
                while ((line = mapIdLangStream.ReadLine()) != null)
                {
                    var linesave = line.Split('\t');
                    if (linesave.Length <= 1 || dictionaryName.ContainsKey(linesave[0])) continue;
                    dictionaryName.Add(linesave[0], linesave[1]);
                }
            }

            using (var npcIdStream = new StreamReader(filePath, Encoding.GetEncoding(1252)))
            {
                var item = new ItemDTO();
                var itemAreaBegin = false;
                var continueToRead = false;
                var itemCounter = 0;

                string line;
                while ((line = npcIdStream.ReadLine()) != null)
                {
                    var currentLine = line.Split('\t');

                    if (currentLine.Length > 3 && currentLine[1] == "VNUM")
                    {
                        itemAreaBegin = true;
                        item.VNum = Convert.ToInt16(currentLine[2]);
                        item.Price = long.Parse(currentLine[3]);
                        item.SellToNpcPrice = item.Price / 20;
                    }
                    else if (currentLine.Length > 1 && currentLine[1] == "END")
                    {
                        if (!itemAreaBegin) continue;

                        if (!existingItems.Contains(item.VNum))
                        {
                            items.Add(item);
                            continueToRead = true;
                            itemCounter++;
                        }

                        item = new ItemDTO();
                        itemAreaBegin = false;
                    }
                    else if (currentLine.Length > 2 && currentLine[1] == "NAME")
                    {
                        if (!continueToRead) continue;

                        item.Name = dictionaryName.TryGetValue(currentLine[2], out var name) ? name : "";
                    }
                    else if (currentLine.Length > 7 && currentLine[1] == "INDEX")
                    {
                        if (!continueToRead) continue;

                        FillMorphAndIndexValues(currentLine, item);
                    }
                    else if (currentLine.Length > 3 && currentLine[1] == "TYPE")
                    {
                        if (!continueToRead) continue;

                        // currentLine[2] 0-range 2-range 3-magic
                        item.Class = item.EquipmentSlot == EquipmentType.Fairy
                            ? (byte) 15
                            : Convert.ToByte(currentLine[3]);
                    }
                    else if (currentLine.Length > 3 && currentLine[1] == "FLAG")
                    {
                        if (!continueToRead) continue;

                        FillFlags(item, currentLine);
                    }
                    else if (currentLine.Length > 1 && currentLine[1] == "DATA")
                    {
                        if (!continueToRead) continue;

                        FillData(item, currentLine);
                    }
                    else if (currentLine.Length > 1 && currentLine[1] == "BUFF")
                    {
                        // Always Re-Import Bcard 
                        FillBuff(currentLine, item, itemCards);
                    }
                }

                items = items.Where(s => DAOFactory.ItemDAO.LoadById(s.VNum) == null).ToList();
                DAOFactory.ItemDAO.Insert(items);
                DAOFactory.BCardDAO.Insert(itemCards);

                Logger.Log.Info($"{itemCounter} Items parsed");
                Logger.Log.Info($"{itemCards.Count} Items BCARD parsed");

                return items;
            }
        }

        private static void FillBuff(IReadOnlyList<string> currentLine, ItemDTO item, List<BCardDTO> itemCards)
        {
            DAOFactory.BCardDAO.DeleteByItemVNum(item.VNum);
            for (var i = 0; i < 5; i++)
            {
                var type = (byte) Convert.ToInt32(currentLine[2 + 5 * i]);
                if (type == 0 || type == 255) continue;

                var first = Convert.ToInt32(currentLine[3 + 5 * i]);
                var itemCard = new BCardDTO
                {
                    ItemVNum = item.VNum,
                    Type = type,
                    SubType = (byte) ((Convert.ToByte(currentLine[5 + 5 * i]) + 1) * 10 + 1 + (first < 0 ? 1 : 0)),
                    IsLevelScaled = Convert.ToBoolean(first % 4),
                    IsLevelDivided = first % 4 == 2,
                    FirstData = (short) (first / 4),
                    SecondData = (short) (Convert.ToInt32(currentLine[4 + 5 * i]) / 4),
                    ThirdData = (short) (Convert.ToInt32(currentLine[6 + 5 * i]) / 4),
                    CastType = Convert.ToByte(currentLine[6 + 5 * i])
                };
                itemCards.Add(itemCard);
            }
        }

        private static void FillFlags(ItemDTO item, string[] currentLine)
        {
            item.IsSoldable = currentLine[5] == "0";
            item.IsDroppable = currentLine[6] == "0";
            item.IsTradable = currentLine[7] == "0";
            item.IsBlocked = currentLine[8] == "1";
            item.IsMinilandObject = currentLine[9] == "1";
            item.IsHolder = currentLine[10] == "1";
            item.IsColored = currentLine[16] == "1";
            item.Sex = currentLine[18] == "1" ? (byte) 1 : currentLine[17] == "1" ? (byte) 2 : (byte) 0;
            if (currentLine[21] == "1") item.ReputPrice = item.Price;
            item.IsHeroic = currentLine[22] == "1";
            /*
            item.IsVehicle = currentLine[11] == "1" ? true : false; // (?)
            item.BoxedVehicle = currentLine[12] == "1" ? true : false; // (?)
            linesave[4]  unknown
            linesave[11] unknown
            linesave[12] unknown
            linesave[13] unknown
            linesave[14] unknown
            linesave[15] unknown
            linesave[19] unknown
            linesave[20] unknown
            */
        }

        private static void FillData(ItemDTO item, string[] currentLine)
        {
            switch (item.ItemType)
            {
                case ItemType.Weapon:
                    item.LevelMinimum = Convert.ToByte(currentLine[2]);
                    item.DamageMinimum = Convert.ToInt16(currentLine[3]);
                    item.DamageMaximum = Convert.ToInt16(currentLine[4]);
                    item.HitRate = Convert.ToInt16(currentLine[5]);
                    item.CriticalLuckRate = Convert.ToByte(currentLine[6]);
                    item.CriticalRate = Convert.ToInt16(currentLine[7]);
                    item.BasicUpgrade = Convert.ToByte(currentLine[10]);
                    item.MaximumAmmo = 100;
                    break;

                case ItemType.Armor:
                    item.LevelMinimum = Convert.ToByte(currentLine[2]);
                    item.CloseDefence = Convert.ToInt16(currentLine[3]);
                    item.DistanceDefence = Convert.ToInt16(currentLine[4]);
                    item.MagicDefence = Convert.ToInt16(currentLine[5]);
                    item.DefenceDodge = Convert.ToInt16(currentLine[6]);
                    item.DistanceDefenceDodge = Convert.ToInt16(currentLine[6]);
                    item.BasicUpgrade = Convert.ToByte(currentLine[10]);
                    break;

                case ItemType.Box:
                    switch (item.VNum)
                    {
                        // add here your custom effect/effectvalue for box item, make
                        // sure its unique for boxitems

                        case 287:
                            item.Effect = 69;
                            item.EffectValue = 1;
                            break;

                        case 4240:
                            item.Effect = 69;
                            item.EffectValue = 2;
                            break;

                        case 4194:
                            item.Effect = 69;
                            item.EffectValue = 3;
                            break;

                        case 4106:
                            item.Effect = 69;
                            item.EffectValue = 4;
                            break;

                        default:
                            item.Effect = Convert.ToInt16(currentLine[2]);
                            item.EffectValue = Convert.ToInt32(currentLine[3]);
                            item.LevelMinimum = Convert.ToByte(currentLine[4]);
                            break;
                    }

                    break;

                case ItemType.Fashion:
                    item.LevelMinimum = Convert.ToByte(currentLine[2]);
                    item.CloseDefence = Convert.ToInt16(currentLine[3]);
                    item.DistanceDefence = Convert.ToInt16(currentLine[4]);
                    item.MagicDefence = Convert.ToInt16(currentLine[5]);
                    item.DefenceDodge = Convert.ToInt16(currentLine[6]);
                    if (item.EquipmentSlot.Equals(EquipmentType.CostumeHat) ||
                        item.EquipmentSlot.Equals(EquipmentType.CostumeSuit))
                        item.ItemValidTime = Convert.ToInt32(currentLine[13]) * 3600;
                    break;

                case ItemType.Food:
                    item.Hp = Convert.ToInt16(currentLine[2]);
                    item.Mp = Convert.ToInt16(currentLine[4]);
                    break;

                case ItemType.Jewelery:
                    if (item.EquipmentSlot.Equals(EquipmentType.Amulet))
                    {
                        item.LevelMinimum = Convert.ToByte(currentLine[2]);
                        if (item.VNum > 4055 && item.VNum < 4061 || item.VNum > 4172 && item.VNum < 4176 ||
                            item.VNum > 4045 && item.VNum < 4056
                            || item.VNum > 8104 && item.VNum < 8115 || item.VNum == 967 || item.VNum == 968)
                            item.ItemValidTime = 10800;
                        else
                            item.ItemValidTime = Convert.ToInt32(currentLine[3]);
                    }
                    else if (item.EquipmentSlot.Equals(EquipmentType.Fairy))
                    {
                        item.Element = Convert.ToByte(currentLine[2]);
                        item.ElementRate = Convert.ToInt16(currentLine[3]);
                        if (item.VNum <= 256)
                            item.MaxElementRate = 50;
                        else
                            switch (item.ElementRate)
                            {
                                case 0:
                                    if (item.VNum >= 800 && item.VNum <= 804)
                                        item.MaxElementRate = 50;
                                    else
                                        item.MaxElementRate = 70;
                                    break;

                                case 30:
                                    if (item.VNum >= 884 && item.VNum <= 887)
                                        item.MaxElementRate = 50;
                                    else
                                        item.MaxElementRate = 30;
                                    break;

                                case 35:
                                    item.MaxElementRate = 35;
                                    break;

                                case 40:
                                    item.MaxElementRate = 70;
                                    break;

                                case 50:
                                    item.MaxElementRate = 80;
                                    break;
                            }
                    }
                    else
                    {
                        item.LevelMinimum = Convert.ToByte(currentLine[2]);
                        item.MaxCellonLvl = Convert.ToByte(currentLine[3]);
                        item.MaxCellon = byte.TryParse(currentLine[4], out byte result) ? result : (byte)0;  // Default to 0 if parse fails

                    }

                    break;

                case ItemType.Event:
                    switch (item.VNum)
                    {
                        case 1332:
                            item.EffectValue = 5108;
                            break;

                        case 1333:
                            item.EffectValue = 5109;
                            break;

                        case 1334:
                            item.EffectValue = 5111;
                            break;

                        case 1335:
                            item.EffectValue = 5107;
                            break;

                        case 1336:
                            item.EffectValue = 5106;
                            break;

                        case 1337:
                            item.EffectValue = 5110;
                            break;

                        case 1339:
                            item.EffectValue = 5114;
                            break;

                        case 9031:
                            item.EffectValue = 5108;
                            break;

                        case 9032:
                            item.EffectValue = 5109;
                            break;

                        case 9033:
                            item.EffectValue = 5011;
                            break;

                        case 9034:
                            item.EffectValue = 5107;
                            break;

                        case 9035:
                            item.EffectValue = 5106;
                            break;

                        case 9036:
                            item.EffectValue = 5110;
                            break;

                        case 9038:
                            item.EffectValue = 5114;
                            break;

                        // EffectItems aka. fireworks
                        case 1581:
                            item.EffectValue = 860;
                            break;

                        case 1582:
                            item.EffectValue = 861;
                            break;

                        case 1585:
                            item.EffectValue = 859;
                            break;

                        case 1983:
                            item.EffectValue = 875;
                            break;

                        case 1984:
                            item.EffectValue = 876;
                            break;

                        case 1985:
                            item.EffectValue = 877;
                            break;

                        case 1986:
                            item.EffectValue = 878;
                            break;

                        case 1987:
                            item.EffectValue = 879;
                            break;

                        case 1988:
                            item.EffectValue = 880;
                            break;

                        case 9044:
                            item.EffectValue = 859;
                            break;

                        case 9059:
                            item.EffectValue = 875;
                            break;

                        case 9060:
                            item.EffectValue = 876;
                            break;

                        case 9061:
                            item.EffectValue = 877;
                            break;

                        case 9062:
                            item.EffectValue = 878;
                            break;

                        case 9063:
                            item.EffectValue = 879;
                            break;

                        case 9064:
                            item.EffectValue = 880;
                            break;

                        default:
                            item.EffectValue = Convert.ToInt16(currentLine[7]);
                            break;
                    }

                    break;

                case ItemType.Special:
                    switch (item.VNum)
                    {
                        case 1115:
                            item.Effect = 201;
                            item.EffectValue = 30;
                            break;

                        case 1116:
                            item.Effect = 202;
                            item.EffectValue = 30;
                            break;

                        case 1246:
                        case 9020:
                            item.Effect = 6600;
                            item.EffectValue = 1;
                            break;

                        case 1247:
                        case 9021:
                            item.Effect = 6600;
                            item.EffectValue = 2;
                            break;

                        case 1248:
                        case 9022:
                            item.Effect = 6600;
                            item.EffectValue = 3;
                            break;

                        case 1249:
                        case 9023:
                            item.Effect = 6600;
                            item.EffectValue = 4;
                            break;

                        case 5130:
                        case 9072:
                            item.Effect = 1006;
                            break;

                        case 1272:
                        case 1858:
                        case 9047:
                            item.Effect = 1009;
                            item.EffectValue = 10;
                            break;

                        case 1273:
                        case 9024:
                            item.Effect = 1009;
                            item.EffectValue = 30;
                            break;

                        case 1274:
                        case 9025:
                            item.Effect = 1009;
                            item.EffectValue = 60;
                            break;

                        case 1279:
                        case 9029:
                            item.Effect = 1007;
                            item.EffectValue = 30;
                            break;

                        case 1280:
                        case 9030:
                            item.Effect = 1007;
                            item.EffectValue = 60;
                            break;

                        case 1923:
                        case 9056:
                            item.Effect = 1007;
                            item.EffectValue = 10;
                            break;

                        case 1275:
                        case 1886:
                        case 9026:
                            item.Effect = 1008;
                            item.EffectValue = 10;
                            break;

                        case 1276:
                        case 9027:
                            item.Effect = 1008;
                            item.EffectValue = 30;
                            break;

                        case 1277:
                        case 9028:
                            item.Effect = 1008;
                            item.EffectValue = 60;
                            break;

                        case 5060:
                        case 9066:
                            item.Effect = 1003;
                            item.EffectValue = 30;
                            break;

                        case 5061:
                        case 9067:
                            item.Effect = 1004;
                            item.EffectValue = 7;
                            break;

                        case 5062:
                        case 9068:
                            item.Effect = 1004;
                            item.EffectValue = 1;
                            break;

                        case 5105:
                            item.Effect = 651;
                            break;

                        case 5115:
                            item.Effect = 652;
                            break;

                        case 1981:
                            item.Effect = 34; // imagined number as for I = √(-1), complex z = a + bi
                            break;

                        case 1982:
                            item.Effect = 6969; // imagined number as for I = √(-1), complex z = a + bi
                            break;

                        case 1904:
                            item.Effect = 1894;
                            break;

                        case 1429:
                            item.Effect = 666;
                            break;

                        case 1430:
                            item.Effect = 666;
                            item.EffectValue = 1;
                            break;

                        case 5107:
                            item.EffectValue = 47;
                            break;

                        case 5207:
                            item.EffectValue = 50;
                            break;

                        case 5519:
                            item.EffectValue = 60;
                            break;

                        default:
                            if (item.VNum > 5891 && item.VNum < 5900 || item.VNum > 9100 && item.VNum < 9109)
                                item.Effect = 69; // imagined number as for I = √(-1), complex z = a + bi
                            else if (item.VNum > 1893 && item.VNum < 1904)
                                item.Effect = 2152;
                            else
                                item.Effect = Convert.ToInt16(currentLine[2]);
                                item.TsMapId = Convert.ToInt16(currentLine[5]);
                            break;
                    }

                    switch (item.Effect)
                    {
                        case 150:
                        case 151:
                            switch (Convert.ToInt32(currentLine[4]))
                            {
                                case 1:
                                    item.EffectValue = 30000;
                                    break;

                                case 2:
                                    item.EffectValue = 70000;
                                    break;

                                case 3:
                                    item.EffectValue = 180000;
                                    break;

                                default:
                                    item.EffectValue = Convert.ToInt32(currentLine[4]);
                                    break;
                            }

                            break;

                        case 204:
                            item.EffectValue = 10000;
                            break;

                        case 305:
                            item.EffectValue = Convert.ToInt32(currentLine[5]);
                            item.Morph = Convert.ToInt16(currentLine[4]);
                            break;

                        default:
                            item.EffectValue = item.EffectValue == 0
                                ? Convert.ToInt32(currentLine[4])
                                : item.EffectValue;
                            break;
                    }

                    item.WaitDelay = 5000;
                    break;

                case ItemType.Magical:
                    if (item.VNum > 2059 && item.VNum < 2070)
                        item.Effect = 10;
                    else
                        item.Effect = Convert.ToInt16(currentLine[2]);
                    item.EffectValue = Convert.ToInt32(currentLine[4]);
                    if (byte.TryParse(currentLine[5], out var sex) && sex > 0) item.Sex = (byte) (sex - 1);
                    break;

                case ItemType.Specialist:

                    // item.isSpecialist = Convert.ToByte(currentLine[2]); item.Unknown = Convert.ToInt16(currentLine[3]);
                    item.ElementRate = Convert.ToInt16(currentLine[4]);
                    item.Speed = Convert.ToByte(currentLine[5]);
                    item.SpType = Convert.ToByte(currentLine[13]);

                    item.MorphSp = (short) (Convert.ToInt16(currentLine[14]) + 1);
                    item.FireResistance = Convert.ToByte(currentLine[15]);
                    item.WaterResistance = Convert.ToByte(currentLine[16]);
                    item.LightResistance = Convert.ToByte(currentLine[17]);
                    item.DarkResistance = Convert.ToByte(currentLine[18]);

                    // item.PartnerClass = Convert.ToInt16(currentLine[19]);
                    item.LevelJobMinimum = Convert.ToByte(currentLine[20]);
                    item.ReputationMinimum = Convert.ToByte(currentLine[21]);

                    var elementdic = new Dictionary<int, int> {[0] = 0};
                    if (item.FireResistance != 0) elementdic.Add(1, item.FireResistance);
                    if (item.WaterResistance != 0) elementdic.Add(2, item.WaterResistance);
                    if (item.LightResistance != 0) elementdic.Add(3, item.LightResistance);
                    if (item.DarkResistance != 0) elementdic.Add(4, item.DarkResistance);

                    item.Element = (byte) elementdic.OrderByDescending(s => s.Value).First().Key;
                    if (elementdic.Count > 1 && elementdic.OrderByDescending(s => s.Value).First().Value ==
                        elementdic.OrderByDescending(s => s.Value).ElementAt(1).Value)
                        item.SecondaryElement = (byte) elementdic.OrderByDescending(s => s.Value).ElementAt(1).Key;

                    // needs to be hardcoded
                    switch (item.VNum)
                    {
                        case 901:
                        case 4815:
                        case 8238:
                        case 4823:
                        case 8245:
                            item.Element = 1;
                            break;

                        case 903:
                            item.Element = 2;
                            break;

                        case 906:
                        case 909:
                            item.Element = 3;
                            break;

                        case 4807:
                        case 4818:
                        case 8230:
                        case 8240:
                        case 8379:
                            item.Element = 4;
                            break;
                    }

                    break;

                case ItemType.Shell:

                    // item.ShellMinimumLevel = Convert.ToInt16(linesave[3]);
                    // item.ShellMaximumLevel = Convert.ToInt16(linesave[4]); item.ShellType
                    // = Convert.ToByte(linesave[5]); // 3 shells of each type
                    break;

                case ItemType.Main:
                    item.Effect = Convert.ToInt16(currentLine[2]);
                    item.EffectValue = Convert.ToInt32(currentLine[4]);
                    break;

                case ItemType.Upgrade:
                    item.Effect = Convert.ToInt16(currentLine[2]);
                    switch (item.VNum)
                    {
                        // UpgradeItems (needed to be hardcoded)
                        case 1218:
                            item.EffectValue = 26;
                            break;

                        case 1363:
                            item.EffectValue = 27;
                            break;

                        case 1364:
                            item.EffectValue = 28;
                            break;

                        case 5107:
                            item.EffectValue = 47;
                            break;

                        case 5207:
                            item.EffectValue = 50;
                            break;

                        case 5369:
                            item.EffectValue = 61;
                            break;

                        case 5519:
                            item.EffectValue = 60;
                            break;

                        default:
                            item.EffectValue = Convert.ToInt32(currentLine[4]);
                            break;
                    }

                    break;

                case ItemType.Production:
                    item.Effect = Convert.ToInt16(currentLine[2]);
                    item.EffectValue = Convert.ToInt32(currentLine[4]);
                    break;

                case ItemType.Map:
                    item.Effect = Convert.ToInt16(currentLine[2]);
                    item.EffectValue = Convert.ToInt32(currentLine[4]);
                    break;

                case ItemType.Potion:
                    item.Hp = Convert.ToInt16(currentLine[2]);
                    item.Mp = Convert.ToInt16(currentLine[4]);
                    break;

                case ItemType.Snack:
                    item.Hp = Convert.ToInt16(currentLine[2]);
                    item.Mp = Convert.ToInt16(currentLine[4]);
                    break;

                case ItemType.Teacher:
                    item.Effect = Convert.ToInt16(currentLine[2]);
                    item.EffectValue = Convert.ToInt32(currentLine[4]);

                    // item.PetLoyality = Convert.ToInt16(linesave[4]); item.PetFood = Convert.ToInt16(linesave[7]);
                    break;

                case ItemType.Part:

                    // nothing to parse
                    break;

                case ItemType.Sell:
                    item.SellToNpcPrice = item.Price;

                    // nothing to parse
                    break;

                case ItemType.Quest2:

                    // nothing to parse
                    break;

                case ItemType.Quest1:

                    // nothing to parse
                    break;

                case ItemType.Ammo:

                    // nothing to parse
                    break;
            }

            if (item.Type == InventoryType.Miniland)
            {
                item.MinilandObjectPoint = Convert.ToInt32(currentLine[2]);
                item.EffectValue = Convert.ToInt16(currentLine[8]);
                item.Width = Convert.ToByte(currentLine[9]);
                item.Height = Convert.ToByte(currentLine[10]);
            }

            if (item.EquipmentSlot != EquipmentType.Boots && item.EquipmentSlot != EquipmentType.Gloves ||
                item.Type != 0) return;

            item.FireResistance = Convert.ToByte(currentLine[7]);
            item.WaterResistance = Convert.ToByte(currentLine[8]);
            item.LightResistance = Convert.ToByte(currentLine[9]);
            item.DarkResistance = Convert.ToByte(currentLine[11]);
        }

        private static void FillMorphAndIndexValues(string[] currentLine, ItemDTO item)
        {
            switch (Convert.ToByte(currentLine[2]))
            {
                case 4:
                case 8:
                    item.Type = InventoryType.Equipment;
                    break;

                case 9:
                    item.Type = InventoryType.Main;
                    break;

                case 10:
                    item.Type = InventoryType.Etc;
                    break;

                default:
                    item.Type = (InventoryType) Enum.Parse(typeof(InventoryType), currentLine[2]);
                    break;
            }

            item.ItemType = currentLine[3] != "-1"
                ? (ItemType) Enum.Parse(typeof(ItemType), $"{(byte) item.Type}{currentLine[3]}")
                : ItemType.Weapon;
            item.ItemSubType = Convert.ToByte(currentLine[4]);
            item.EquipmentSlot = (EquipmentType) Enum.Parse(typeof(EquipmentType),
                currentLine[5] != "-1" && item.Type == InventoryType.Equipment ? currentLine[5] : "0");

            if (item.ItemType == ItemType.Special)
            {
                // add a value for design here design id might also come in handy
            }

            // item.DesignId = Convert.ToInt16(currentLine[6]);
            switch (item.VNum)
            {
                case 1906:
                    item.Morph = 2368;
                    item.Speed = 20;
                    item.WaitDelay = 3000;
                    break;

                case 1907:
                    item.Morph = 2370;
                    item.Speed = 20;
                    item.WaitDelay = 3000;
                    break;

                case 1965:
                    item.Morph = 2406;
                    item.Speed = 20;
                    item.WaitDelay = 3000;
                    break;

                case 5008:
                    item.Morph = 2411;
                    item.Speed = 20;
                    item.WaitDelay = 3000;
                    break;

                case 5117:
                    item.Morph = 2429;
                    item.Speed = 21;
                    item.WaitDelay = 3000;
                    break;

                case 5152:
                    item.Morph = 2432;
                    item.Speed = 21;
                    item.WaitDelay = 3000;
                    break;

                case 5173:
                    item.Morph = 2511;
                    item.Speed = 16;
                    item.WaitDelay = 3000;
                    break;

                case 5196:
                case 9076:
                    item.Morph = 2517;
                    item.Speed = 21;
                    item.WaitDelay = 3000;
                    break;

                case 5226: // Invisible locomotion, only 5 seconds with booster
                    item.Morph = 1817;
                    item.Speed = 20;
                    item.WaitDelay = 3000;
                    break;

                case 5228: // Invisible locoomotion, only 5 seconds with booster
                    item.Morph = 1819;
                    item.Speed = 20;
                    item.WaitDelay = 3000;
                    break;

                case 5232:
                    item.Morph = 2520;
                    item.Speed = 21;
                    item.WaitDelay = 3000;
                    break;

                case 5234:
                    item.Morph = 2522;
                    item.Speed = 20;
                    item.WaitDelay = 3000;
                    break;

                case 5236:
                    item.Morph = 2524;
                    item.Speed = 20;
                    item.WaitDelay = 3000;
                    break;

                case 5238:
                    item.Morph = 1817;
                    item.Speed = 20;
                    item.WaitDelay = 3000;
                    break;

                case 5240:
                    item.Morph = 1819;
                    item.Speed = 20;
                    item.WaitDelay = 3000;
                    break;

                case 5319:
                    item.Morph = 2526;
                    item.Speed = 22;
                    item.WaitDelay = 3000;
                    break;

                case 5321:
                    item.Morph = 2528;
                    item.Speed = 21;
                    item.WaitDelay = 3000;
                    break;

                case 5323:
                    item.Morph = 2530;
                    item.Speed = 22;
                    item.WaitDelay = 3000;
                    break;

                case 5330:
                    item.Morph = 2928;
                    item.Speed = 22;
                    item.WaitDelay = 3000;
                    break;

                case 5332:
                    item.Morph = 2930;
                    item.Speed = 14;
                    item.WaitDelay = 3000;
                    break;

                case 5360:
                    item.Morph = 2932;
                    item.Speed = 22;
                    item.WaitDelay = 3000;
                    break;

                case 5386:
                    item.Morph = 2934;
                    item.Speed = 21;
                    item.WaitDelay = 3000;
                    break;

                case 5387:
                    item.Morph = 2936;
                    item.Speed = 21;
                    item.WaitDelay = 3000;
                    break;

                case 5388:
                    item.Morph = 2938;
                    item.Speed = 21;
                    item.WaitDelay = 3000;
                    break;

                case 5389:
                    item.Morph = 2940;
                    item.Speed = 21;
                    item.WaitDelay = 3000;
                    break;

                case 5390:
                    item.Morph = 2942;
                    item.Speed = 21;
                    item.WaitDelay = 3000;
                    break;

                case 5391:
                    item.Morph = 2944;
                    item.Speed = 21;
                    item.WaitDelay = 3000;
                    break;

                case 5914:
                    item.Morph = 2513;
                    item.Speed = 14;
                    item.WaitDelay = 3000;
                    break;

                case 5997:
                    item.Morph = 3679;
                    item.Speed = 21;
                    item.WaitDelay = 3000;
                    break;

                case 9054:
                    item.Morph = 2368;
                    item.Speed = 20;
                    item.WaitDelay = 3000;
                    break;

                case 9055:
                    item.Morph = 2370;
                    item.Speed = 20;
                    item.WaitDelay = 3000;
                    break;

                case 9058:
                    item.Morph = 2406;
                    item.Speed = 20;
                    item.WaitDelay = 3000;
                    break;

                case 9065:
                    item.Morph = 2411;
                    item.Speed = 20;
                    item.WaitDelay = 3000;
                    break;

                case 9070:
                    item.Morph = 2429;
                    item.Speed = 21;
                    item.WaitDelay = 3000;
                    break;

                case 9073:
                    item.Morph = 2432;
                    item.Speed = 21;
                    item.WaitDelay = 3000;
                    break;

                case 9078:
                    item.Morph = 2520;
                    item.Speed = 21;
                    item.WaitDelay = 3000;
                    break;

                case 9079:
                    item.Morph = 2522;
                    item.Speed = 21;
                    item.WaitDelay = 3000;
                    break;

                case 9080:
                    item.Morph = 2524;
                    item.Speed = 21;
                    item.WaitDelay = 3000;
                    break;

                case 9081:
                    item.Morph = 1817;
                    item.Speed = 21;
                    item.WaitDelay = 3000;
                    break;

                case 9082:
                    item.Morph = 1819;
                    item.Speed = 21;
                    item.WaitDelay = 3000;
                    break;

                case 9083:
                    item.Morph = 2526;
                    item.Speed = 22;
                    item.WaitDelay = 3000;
                    break;

                case 9084:
                    item.Morph = 2528;
                    item.Speed = 22;
                    item.WaitDelay = 3000;
                    break;

                case 9085:
                    item.Morph = 2930;
                    item.Speed = 22;
                    item.WaitDelay = 3000;
                    break;

                case 9086:
                    item.Morph = 2928;
                    item.Speed = 22;
                    item.WaitDelay = 3000;
                    break;

                case 9087:
                    item.Morph = 2930;
                    item.Speed = 14;
                    item.WaitDelay = 3000;
                    break;

                case 9088:
                    item.Morph = 2932;
                    item.Speed = 22;
                    item.WaitDelay = 3000;
                    break;

                case 9090:
                    item.Morph = 2934;
                    item.Speed = 21;
                    item.WaitDelay = 3000;
                    break;

                case 9091:
                    item.Morph = 2936;
                    item.Speed = 21;
                    item.WaitDelay = 3000;
                    break;

                case 9092:
                    item.Morph = 2938;
                    item.Speed = 21;
                    item.WaitDelay = 3000;
                    break;

                case 9093:
                    item.Morph = 2940;
                    item.Speed = 21;
                    item.WaitDelay = 3000;
                    break;

                case 9094:
                    item.Morph = 2942;
                    item.Speed = 21;
                    item.WaitDelay = 3000;
                    break;

                case 9115:
                    item.Morph = 3679;
                    item.Speed = 21;
                    item.WaitDelay = 3000;
                    break;

                case 5834:
                    item.Morph = 3693;
                    item.Speed = 20;
                    item.WaitDelay = 3000;
                    break;

                case 9121:
                    item.Morph = 3693;
                    item.Speed = 20;
                    item.WaitDelay = 3000;
                    break;

                case 5712:
                case 9138:
                    item.Morph = 2440;
                    item.Speed = 20;
                    item.WaitDelay = 3000;
                    break;

                case 5714:
                case 9140:
                    item.Morph = 2442;
                    item.Speed = 22;
                    item.WaitDelay = 3000;
                    break;

                default:
                    if (item.EquipmentSlot.Equals(EquipmentType.Amulet))
                        switch (item.VNum)
                        {
                            case 4503:
                                item.EffectValue = 4544;
                                break;

                            case 4504:
                                item.EffectValue = 4294;
                                break;

                            case 282: // Red amulet
                                item.Effect = 791;
                                item.EffectValue = 3;
                                break;

                            case 283: // Blue amulet
                                item.Effect = 792;
                                item.EffectValue = 3;
                                break;

                            case 284: // Reinforcement amulet
                                item.Effect = 793;
                                item.EffectValue = 3;
                                break;

                            case 4264: // Heroic
                                item.Effect = 794;
                                item.EffectValue = 3;
                                break;

                            case 4262: // Random heroic
                                item.Effect = 795;
                                item.EffectValue = 3;
                                break;

                            default:
                                item.EffectValue = Convert.ToInt16(currentLine[7]);
                                break;
                        }
                    else
                        item.Morph = Convert.ToInt16(currentLine[7]);

                    break;
            }
        }
    }
}