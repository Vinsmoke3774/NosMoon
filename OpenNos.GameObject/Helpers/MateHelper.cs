using System;
using System.Collections.Generic;
using OpenNos.Core;
using OpenNos.Domain;
using OpenNos.GameObject.Npc;

namespace OpenNos.GameObject.Helpers
{
    public class MateHelper : Singleton<MateHelper>
    {
        #region Instantiation

        public MateHelper()
        {
            LoadXpData();
            LoadPetSkills();
            LoadTrainerUpgradeHits();
            LoadTrainerUpRate();
            LoadTrainerDownRate();
            LoadPartnerSkills();
        }

        #endregion

        #region Properties

        public short[] EggTypeMates => new short[] { 988 };

        public List<short> PartnerSpBuffs { get; set; }

        public Dictionary<int, int> PartoBuffs { get; set; }

        public List<int> PetSkills { get; set; }

        public short[] TrainerDownRate { get; private set; }

        public short[] TrainerUpgradeHits { get; private set; }

        public short[] TrainerUpRate { get; private set; }

        public double[] XpData { get; private set; }

        #endregion

        #region Methods

        public int GetCriticalChanceData(NpcMonster npcMonster, bool isPartner)
        {
            if (!isPartner)
            {
                switch (npcMonster.AttackClass)
                {
                    case 0:
                    case 1:
                        return 4 + npcMonster.WeaponData6;
                    case 2:
                        return 0 + npcMonster.WeaponData6;
                }
            }
            return 0;
        }

        public int GetCriticalData(NpcMonster npcMonster, bool isPartner)
        {
            if (!isPartner)
            {
                switch (npcMonster.AttackClass)
                {
                    case 0:
                    case 1:
                        return 70 + npcMonster.WeaponData7;
                    case 2:
                        return 0 + npcMonster.WeaponData7;
                }
            }
            return 0;
        }

        public void AddPartnerBuffs(ClientSession session, Mate mate)
        {
            if (PartoBuffs.TryGetValue(mate.Sp.Instance.ItemVNum, out var cardId) && session.Character.Buff.All(b => b.Card.CardId != cardId))
            {
                var sum = mate.Sp.GetLevelForAllSkill() / 3;
                if (sum < 1)
                {
                    sum = 1;
                }
                session.Character.AddBuff(new Buff((short)(cardId + (sum - 1)), mate.Level, isPermaBuff: true), mate.BattleEntity);
            }
        }

        public short GetUpgradeType(short morph)
        {
            switch (morph)
            {
                case 2043:
                    return 1;

                case 2044:
                    return 2;

                case 2045:
                    return 3;

                case 2046:
                    return 4;

                case 2047:
                    return 5;

                case 2048:
                    return 6;

                case 2310:
                    return 7;

                case 2317:
                    return 8;

                case 2323:
                    return 9;

                case 2325:
                    return 10;

                case 2333:
                    return 11;

                case 2334:
                    return 12;

                case 2343:
                    return 13;

                case 2355:
                    return 14;

                case 2356:
                    return 15;

                case 2367:
                    return 16;

                case 2368:
                    return 17;

                case 2371:
                    return 18;

                case 2372:
                    return 19;

                case 2373:
                    return 20;

                case 2374:
                    return 21;

                case 2376:
                    return 22;

                case 2377:
                    return 23;

                case 2378:
                    return 24;

                case 2537:
                    return 25;

                case 2538:
                    return 26;
            }
            return -1;
        }

        public int LoadConcentrate(NpcMonster npcMonster, short level, bool isPartner)
        {
            if (!isPartner)
            {
                double accD = 0;
                double accA = 0;

                switch (npcMonster.Race)
                {
                    case 0:
                        switch (npcMonster.AttackClass)
                        {
                            case 0:
                                accD = 22;
                                accA = 0;
                                break;
                            case 1:
                                accD = 28;
                                accA = 0;
                                break;
                        }
                        break;

                    case 1:
                        switch (npcMonster.AttackClass)
                        {
                            case 0:
                                accD = 30;
                                accA = 10;
                                break;
                            case 1:
                                accD = 44;
                                accA = 10;
                                break;
                        }
                        break;
                    case 2:
                        switch (npcMonster.AttackClass)
                        {
                            case 0:
                                accD = 25;
                                accA = 0;
                                break;
                            case 1:
                                accD = 34;
                                accA = 0;
                                break;
                        }
                        break;
                    case 3:
                        switch (npcMonster.AttackClass)
                        {
                            case 0:
                                accD = 25;
                                accA = 0;
                                break;
                            case 1:
                                accD = 34;
                                accA = 0;
                                break;
                        }
                        break;
                    case 4:
                        switch (npcMonster.AttackClass)
                        {
                            case 0:
                                accD = 30;
                                accA = 2;
                                break;
                            case 1:
                                accD = 44;
                                accA = 2;
                                break;
                        }
                        break;
                    case 5:
                        switch (npcMonster.AttackClass)
                        {
                            case 0:
                                accD = 22;
                                accA = -2;
                                break;
                            case 1:
                                accD = 28;
                                accA = -2;
                                break;
                        }
                        break;
                    case 6:
                        switch (npcMonster.AttackClass)
                        {
                            case 0:
                                accD = 25;
                                accA = 0;
                                break;
                            case 1:
                                accD = 34;
                                accA = 0;
                                break;
                        }
                        break;
                }

                int weaponLevel = level + npcMonster.Level - npcMonster.WeaponData1;

                switch (npcMonster.AttackClass)
                {
                    case 0:
                        return (int)(level + 4 * weaponLevel + accD + Math.Floor((level - 1) * ((npcMonster.PetInfo2 + accA) / 10)) + npcMonster.WeaponData5);
                    case 1:
                        return (int)(2 * level + 4 * weaponLevel + accD + Math.Floor((level - 1) * ((npcMonster.PetInfo2 + accA) / 10)) * 2 + npcMonster.WeaponData5);
                    case 2:
                        return 70 + npcMonster.WeaponData5;
                }
            }
            else
            {
                switch (npcMonster.AttackClass)
                {
                    case 0:
                        return (int)(level + 9 + Math.Floor((level - 1) * ((double)(npcMonster.PetInfo2) / 10)));
                    case 1:
                        return (int)(2 * (level + 9 + Math.Floor((level - 1) * ((double)(npcMonster.PetInfo2) / 10))));
                    case 2:
                        return 0;
                }
            }
            return 0;
        }

        public int LoadDefences(NpcMonster npcMonster, int level, bool isPartner, byte defType)
        {
            if (!isPartner)
            {
                int armorLevel = level + npcMonster.Level - npcMonster.ArmorData1;

                double defShortD = 0;
                double defLongD = 0;
                double defMagD = 0;
                double defShortA = 0;
                double defLongA = 0;
                double defMagA = 0;

                switch (npcMonster.Race)
                {
                    case 0:
                        defShortD = 16;
                        defLongD = 13.5;
                        defMagD = 11;
                        defShortA = 50;
                        defLongA = 50;
                        defMagA = 50;
                        break;
                    case 1:
                        defShortD = 20;
                        defLongD = 17;
                        defMagD = 19;
                        defShortA = 100;
                        defLongA = 100;
                        defMagA = 100;
                        break;
                    case 2:
                        defShortD = 15;
                        defLongD = 15;
                        defMagD = 15;
                        defShortA = 75;
                        defLongA = 50;
                        defMagA = 40;
                        break;
                    case 3:
                        defShortD = 15;
                        defLongD = 15;
                        defMagD = 15;
                        defShortA = 50;
                        defLongA = 50;
                        defMagA = 50;
                        break;
                    case 4:
                        defShortD = 17.4;
                        defLongD = 17.4;
                        defMagD = 17.4;
                        defShortA = 60;
                        defLongA = 60;
                        defMagA = 100;
                        break;
                    case 5:
                        defShortD = 13.4;
                        defLongD = 13.4;
                        defMagD = 13.4;
                        defShortA = 40;
                        defLongA = 40;
                        defMagA = 40;
                        break;
                    case 6:
                        defShortD = 11.5;
                        defLongD = 15;
                        defMagD = 25;
                        defShortA = 50;
                        defLongA = 50;
                        defMagA = 75;
                        break;
                }

                switch (defType)
                {
                    case 0:
                        return (int)(2 * armorLevel + defShortD + Math.Floor((armorLevel + 5) * ((double)8 / 100)) + (level - 1) * (((npcMonster.PetInfo1 * 10) + (defShortA - 5 * npcMonster.PetInfo1)) / 100) + npcMonster.ArmorData2);
                    case 1:
                        return (int)(2 * armorLevel + defLongD + Math.Floor((armorLevel + 5) * ((double)36 / 100)) + (level - 1) * (((npcMonster.PetInfo2 * 10) + (defLongA - 5 * npcMonster.PetInfo2)) / 100) + npcMonster.ArmorData3);
                    case 2:
                        return (int)(2 * armorLevel + defMagD + Math.Floor((armorLevel + 5) * ((double)4 / 100)) + (level - 1) * (((npcMonster.PetInfo3 * 10) + (defMagA - 5 * npcMonster.PetInfo3)) / 100) + npcMonster.ArmorData4);
                }
            }
            else
            {
                switch (defType)
                {
                    case 0:
                        return (int)(0.5 * (level + 9 + Math.Floor((level - 1) * ((double)(npcMonster.PetInfo1) / 10))));
                    case 1:
                        return (int)(0.5 * (level + 9 + Math.Floor((level - 1) * ((double)(npcMonster.PetInfo2) / 10))));
                    case 2:
                        return (int)(0.5 * (level + 9 + Math.Floor((level - 1) * ((double)(npcMonster.PetInfo3) / 10))));
                }
            }
            return 0;
        }

        public int LoadHpData(NpcMonster npcMonster, byte level)
        {
            double hpD = 0;
            double hpA = 0;
            double hpG = 0;

            switch (npcMonster.Race)
            {
                case 0:
                    hpD = 138;
                    hpA = 0;
                    hpG = 2;
                    break;
                case 1:
                    hpD = 610;
                    hpA = 10;
                    hpG = 10;
                    break;
                case 2:
                    hpD = 105;
                    hpA = 5;
                    hpG = 0;
                    break;
                case 3:
                    hpD = 205;
                    hpA = 0;
                    hpG = 0;
                    break;
                case 4:
                    hpD = 695;
                    hpA = 2;
                    hpG = 5;
                    break;
                case 5:
                    hpD = 263;
                    hpA = -2;
                    hpG = -3;
                    break;
                case 6:
                    hpD = 21;
                    hpA = 0;
                    hpG = -7;
                    break;
            }

            double hpX = level + Math.Floor((level - 1) * ((npcMonster.PetInfo1 + hpA) / 10));
            return (int)(0.5 * Math.Pow(hpX, 2) + (15.5 + hpG) * hpX + hpD + npcMonster.HpData);
        }

        public int LoadMaxMinDamageData(NpcMonster npcMonster, short level, bool isPartner, bool min)
        {
            double attackD = 0;
            double attackA = 0;
            double aPetInfo = 0;

            switch (npcMonster.Race)
            {
                case 0:
                    switch (npcMonster.AttackClass)
                    {
                        case 0:
                            attackD = 35;
                            attackA = 0;
                            aPetInfo = npcMonster.PetInfo1;
                            break;
                        case 1:
                            attackD = 30;
                            attackA = 0;
                            aPetInfo = npcMonster.PetInfo2;
                            break;
                        case 2:
                            attackD = 25;
                            attackA = 0;
                            aPetInfo = npcMonster.PetInfo3;
                            break;
                    }
                    break;
                case 1:
                    switch (npcMonster.AttackClass)
                    {
                        case 0:
                            attackD = 43;
                            attackA = 10;
                            aPetInfo = npcMonster.PetInfo1;
                            break;
                        case 1:
                            attackD = 38;
                            attackA = 10;
                            aPetInfo = npcMonster.PetInfo2;
                            break;
                        case 2:
                            attackD = 41;
                            attackA = 10;
                            aPetInfo = npcMonster.PetInfo3;
                            break;
                    }
                    break;
                case 2:
                    switch (npcMonster.AttackClass)
                    {
                        case 0:
                            attackD = 33;
                            attackA = 5;
                            aPetInfo = npcMonster.PetInfo1;
                            break;
                        case 1:
                            attackD = 33;
                            attackA = 0;
                            aPetInfo = npcMonster.PetInfo2;
                            break;
                        case 2:
                            attackD = 33;
                            attackA = -2;
                            aPetInfo = npcMonster.PetInfo3;
                            break;
                    }
                    break;
                case 3:
                    switch (npcMonster.AttackClass)
                    {
                        case 0:
                            attackD = 33;
                            attackA = 0;
                            aPetInfo = npcMonster.PetInfo1;
                            break;
                        case 1:
                            attackD = 33;
                            attackA = 0;
                            aPetInfo = npcMonster.PetInfo2;
                            break;
                        case 2:
                            attackD = 33;
                            attackA = 0;
                            aPetInfo = npcMonster.PetInfo3;
                            break;
                    }
                    break;
                case 4:
                    switch (npcMonster.AttackClass)
                    {
                        case 0:
                            attackD = 38;
                            attackA = 2;
                            aPetInfo = npcMonster.PetInfo1;
                            break;
                        case 1:
                            attackD = 38;
                            attackA = 2;
                            aPetInfo =
                            npcMonster.PetInfo2;
                            break;
                        case 2:
                            attackD = 38;
                            attackA = 10;
                            aPetInfo = npcMonster.PetInfo3;
                            break;
                    }
                    break;
                case 5:
                    switch (npcMonster.AttackClass)
                    {
                        case 0:
                            attackD = 30;
                            attackA = -2;
                            aPetInfo = npcMonster.PetInfo1;
                            break;
                        case 1:
                            attackD = 30;
                            attackA = -2;
                            aPetInfo = npcMonster.PetInfo2;
                            break;
                        case 2:
                            attackD = 30;
                            attackA = -2;
                            aPetInfo = npcMonster.PetInfo3;
                            break;
                    }
                    break;
                case 6:
                    switch (npcMonster.AttackClass)
                    {
                        case 0:
                            attackD = 26;
                            attackA = 0;
                            aPetInfo = npcMonster.PetInfo1;
                            break;
                        case 1:
                            attackD = 33;
                            attackA = 0;
                            aPetInfo = npcMonster.PetInfo2;
                            break;
                        case 2:
                            attackD = 53;
                            attackA = 5;
                            aPetInfo = npcMonster.PetInfo3;
                            break;
                    }
                    break;
            }

            int weaponLevel = level + npcMonster.Level - npcMonster.WeaponData1;
            double differenceMaxMin = 0;

            if (npcMonster.Winfo2 > 1)
            {
                differenceMaxMin = Math.Floor((double)(level + 2) / (npcMonster.Winfo2 - 1) + 1);
            }

            if (!isPartner)
            {
                if (min)
                {
                    return (int)(level + (attackD - 7.2) + 3.2 * weaponLevel + Math.Floor((level - 1) * ((aPetInfo + attackA) / 10)) + npcMonster.WeaponData3 + differenceMaxMin);
                }
                else
                {
                    return (int)(level + (attackD) + 4.8 * weaponLevel + Math.Floor((level - 1) * ((aPetInfo + attackA) / 10)) + npcMonster.WeaponData4 + differenceMaxMin);
                }
            }
            else
            {
                if (min)
                {
                    return (int)(level + 9 + Math.Floor((level - 1) * ((aPetInfo) / 10)));
                }
                else
                {
                    return (int)(level + 9 + Math.Floor((level - 1) * ((aPetInfo) / 10)));
                }
            }
        }

        public int LoadMpData(NpcMonster npcMonster, byte level)
        {
            double mpD = 0;
            double mpA = 0;
            double mpG = 0;
            double mpZ = 0;

            switch (npcMonster.Race)
            {
                case 0:
                    mpD = 4.75;
                    mpA = 0;
                    mpG = 0;
                    mpZ = 0;
                    break;
                case 1:
                    mpD = 178.75;
                    mpA = 10;
                    mpG = 8;
                    mpZ = 0;
                    break;
                case 2:
                    mpD = 50.75;
                    mpA = -2;
                    mpG = 4;
                    mpZ = 0;
                    break;
                case 3:
                    mpD = 50.75;
                    mpA = 0;
                    mpG = 4;
                    mpZ = 0;
                    break;
                case 4:
                    mpD = 385.75;
                    mpA = 10;
                    mpG = 6;
                    mpZ = 1;
                    break;
                case 5:
                    mpD = 23.75;
                    mpA = -2;
                    mpG = 2;
                    mpZ = 1;
                    break;
                case 6:
                    mpD = 705.75;
                    mpA = 5;
                    mpG = 14;
                    mpZ = 0;
                    break;
            }

            double mpX = level + Math.Floor((level - 1) * ((npcMonster.PetInfo3 + mpA) / 10) + mpZ);
            return (int)(Math.Floor((5.25 + mpG) * mpX + mpD) + ((int)((mpX - 2) / 4) * 2) * ((mpX - 2) % 4 + 1 + (int)((mpX - 6) / 4) * 2) + npcMonster.MpData);
        }

        public void LoadPetSkills()
        {
            PetSkills = new List<int>
            {
                1513, // Purcival
                1514, // Baron scratch ?
                1515, // Amiral (le chat chelou)
                1516, // roi des pirates pussifer
                1524 // Miaou fou
            };
        }

        public void LoadTrainerDownRate()
        {
            TrainerDownRate = new short[] { 0, 7, 13, 16, 28, 29, 33, 36, 50, 60 };
        }

        public void LoadTrainerUpgradeHits()
        {
            TrainerUpgradeHits = new short[10];

            short baseValue = 0;

            for (int i = 0; i < 10; i++)
            {
                baseValue += 50;
                TrainerUpgradeHits[i] = baseValue;
            }
        }

        public void LoadTrainerUpRate()
        {
            TrainerUpRate = new short[] { 67, 67, 44, 34, 22, 15, 14, 8, 1, 0 };
        }

        public void LoadXpData()
        {
            // Load XpData
            XpData = new double[256];
            double[] v = new double[256];
            double var = 1;
            v[0] = 540;
            v[1] = 960;
            XpData[0] = 300;
            for (int i = 2; i < v.Length; i++)
            {
                v[i] = v[i - 1] + 420 + 120 * (i - 1);
            }

            for (int i = 1; i < XpData.Length; i++)
            {
                if (i < 79)
                {
                    switch (i)
                    {
                        case 14:
                            var = 6 / 3d;
                            break;

                        case 39:
                            var = 19 / 3d;
                            break;

                        case 59:
                            var = 70 / 3d;
                            break;
                    }

                    XpData[i] = Convert.ToInt64(XpData[i - 1] + var * v[i - 1]);
                }

                if (i < 79)
                {
                    continue;
                }

                switch (i)
                {
                    case 79:
                        var = 5000;
                        break;

                    case 82:
                        var = 9000;
                        break;

                    case 84:
                        var = 13000;
                        break;
                }

                XpData[i] = Convert.ToInt64(XpData[i - 1] + var * (i + 2) * (i + 2));
            }
        }

        public void RemoveMateBuffs(Mate mate)
        {
            if (mate.Owner == null)
            {
                return;
            }

            if (mate.MateType == MateType.Pet && mate.Monster.BuffId.HasValue)
            {
                mate.Owner.RemoveBuff(mate.Monster.BuffId.Value, true);
            }

            foreach (var val in PartnerSpBuffs)
            {
                mate.Owner.RemoveBuff(val, true);
            }
        }

        private void LoadPartnerSkills()
        {
            PartoBuffs = new Dictionary<int, int>
            {
                { 4825 , 3000 }, // Vénus
                { 4326 , 3007 }, // Guerrier Squelettique Ragnar
                { 4405 , 3014 }, // Yuna
                { 4413 , 3021 }, // Cupidia
                { 4446, 3028 } // Perti
            };
            PartnerSpBuffs = new List<short>
            {
                3000,
                3001,
                3002,
                3003,
                3004,
                3005,
                3006,
                3007,
                3008,
                3009,
                3010,
                3011,
                3012,
                3013,
                3014,
                3015,
                3016,
                3017,
                3018,
                3019,
                3020,
                3021,
                3022,
                3023,
                3024,
                3025,
                3026,
                3027,
                3028,
                3029,
                3030,
                3031,
                3032,
                3033,
                3034
            };
        }

        #endregion
    }
}