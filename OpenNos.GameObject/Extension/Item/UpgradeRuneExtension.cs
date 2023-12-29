using NosTale.Configuration;
using OpenNos.DAL;
using OpenNos.Data;
using OpenNos.Domain;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;
using System.Collections.Generic;
using System.Linq;

namespace OpenNos.GameObject.Extension.Item
{
    public static class UpgradeRuneExtension
    {
        #region Methods

        public static void UpgradeRune(this ItemInstance e, ClientSession s, UpgradeRuneType protectionType)
        {
            if (!e.CanUpgrade())
            {
                s.SendShopEnd();
                return;
            }

            if (protectionType != UpgradeRuneType.None)
                if (s.Character.Inventory.CountItem(protectionType == UpgradeRuneType.Premium ? 5813 : 5823) < 1)
                {
                    s.SendShopEnd();
                    return;
                }

            var isProtected = false;

            int value = e.RuneAmount;

            var get = GameConfiguration.RUpgrade;

            if (s.Character.Gold < get.GoldPrice[value])
            {
                s.SendShopEnd();
                return;
            }

            foreach (var item in get.Item[value])
                if (s.Character.Inventory.CountItem(item.Id) < item.Quantity)
                {
                    // Not Enough Item
                    s.SendShopEnd();
                    return;
                }

            switch (protectionType)
            {
                case UpgradeRuneType.Premium:
                case UpgradeRuneType.Basic:
                    isProtected = true;
                    s.Character.Inventory.RemoveItemAmount(protectionType == UpgradeRuneType.Premium ? 5813 : 5823);
                    break;
            }

            if (isProtected)
            {
                var rndDestroy = ServerManager.RandomNumber(2, 5);

                // Lmao Official give 50% so Fucking Broken Give Like 25% ↓
                if (rndDestroy != 3)
                {
                    foreach (var item in get.Item[value])
                        s.Character.Inventory.RemoveItemAmount(item.Id, item.Quantity);
                }
                else
                {
                    s.SendPacket(UserInterfaceHelper.GenerateMsg("All item was Not Deleted Youpiiiii", 0));
                    s.SendPacket(UserInterfaceHelper.GenerateSay("All item was Not Deleted Youpiiiii", 11));
                }
            }
            else
            {
                foreach (var item in get.Item[value]) s.Character.Inventory.RemoveItemAmount(item.Id, item.Quantity);
            }

            var rnd = ServerManager.RandomNumber();
            string msg;
            int effectId;
            if (rnd < get.PercentBreaked[value]) // fail + level --
            {
                if (!isProtected)
                {
                    e.IsBreaked = true;
                    effectId = 3003;
                    msg = $"The {e.Item.Name} Rune improvement failed ! and broke.";
                }
                else
                {
                    effectId = 3004;
                    msg = $"The {e.Item.Name} Rune improvement failed ! and was saved with the scroll !";
                }
            }
            else if (rnd < get.PercentFail[value] - (protectionType == UpgradeRuneType.Premium ? 2 : 0)) // fail
            {
                effectId = 3004;
                msg = $"The {e.Item.Name} Rune improvement failed !";
            }
            else // success
            {
                e.RuneAmount++;
                effectId = 3005;
                msg = $"The {e.Item.Name} Rune has been improved ! +{e.RuneAmount}";
                e.ApplyRandomRune(s, msg);
            }

            s.SendPacket(e.GenerateInventoryAdd());
            s.GoldLess(get.GoldPrice[value]);
            s.CurrentMapInstance.Broadcast(
                StaticPacketHelper.GenerateEff(UserType.Player, s.Character.CharacterId, effectId),
                s.Character.PositionX, s.Character.PositionY);
            s.SendPacket(UserInterfaceHelper.GenerateMsg(msg, 0));
            s.SendPacket(UserInterfaceHelper.GenerateSay(msg, 11));
            s.SendPacket(UserInterfaceHelper.GenerateGuri(19, 1, s.Character.CharacterId, 2388));
            s.SendShopEnd();
        }

        private static void ApplyBuffEffect(this ItemInstance e, ClientSession s, string msg)
        {
            var possibleBuffByTypeAndSubType = GetList();

            var rndmBuff = ServerManager.RandomNumber(0, possibleBuffByTypeAndSubType.Length);

            var getBuff = possibleBuffByTypeAndSubType[rndmBuff];

            var listRune = DAOFactory.ShellEffectDAO.LoadByEquipmentSerialId(e.EquipmentSerialId, true);

            var BuffEffect = listRune.Where(b => b.EffectLevel == (ShellEffectLevelType)getBuff.SubType && b.Effect == (byte)getBuff.Type).FirstOrDefault();

            if (BuffEffect != null)
            {
                BuffEffect.Upgrade++;

                if (BuffEffect.Upgrade == 6)
                {
                    // can't > 5 so Regenerate this
                    BuffEffect.Upgrade--;
                    e.ApplyRuneBuff(s, msg);
                    return;
                }

                var upgr = BuffEffect.Upgrade -= 1;
                BuffEffect.Value = BuffEffect.Upgrade;
                BuffEffect.Type = getBuff.ValueByLevel[upgr];
            }

            BuffEffect = new ShellEffectDTO
            {
                EffectLevel = (ShellEffectLevelType)getBuff.SubType,
                Effect = (byte)getBuff.Type,
                Value = 1,
                Type = getBuff.ValueByLevel[0],
                Upgrade = 1,
                IsRune = true,
                EquipmentSerialId = e.EquipmentSerialId
            };

            e.RuneEffects.Add(BuffEffect);

            DAOFactory.ShellEffectDAO.InsertOrUpdate(BuffEffect);

            s.SendPacket(
                $"ru_suc 0 {BuffEffect.Effect}.{(byte)BuffEffect.EffectLevel}.{BuffEffect.Value * 4}.{BuffEffect.Type * 4}.{BuffEffect.Upgrade} " +
                msg);
        }

        private static void ApplyRandomRune(this ItemInstance e, ClientSession s, string msg)
        {
            switch (e.RuneAmount)
            {
                case 3:
                case 6:
                case 9:
                case 12:
                case 15:
                    e.ApplyRuneBuff(s, msg);
                    break;

                default:
                    e.ApplyRuneEffect(s, msg);
                    break;
            }
        }

        private static void ApplyRuneBuff(this ItemInstance e, ClientSession s, string msg)
        {
            var listRune = DAOFactory.ShellEffectDAO.LoadByEquipmentSerialId(e.EquipmentSerialId, true);

            if (listRune.Count() == 2)
            {
                UpgradeOnlyBuffEffect(e, s, msg);
                return;
            }

            ApplyBuffEffect(e, s, msg);
        }

        private static void ApplyRuneEffect(this ItemInstance e, ClientSession s, string msg)
        {
            // 13 Possible Type
            var possibleListTypeandSubType = new[]
            {
                new PossibleTypeAndSubtype
                {
                    Type = 3,
                    SubType = 0,
                    ValueByLevel = new short[] {20, 40, 80, 150, 200}
                },
                new PossibleTypeAndSubtype
                {
                    Type = 44,
                    SubType = 1,
                    ValueByLevel = new short[] {1, 2, 4, 7, 10}
                },
                new PossibleTypeAndSubtype
                {
                    Type = 9,
                    SubType = 0,
                    ValueByLevel = new short[] {20, 40, 80, 150, 200}
                },
                new PossibleTypeAndSubtype
                {
                    Type = 44,
                    SubType = 1,
                    ValueByLevel = new short[] {1, 2, 4, 7, 10}
                },
                new PossibleTypeAndSubtype
                {
                    Type = 4,
                    SubType = (short) (e.Item.Class == 8 ? 3 : 0),
                    ValueByLevel = e.Item.Class == 8 ? new short[] {1, 3, 5, 7, 15} : new short[] {20, 40, 70, 110, 150}
                },
                new PossibleTypeAndSubtype
                {
                    Type = (short) (e.Item.Class == 8 ? 103 : 102),
                    SubType = 4,
                    ValueByLevel = new short[] {1, 2, 4, 7, 10}
                },
                new PossibleTypeAndSubtype
                {
                    Type = 7,
                    SubType = 4,
                    ValueByLevel = new short[] {10, 15, 20, 30, 50}
                },
                new PossibleTypeAndSubtype
                {
                    Type = 13,
                    SubType = 0,
                    ValueByLevel = new short[] {3, 5, 7, 10, 15}
                },
                new PossibleTypeAndSubtype
                {
                    Type = 33,
                    SubType = 0,
                    ValueByLevel = new short[] {200, 400, 800, 1300, 2000}
                },
                new PossibleTypeAndSubtype
                {
                    Type = 110,
                    SubType = 2,
                    ValueByLevel = new short[] {1, 2, 4, 7, 10}
                },
                new PossibleTypeAndSubtype
                {
                    Type = 33,
                    SubType = 1,
                    ValueByLevel = new short[] {200, 400, 800, 1300, 2000}
                },
                new PossibleTypeAndSubtype
                {
                    Type = 110,
                    SubType = 3,
                    ValueByLevel = new short[] {1, 2, 4, 7, 10}
                },
                new PossibleTypeAndSubtype
                {
                    Type = 104,
                    SubType = 3,
                    ValueByLevel = new short[] {1, 2, 4, 7, 10}
                }
            };

            var rndm = ServerManager.RandomNumber(0, possibleListTypeandSubType.Length);

            var getTypeAndSubtype = possibleListTypeandSubType[rndm];

            var ShellEffect = DAOFactory.ShellEffectDAO.LoadByEquipmentSerialId(e.EquipmentSerialId, true).Where(
                b => b.EffectLevel == (ShellEffectLevelType)getTypeAndSubtype.SubType &&
                     b.Effect == (byte)getTypeAndSubtype.Type).FirstOrDefault();

            if (ShellEffect != null)
            {
                ShellEffect.Upgrade++;

                if (ShellEffect.Upgrade == 6)
                {
                    // can't > 5 so Regenerate this
                    ShellEffect.Upgrade--;
                    e.ApplyRuneEffect(s, msg);
                    return;
                }

                var upgr = ShellEffect.Upgrade -= 1;
                ShellEffect.Value = getTypeAndSubtype.ValueByLevel[upgr];
            }

            ShellEffect = new ShellEffectDTO
            {
                EffectLevel = (ShellEffectLevelType)getTypeAndSubtype.SubType,
                Effect = (byte)getTypeAndSubtype.Type,
                Value = getTypeAndSubtype.ValueByLevel[0],
                Type = 0,
                Upgrade = 1,
                IsRune = true,
                EquipmentSerialId = e.EquipmentSerialId
            };

            e.RuneEffects.Add(ShellEffect);

            DAOFactory.ShellEffectDAO.InsertOrUpdate(ShellEffect);

            s.SendPacket(
                $"ru_suc 0 {ShellEffect.Effect}.{(byte)ShellEffect.EffectLevel}.{ShellEffect.Value * 4}.{ShellEffect.Type * 4}.{ShellEffect.Upgrade} " +
                msg);
        }

        private static bool CanUpgrade(this ItemInstance e)
        {
            if (e.Item.EquipmentSlot != EquipmentType.MainWeapon) return false;

            if (e.RuneAmount == 15) return false;

            if (e.Item.LevelMinimum < 80 && !e.Item.IsHeroic) return false;

            if (e.IsBreaked) return false;

            return true;
        }

        private static PossibleTypeAndSubtype[] GetList()
        {
            var possibleBuffByTypeAndSubType = new[]
            {
                new PossibleTypeAndSubtype
                {
                    Type = 105,
                    SubType = 0,
                    ValueByLevel = new short[] {1900, 1901, 1902, 1903, 1904}
                },
                new PossibleTypeAndSubtype
                {
                    Type = 105,
                    SubType = 1,
                    ValueByLevel = new short[] {1905, 1906, 1907, 1908, 1909}
                },
                new PossibleTypeAndSubtype
                {
                    Type = 105,
                    SubType = 2,
                    ValueByLevel = new short[] {1910, 1911, 1912, 1913, 1914}
                },
                new PossibleTypeAndSubtype
                {
                    Type = 105,
                    SubType = 3,
                    ValueByLevel = new short[] {1915, 1916, 1917, 1918, 1919}
                },
                new PossibleTypeAndSubtype
                {
                    Type = 105,
                    SubType = 4,
                    ValueByLevel = new short[] {1920, 1921, 1922, 1923, 1924}
                },
                new PossibleTypeAndSubtype
                {
                    Type = 106,
                    SubType = 0,
                    ValueByLevel = new short[] {1925, 1926, 1927, 1928, 1929}
                },
                new PossibleTypeAndSubtype
                {
                    Type = 106,
                    SubType = 1,
                    ValueByLevel = new short[] {1930, 1931, 1932, 1933, 1934}
                },
                new PossibleTypeAndSubtype
                {
                    Type = 106,
                    SubType = 2,
                    ValueByLevel = new short[] {1935, 1936, 1937, 1938, 1939}
                },
                new PossibleTypeAndSubtype
                {
                    Type = 106,
                    SubType = 3,
                    ValueByLevel = new short[] {1940, 1941, 1942, 1943, 1944}
                },
                new PossibleTypeAndSubtype
                {
                    Type = 106,
                    SubType = 4,
                    ValueByLevel = new short[] {1945, 1946, 1947, 1948, 1949}
                }
            };

            return possibleBuffByTypeAndSubType;
        }

        private static void UpgradeOnlyBuffEffect(this ItemInstance e, ClientSession s, string msg)
        {
            var possibleBuffByTypeAndSubType = GetList();

            var listRune = DAOFactory.ShellEffectDAO.LoadByEquipmentSerialId(e.EquipmentSerialId, true).Where(b => b.Type != 0).ToList();

            List<ShellEffectDTO> list = new List<ShellEffectDTO>();
            list.AddRange(listRune);

            var rndmBuff = ServerManager.RandomNumber(0, list.Count());
            ShellEffectDTO effect;
            switch (rndmBuff)
            {
                case 1:
                    effect = list[0];
                    break;

                default:
                    effect = list[1];
                    break;
            }

            if (effect == null)
            {
                return;
            }

            effect.Type++;
            effect.Upgrade++;

            e.RuneEffects.Add(effect);

            DAOFactory.ShellEffectDAO.InsertOrUpdate(effect);

            s.SendPacket(
                $"ru_suc 0 {effect.Effect}.{(byte)effect.EffectLevel}.{effect.Value * 4}.{effect.Type * 4}.{effect.Upgrade} " +
                msg);
        }

        #endregion
    }
}