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

using OpenNos.Domain;
using System;

namespace OpenNos.GameObject.Helpers
{
    public class CharacterHelper
    {
        #region Members

        private static int[,] _criticalDist;

        private static int[,] _criticalDistRate;

        private static int[,] _criticalHit;

        private static int[,] _criticalHitRate;

        private static int[,] _distDef;
        
        private static int[,] _distDodge;

        private static int[,] _distRate;

        private static double[,] _jobXpData;

        private static double[] _heroXpData;

        private static int[,] _hitDef;

        private static int[,] _hitDodge;

        private static int[,] _hitRate;

        private static int[,] _hp;

        private static int[] _hpHealth;

        private static int[] _hpHealthStand;

        private static int[,] _magicalDef;

        private static int[,] _maxDist;

        private static int[,] _maxHit;

        private static int[,] _minDist;

        // difference between class
        private static int[,] _minHit;

        private static int[,] _mp;

        private static int[] _mpHealth;

        private static int[] _mpHealthStand;

        // STAT DATA
        private static byte[] _speedData;

        private static double[,] _spxpData;

        // same for all class
        private static double[] _xpData;

        public static readonly int[][] ClassConstants = new int[][] { new int[] { 0, 0, 0 }, new int[] { 8, 2, 0 }, new int[] { 3, 6, 1 }, new int[] { 0, 2, 8 }, new int[] { 5, 3, 2 } };

        #endregion

        #region Instantiation

        public CharacterHelper()
        {
            loadSpeedData();
            LoadJobXPData();
            loadSPXPData();
            loadHeroXpData();
            loadXPData();
            loadHPData();
            loadMPData();
            LoadStats();
            loadHPHealth();
            loadMPHealth();
            loadHPHealthStand();
            loadMPHealthStand();
        }

        #endregion

        #region Properties

        public static double[] HeroXpData
        {
            get
            {
                if (_heroXpData == null)
                {
                    new CharacterHelper();
                }
                return _heroXpData;
            }
        }

        public static int[,] HPData
        {
            get
            {
                if (_hp == null)
                {
                    new CharacterHelper();
                }
                return _hp;
            }
        }

        public static int[] HPHealth
        {
            get
            {
                if (_hpHealth == null)
                {
                    new CharacterHelper();
                }
                return _hpHealth;
            }
        }

        public static int[] HPHealthStand
        {
            get
            {
                if (_hpHealthStand == null)
                {
                    new CharacterHelper();
                }
                return _hpHealthStand;
            }
        }

        public static int[,] MPData
        {
            get
            {
                if (_mp == null)
                {
                    new CharacterHelper();
                }
                return _mp;
            }
        }

        public static int[] MPHealth
        {
            get
            {
                if (_mpHealth == null)
                {
                    new CharacterHelper();
                }
                return _mpHealth;
            }
        }

        public static int[] MPHealthStand
        {
            get
            {
                if (_mpHealthStand == null)
                {
                    new CharacterHelper();
                }
                return _mpHealthStand;
            }
        }

        public static double[,] JobXpData
        {
            get
            {
                if (_jobXpData == null)
                {
                    new CharacterHelper();
                }
                return _jobXpData;
            }
        }

        public static byte[] SpeedData
        {
            get
            {
                if (_speedData == null)
                {
                    new CharacterHelper();
                }
                return _speedData;
            }
        }

        public static double[,] SPXPData
        {
            get
            {
                if (_spxpData == null)
                {
                    new CharacterHelper();
                }
                return _spxpData;
            }
        }

        public static double[] XPData
        {
            get
            {
                if (_xpData == null)
                {
                    new CharacterHelper();
                }
                return _xpData;
            }
        }

        #endregion

        #region Methods

        public static byte AuthorityChatColor(AuthorityType authority)
        {
            switch (authority)
            {
                case AuthorityType.GS:
                case AuthorityType.TGS:
                    return 12;

                case AuthorityType.TMOD:
                case AuthorityType.MOD:
                case AuthorityType.SMOD:
                case AuthorityType.BA:
                case AuthorityType.TGM:
                case AuthorityType.GM:
                case AuthorityType.SGM:
                case AuthorityType.GD:
                case AuthorityType.TM:
                case AuthorityType.CM:
                case AuthorityType.DEV:
                case AuthorityType.Administrator:
                case AuthorityType.Founder:
                case AuthorityType.God:
                    return 15;

                default:
                    return 0;
            }
        }

        public static short AuthorityColor(AuthorityType authority)
        {
            switch (authority)
            {
                case AuthorityType.GS:
                case AuthorityType.TGS:
                    return 500;

                case AuthorityType.TMOD:
                case AuthorityType.MOD:
                case AuthorityType.SMOD:
                case AuthorityType.BA:
                case AuthorityType.TGM:
                case AuthorityType.GM:
                case AuthorityType.SGM:
                case AuthorityType.GD:
                case AuthorityType.TM:
                case AuthorityType.CM:
                case AuthorityType.DEV:
                case AuthorityType.Administrator:
                case AuthorityType.Founder:
                case AuthorityType.God:
                    return 500;

                default:
                    return 50;
            }
        }

        public static float ExperiencePenalty(byte playerLevel, byte monsterLevel)
        {
            int leveldifference = playerLevel - monsterLevel;
            float penalty;

            // penalty calculation
            switch (leveldifference)
            {
                case 6:
                    penalty = 0.9f;
                    break;

                case 7:
                    penalty = 0.7f;
                    break;

                case 8:
                    penalty = 0.5f;
                    break;

                case 9:
                    penalty = 0.3f;
                    break;

                default:
                    if (leveldifference > 9)
                    {
                        penalty = 0.1f;
                    }
                    else if (leveldifference > 18)
                    {
                        penalty = 0.05f;
                    }
                    else
                    {
                        penalty = 1f;
                    }
                    break;
            }

            return penalty;
        }

        public static float GoldPenalty(byte playerLevel, byte monsterLevel)
        {
            int leveldifference = playerLevel - monsterLevel;
            float penalty;

            // penalty calculation
            switch (leveldifference)
            {
                case 5:
                    penalty = 0.9f;
                    break;

                case 6:
                    penalty = 0.7f;
                    break;

                case 7:
                    penalty = 0.5f;
                    break;

                case 8:
                    penalty = 0.3f;
                    break;

                case 9:
                    penalty = 0.2f;
                    break;

                default:
                    if (leveldifference > 9 && leveldifference < 19)
                    {
                        penalty = 0.1f;
                    }
                    else if (leveldifference > 18 && leveldifference < 30)
                    {
                        penalty = 0.05f;
                    }
                    else if (leveldifference > 30)
                    {
                        penalty = 0f;
                    }
                    else
                    {
                        penalty = 1f;
                    }
                    break;
            }

            return penalty;
        }

        public static long LoadFairyXPData(long elementRate)
        {
            if (elementRate < 40)
            {
                return elementRate * elementRate + 50;
            }
            return (elementRate * elementRate + 50) * 3;
        }

        public static int LoadFamilyXPData(byte familyLevel)
        {
            return familyLevel switch
            {
                1 => 100000,
                2 => 250000,
                3 => 370000,
                4 => 560000,
                5 => 840000,
                6 => 1260000,
                7 => 1900000,
                8 => 2850000,
                9 => 3570000,
                10 => 3830000,
                11 => 4150000,
                12 => 4750000,
                13 => 5500000,
                14 => 6500000,
                15 => 7000000,
                16 => 8500000,
                17 => 9500000,
                18 => 10000000,
                19 => 17000000,
                _ => 999999999,
            };
        }

        public static int MagicalDefence(ClassType @class, byte level)
        {
            if (_magicalDef == null)
            {
                new CharacterHelper();
            }
            return _magicalDef[(int)@class, level - 1];
        }

        public static int MaxDistance(ClassType @class, byte level)
        {
            if (_maxDist == null)
            {
                new CharacterHelper();
            }
            return _maxDist[(int)@class, level - 1];
        }

        public static int MaxHit(ClassType @class, byte level)
        {
            if (_maxHit == null)
            {
                new CharacterHelper();
            }
            return _maxHit[(int)@class, level -1];
        }

        public static int MinDistance(ClassType @class, byte level)
        {
            if (_minDist == null)
            {
                new CharacterHelper();
            }
            return _minDist[(int)@class, level - 1];
        }

        public static int MinHit(ClassType @class, byte level)
        {
            if (_minHit == null)
            {
                new CharacterHelper();
            }
            return _minHit[(int)@class, level - 1];
        }

        public static int RarityPoint(short rarity, short lvl, bool armor)
        {
            int p;
            switch (rarity)
            {
                case 0:
                    p = 0;
                    break;

                case 1:
                    p = 1;
                    break;

                case 2:
                    p = 2;
                    break;

                case 3:
                    p = 3;
                    break;

                case 4:
                    p = 4;
                    break;

                case 5:
                    p = 5;
                    break;

                case 6:
                    p = 7;
                    break;

                case 7:
                    p = 10;
                    break;

                case 8:
                    p = 15;
                    break;

                default:
                    p = rarity * 2;
                    break;
            }
            return p * ((lvl / (armor ? 10 : 5)) + 1);
        }

        public static int SlPoint(short spPoint, short mode)
        {
            try
            {
                int point = 0;
                switch (mode)
                {
                    case 0:
                        if (spPoint <= 10)
                        {
                            point = spPoint;
                        }
                        else if (spPoint <= 28)
                        {
                            point = 10 + ((spPoint - 10) / 2);
                        }
                        else if (spPoint <= 88)
                        {
                            point = 19 + ((spPoint - 28) / 3);
                        }
                        else if (spPoint <= 168)
                        {
                            point = 39 + ((spPoint - 88) / 4);
                        }
                        else if (spPoint <= 268)
                        {
                            point = 59 + ((spPoint - 168) / 5);
                        }
                        else if (spPoint <= 334)
                        {
                            point = 79 + ((spPoint - 268) / 6);
                        }
                        else if (spPoint <= 383)
                        {
                            point = 90 + ((spPoint - 334) / 7);
                        }
                        else if (spPoint <= 391)
                        {
                            point = 97 + ((spPoint - 383) / 8);
                        }
                        else if (spPoint <= 400)
                        {
                            point = 98 + ((spPoint - 391) / 9);
                        }
                        else if (spPoint <= 410)
                        {
                            point = 99 + ((spPoint - 400) / 10);
                        }

                        break;

                    case 2:
                        if (spPoint <= 20)
                        {
                            point = spPoint;
                        }
                        else if (spPoint <= 40)
                        {
                            point = 20 + ((spPoint - 20) / 2);
                        }
                        else if (spPoint <= 70)
                        {
                            point = 30 + ((spPoint - 40) / 3);
                        }
                        else if (spPoint <= 110)
                        {
                            point = 40 + ((spPoint - 70) / 4);
                        }
                        else if (spPoint <= 210)
                        {
                            point = 50 + ((spPoint - 110) / 5);
                        }
                        else if (spPoint <= 270)
                        {
                            point = 70 + ((spPoint - 210) / 6);
                        }
                        else if (spPoint <= 410)
                        {
                            point = 80 + ((spPoint - 270) / 7);
                        }

                        break;

                    case 1:
                        if (spPoint <= 10)
                        {
                            point = spPoint;
                        }
                        else if (spPoint <= 48)
                        {
                            point = 10 + ((spPoint - 10) / 2);
                        }
                        else if (spPoint <= 81)
                        {
                            point = 29 + ((spPoint - 48) / 3);
                        }
                        else if (spPoint <= 161)
                        {
                            point = 40 + ((spPoint - 81) / 4);
                        }
                        else if (spPoint <= 236)
                        {
                            point = 60 + ((spPoint - 161) / 5);
                        }
                        else if (spPoint <= 290)
                        {
                            point = 75 + ((spPoint - 236) / 6);
                        }
                        else if (spPoint <= 360)
                        {
                            point = 84 + ((spPoint - 290) / 7);
                        }
                        else if (spPoint <= 400)
                        {
                            point = 97 + ((spPoint - 360) / 8);
                        }
                        else if (spPoint <= 410)
                        {
                            point = 99 + ((spPoint - 400) / 10);
                        }

                        break;

                    case 3:
                        if (spPoint <= 10)
                        {
                            point = spPoint;
                        }
                        else if (spPoint <= 50)
                        {
                            point = 10 + ((spPoint - 10) / 2);
                        }
                        else if (spPoint <= 110)
                        {
                            point = 30 + ((spPoint - 50) / 3);
                        }
                        else if (spPoint <= 150)
                        {
                            point = 50 + ((spPoint - 110) / 4);
                        }
                        else if (spPoint <= 200)
                        {
                            point = 60 + ((spPoint - 150) / 5);
                        }
                        else if (spPoint <= 260)
                        {
                            point = 70 + ((spPoint - 200) / 6);
                        }
                        else if (spPoint <= 330)
                        {
                            point = 80 + ((spPoint - 260) / 7);
                        }
                        else if (spPoint <= 410)
                        {
                            point = 90 + ((spPoint - 330) / 8);
                        }

                        break;
                }
                return point;
            }
            catch
            {
                return 0;
            }
        }

        public static int SPPoint(short spLevel, short upgrade)
        {
            int point = (spLevel - 20) * 3;
            if (spLevel <= 20)
            {
                point = 0;
            }
            switch (upgrade)
            {
                case 1:
                    point += 5;
                    break;

                case 2:
                    point += 10;
                    break;

                case 3:
                    point += 15;
                    break;

                case 4:
                    point += 20;
                    break;

                case 5:
                    point += 28;
                    break;

                case 6:
                    point += 36;
                    break;

                case 7:
                    point += 46;
                    break;

                case 8:
                    point += 56;
                    break;

                case 9:
                    point += 68;
                    break;

                case 10:
                    point += 80;
                    break;

                case 11:
                    point += 95;
                    break;

                case 12:
                    point += 110;
                    break;

                case 13:
                    point += 128;
                    break;

                case 14:
                    point += 148;
                    break;

                case 15:
                    point += 173;
                    break;
            }

            if (upgrade > 15)
            {
                point += 173 + (25 + (5 * (upgrade - 15)));
            }

            return point;
        }

        internal static int Defence(ClassType @class, byte level)
        {
            if (_hitDef == null)
            {
                new CharacterHelper();
            }
            return _hitDef[(int)@class, level - 1];
        }

        internal static int DefenceRate(ClassType @class, byte level)
        {
            if (_hitDodge == null)
            {
                new CharacterHelper();
            }
            return _hitDodge[(int)@class, level - 1];
        }

        internal static int DistanceDefence(ClassType @class, byte level)
        {
            if (_distDef == null)
            {
                new CharacterHelper();
            }
            return _distDef[(int)@class, level - 1];
        }

        internal static int DistanceDefenceRate(ClassType @class, byte level)
        {
            if (_distDodge == null)
            {
                new CharacterHelper();
            }
            return _distDodge[(int)@class, level - 1];
        }

        internal static int DistanceRate(ClassType @class, byte level)
        {
            if (_distRate == null)
            {
                new CharacterHelper();
            }
            return _distRate[(int)@class, level - 1];
        }

        internal static int DistCritical(ClassType @class, byte level)
        {
            if (_criticalDist == null)
            {
                new CharacterHelper();
            }
            return _criticalDist[(int)@class, level - 1];
        }

        internal static int DistCriticalRate(ClassType @class, byte level)
        {
            if (_criticalDistRate == null)
            {
                new CharacterHelper();
            }
            return _criticalDistRate[(int)@class, level - 1];
        }

        internal static int HitCritical(ClassType @class, byte level)
        {
            if (_criticalHit == null)
            {
                new CharacterHelper();
            }
            return _criticalHit[(int)@class, level - 1];
        }

        internal static int HitCriticalRate(ClassType @class, byte level)
        {
            if (_criticalHitRate == null)
            {
                new CharacterHelper();
            }
            return _criticalHitRate[(int)@class, level - 1];
        }

        internal static int HitRate(ClassType @class, byte level)
        {
            if (_hitRate == null)
            {
                new CharacterHelper();
            }
            return _hitRate[(int)@class, level - 1];
        }

        private static void loadHeroXpData()
        {
            // Load SpData
            _heroXpData = new double[60];
            var index = 1;
            var increment = 118980;
            var increment2 = 9120;
            var increment3 = 360;

            _heroXpData[0] = 949560;
            _heroXpData[54] = 33224190;
            for (var lvl = 1; lvl < 60; lvl++)
            {
                if (lvl == 54)
                {
                    continue;
                }

                if (lvl > 54)
                {
                    _heroXpData[lvl] = (long)Math.Floor(_heroXpData[lvl - 1] * 1.15);
                    continue;
                }

                _heroXpData[lvl] = _heroXpData[lvl - 1] + increment;
                increment2 += increment3;
                increment += increment2;
                index++;
                if (index % 10 == 0)
                {
                    increment3 -= index / 10 < 3 ? index / 10 * 30 : 30;
                }
            }
        }

        private static void loadHPData()
        {
            _hp = new int[5, 99];

            foreach (ClassType classType in Enum.GetValues(typeof(ClassType)))
            {
                for (var i = 0; i < 99; i++)
                {
                    var hpx = i + 1 + Math.Floor(i * (double)(ClassConstants[(int)classType][0]) / 10);
                    var hp = (0.5 * Math.Pow(hpx, 2)) + (15.5 * hpx) + 205;
                    _hp[(int)classType, i] = (int)hp;
                }
            }
        }

        private static void loadHPHealth()
        {
            _hpHealth = new int[5];
            _hpHealth[(int)ClassType.Archer] = 60;
            _hpHealth[(int)ClassType.Adventurer] = 30;
            _hpHealth[(int)ClassType.Swordsman] = 90;
            _hpHealth[(int)ClassType.Magician] = 30;

            _hpHealth[(int)ClassType.MartialArtist] = 90;
        }

        private static void loadHPHealthStand()
        {
            _hpHealthStand = new int[5];
            _hpHealthStand[(int)ClassType.Archer] = 32;
            _hpHealthStand[(int)ClassType.Adventurer] = 25;
            _hpHealthStand[(int)ClassType.Swordsman] = 26;
            _hpHealthStand[(int)ClassType.Magician] = 20;

            _hpHealthStand[(int)ClassType.MartialArtist] = 26;
        }

        private static void LoadJobXPData()
        {
            _jobXpData = new double[5, 99];

            _jobXpData[(byte)ClassType.Adventurer, 0] = 2200;
            _jobXpData[(byte)ClassType.Archer, 0] = 14500;
            _jobXpData[(byte)ClassType.Magician, 0] = 14500;
            _jobXpData[(byte)ClassType.MartialArtist, 0] = 14500;
            _jobXpData[(byte)ClassType.Swordsman, 0] = 14500;

            for (var i = 1; i < 80; i++)
            {
                if (i < 20)
                {
                    _jobXpData[(byte)ClassType.Adventurer, i] = _jobXpData[(byte)ClassType.Adventurer, i - 1] + 700;
                }

                _jobXpData[(byte)ClassType.Archer, i] = _jobXpData[(byte)ClassType.Archer, i - 1] + (i > 39 ? 15000 : 4500);

                _jobXpData[(byte)ClassType.Magician, i] = _jobXpData[(byte)ClassType.Archer, i];
                _jobXpData[(byte)ClassType.MartialArtist, i] = _jobXpData[(byte)ClassType.Archer, i];
                _jobXpData[(byte)ClassType.Swordsman, i] = _jobXpData[(byte)ClassType.Archer, i];
            }
        }

        private static void loadMPData()
        {
            _mp = new int[5, 99];

            var adventurerMpAdd = 9;
            var magicianMpAdd = new[] { 9, 18, 21, 22, 25, 13, 27, 30, 31, 34 };
            var martialArtistMpAdd = new[] { 9, 9, 9, 10, 22, 11, 12, 13, 13, 27 };
            var archerMpAdd = new[] { 9, 9, 9, 10, 11, 11, 11, 12, 13, 26 };

            _mp[(byte)ClassType.Adventurer, 0] = 60;
            _mp[(byte)ClassType.Swordsman, 0] = 60;
            _mp[(byte)ClassType.Magician, 0] = 60;
            _mp[(byte)ClassType.MartialArtist, 0] = 60;
            _mp[(byte)ClassType.Archer, 0] = 60;
            var countMagician = 9;
            var countArcher = 11;
            var substractMagician = true;
            var substractArcher = false;
            var reverseArcher = 0;
            for (var i = 1; i < 99; i++)
            {
                adventurerMpAdd += i % 4 == 0 ? 2 : 0;
                _mp[(byte)ClassType.Adventurer, i] =
                    _mp[(byte)ClassType.Adventurer, i - 1] + (i % 4 == 0 ? adventurerMpAdd - 1 : adventurerMpAdd);
                _mp[(byte)ClassType.Swordsman, i] = _mp[(byte)ClassType.Adventurer, i];
                if (i - 1 > 9)
                {
                    var switcher = !substractMagician ? 1 : -1;
                    if ((i - 1) % 5 == 0)
                    {
                        countMagician += !substractMagician ? 1 : -1;
                        substractMagician = countMagician == 10 || countMagician == 8 ? !substractMagician : substractMagician;
                        magicianMpAdd[(i - 1) % 10] = magicianMpAdd[(i - 1) % 10] + countMagician;
                    }
                    else
                    {
                        magicianMpAdd[(i - 1) % 10] += 18 + switcher / ((i - 1) % 5 % 2 == 1 ? 1 : -1);
                    }

                    if (i % 10 == 0)
                    {
                        countArcher += !substractArcher ? 1 : -1;
                        substractArcher = countArcher == 12 || countArcher == 10 ? !substractArcher : substractArcher;
                        archerMpAdd[(i - 1) % 10] = archerMpAdd[(i - 1) % 10] + countArcher;
                        reverseArcher++;
                    }
                    else
                    {
                        var switcherArcher = reverseArcher % 4 == 2 || reverseArcher % 4 == 3 ? -1 : 1;
                        archerMpAdd[(i - 1) % 10] += (switcherArcher == -1 ? 6 : 5) +
                                                     (i % 20 < 10
                                                         ? i % 4 == 2 || i % 4 == 1 ? 0 : switcherArcher
                                                         : i % 4 == 0 || i % 4 == 1 ? switcherArcher : 0);

                    }

                    martialArtistMpAdd[(i - 1) % 10] += (i - 1) % 5 == 4 ? 12 : 6;
                }
                _mp[(byte)ClassType.Magician, i] =
                    _mp[(byte)ClassType.Magician, i - 1] + magicianMpAdd[(i - 1) % 10];

                _mp[(byte)ClassType.MartialArtist, i] =
                    _mp[(byte)ClassType.MartialArtist, i - 1] + martialArtistMpAdd[(i - 1) % 10];

                _mp[(byte)ClassType.Archer, i] =
                    _mp[(byte)ClassType.Archer, i - 1] + archerMpAdd[(i - 1) % 10];
            }
        }

        private static void loadMPHealth()
        {
            _mpHealth = new int[5];
            _mpHealth[(int)ClassType.Adventurer] = 10;
            _mpHealth[(int)ClassType.Swordsman] = 30;
            _mpHealth[(int)ClassType.Archer] = 50;
            _mpHealth[(int)ClassType.Magician] = 80;

            _mpHealth[(int)ClassType.MartialArtist] = 30;
        }

        private static void loadMPHealthStand()
        {
            _mpHealthStand = new int[5];
            _mpHealthStand[(int)ClassType.Adventurer] = 5;
            _mpHealthStand[(int)ClassType.Swordsman] = 16;
            _mpHealthStand[(int)ClassType.Archer] = 28;
            _mpHealthStand[(int)ClassType.Magician] = 40;

            _mpHealthStand[(int)ClassType.MartialArtist] = 16;
        }

        private static void loadSpeedData()
        {
            _speedData = new byte[5];
            _speedData[(int)ClassType.Adventurer] = 11;
            _speedData[(int)ClassType.Swordsman] = 11;
            _speedData[(int)ClassType.Archer] = 12;
            _speedData[(int)ClassType.Magician] = 10;

            _speedData[(int)ClassType.MartialArtist] = 11;
        }

        private static void loadSPXPData()
        {
            _spxpData = new double[2, 99];
            _spxpData[1, 0] = 10000;
            _spxpData[0, 0] = 15000;
            _spxpData[0, 19] = 218000;
            _spxpData[1, 19] = 100000;
            for (var i = 1; i < 19; i++)
            {
                _spxpData[0, i] = _spxpData[0, i - 1] + 10000;
                _spxpData[1, i] = _spxpData[1, i - 1];
            }
            for (var i = 20; i < _spxpData.GetLength(1); i++)
            {
                _spxpData[0, i] = _spxpData[0, i - 1] + 6 * (3 * i * (i + 1) + 1);
                _spxpData[1, i] = i switch
                {
                    37 => 304000,
                    47 => 672000,
                    _ => _spxpData[1, i - 1] + (i < 37 ? 5000 : i < 47 ? 8000 : 14000)
                };
            }
        }

        private static void LoadStats()
        {
            _minHit = new int[5, 99];
            _maxHit = new int[5, 99];
            _hitRate = new int[5, 99];
            _criticalHitRate = new int[5, 99];
            _criticalHit = new int[5, 99];
            _minDist = new int[5, 99];
            _maxDist = new int[5, 99];
            _distRate = new int[5, 99];
            _criticalDistRate = new int[5, 99];
            _criticalDist = new int[5, 99];
            _hitDef = new int[5, 99];
            _hitDodge = new int[5, 99];
            _distDef = new int[5, 99];
            _distDodge = new int[5, 99];
            _magicalDef = new int[5, 99];

            var adventurerDefence = 4;
            var swordmanDefence = 4;
            var archerDefence = 4;
            var mageDefence = 4;
            var fighterDefence = 4;

            var archerHitRate = 31;
            var fighterHitRate = 8;
            var swordHitRate = 23;

            var swordmanDodge = 8;
            var archerDodge = 18;
            var mageDodge = 18;
            var fighterDodge = 28;

            var swordmanDistDodge = 8;
            var archerDistDodge = 18;
            var mageDistDodge = 8;
            var fighterDistDodge = 18;

            var adventurerDistDefence = 4;
            var swordmanDistDefence = 4;
            var archerDistDefence = 4;
            var mageDistDefence = 24;
            var fighterDistDefence = 14;

            for (int i = 0; i < 99; i++)
            {
                // ADVENTURER
                _minHit[(int)ClassType.Adventurer, i] = i + 9; // approx
                _maxHit[(int)ClassType.Adventurer, i] = i + 9; // approx
                _hitRate[(int)ClassType.Adventurer, i] = i + 10;
                _criticalHitRate[(int)ClassType.Adventurer, i] = 0; // sure
                _criticalHit[(int)ClassType.Adventurer, i] = 0; // sure
                _minDist[(int)ClassType.Adventurer, i] = i + 9; // approx
                _maxDist[(int)ClassType.Adventurer, i] = i + 9; // approx
                _distRate[(int)ClassType.Adventurer, i] = (i + 9) * 2; // approx
                _criticalDistRate[(int)ClassType.Adventurer, i] = 0; // sure
                _criticalDist[(int)ClassType.Adventurer, i] = 0; // sure
                _hitDef[(int)ClassType.Adventurer, i] = i + (9 / 2); // approx
                _hitDodge[(int)ClassType.Adventurer, i] = i + 10;
                adventurerDistDefence += i % 2 == 0 ? 1 : 0;
                _distDef[(int)ClassType.Adventurer, i] = adventurerDistDefence;
                _distDodge[(int)ClassType.Adventurer, i] = i + 10;
                adventurerDefence += i % 2 == 0 ? 1 : 0;
                _magicalDef[(byte)ClassType.Adventurer, i] = adventurerDefence;

                // SWORDMAN
                _criticalHitRate[(int)ClassType.Swordsman, i] = 0; // approx
                _criticalHit[(int)ClassType.Swordsman, i] = 0; // approx
                _criticalDist[(int)ClassType.Swordsman, i] = 0; // approx
                _criticalDistRate[(int)ClassType.Swordsman, i] = 0; // approx
                _minDist[(int)ClassType.Swordsman, i] = i + 12; // approx
                _maxDist[(int)ClassType.Swordsman, i] = i + 12; // approx
                _distRate[(int)ClassType.Swordsman, i] = 2 * (i + 12); // approx
                swordmanDodge += (i - 5) % 5 == 0 ? 2 : 1;
                _hitDodge[(int)ClassType.Swordsman, i] = swordmanDodge;
                swordmanDistDodge += (i - 5) % 5 == 0 ? 2 : 1;
                _distDodge[(int)ClassType.Swordsman, i] = swordmanDistDodge;
                swordmanDefence += (i % 2 == 0) ? 1 : 0;
                _magicalDef[(byte)ClassType.Swordsman, i] = swordmanDefence;
                swordHitRate += (i - 5) % 5 == 0 ? 2 : 1;
                _hitRate[(int)ClassType.Swordsman, i] = swordHitRate;
                _hitDef[(int)ClassType.Swordsman, i] = i + 2; // approx

                _minHit[(int)ClassType.Swordsman, i] = (2 * i) + 5; // approx Numbers n such that 10n+9 is prime.
                _maxHit[(int)ClassType.Swordsman, i] = (2 * i) + 5; // approx Numbers n such that 10n+9 is prime.
                swordmanDistDefence += i == 0 || (i - 2) % 10 == 0 || (i - 4) % 10 == 0 || (i - 5) % 5 == 0 || (i - 7) % 10 == 0 || (i - 9) % 10 == 0 ? 1 : 0;
                _distDef[(int)ClassType.Swordsman, i] = swordmanDistDefence;

                // MAGICIAN
                _hitRate[(int)ClassType.Magician, i] = 0;
                _criticalHitRate[(int)ClassType.Magician, i] = 0; // sure
                _criticalHit[(int)ClassType.Magician, i] = 0; // sure
                _criticalDistRate[(int)ClassType.Magician, i] = 0; // sure
                _criticalDist[(int)ClassType.Magician, i] = 0; // sure

                _minDist[(int)ClassType.Magician, i] = 14 + i; // approx
                _maxDist[(int)ClassType.Magician, i] = 14 + i; // approx
                _distRate[(int)ClassType.Magician, i] = (14 + i) * 2; // approx
                _hitDef[(int)ClassType.Magician, i] = (i + 11) / 2; // approx
                mageDefence += (i % 2 == 0 || (i - 3) % 10 == 0 || (i - 5) % 5 == 0 || (i - 7) % 10 == 0 || (i - 9) % 10 == 0) ? 1 : 0;
                _magicalDef[(int)ClassType.Magician, i] = mageDefence;
                mageDodge += (i - 5) % 5 == 0 ? 2 : 1;
                _hitDodge[(int)ClassType.Magician, i] = mageDodge;
                mageDistDodge += (i - 5) % 5 == 0 ? 2 : 1;
                _distDodge[(int)ClassType.Magician, i] = mageDistDodge;

                _minHit[(int)ClassType.Magician, i] = (2 * i) + 9; // approx Numbers n such that n^2 is of form x^ 2 + 40y ^ 2 with positive x,y.
                _maxHit[(int)ClassType.Magician, i] = (2 * i) + 9; // approx Numbers n such that n^2 is of form x^2+40y^2 with positive x,y.
                mageDistDefence += i == 0 || (i - 2) % 10 == 0 || (i - 4) % 10 == 0 || (i - 5) % 5 == 0 || (i - 7) % 10 == 0 || (i - 9) % 10 == 0 ? 1 : 0;
                _distDef[(int)ClassType.Magician, i] = mageDistDefence;

                // ARCHER
                _criticalHitRate[(int)ClassType.Archer, i] = 0; // sure
                _criticalHit[(int)ClassType.Archer, i] = 0; // sure
                _criticalDistRate[(int)ClassType.Archer, i] = 0; // sure
                _criticalDist[(int)ClassType.Archer, i] = 0; // sure

                _minHit[(int)ClassType.Archer, i] = 9 + (i * 3); // approx
                _maxHit[(int)ClassType.Archer, i] = 9 + (i * 3); // approx
                archerHitRate += i != 96 && i % 2 == 0 || i > 0 && i % 5 == 0 ? 4 : 2;
                _hitRate[(int)ClassType.Archer, i] = archerHitRate; // approx
                _minDist[(int)ClassType.Archer, i] = 2 * i; // approx
                _maxDist[(int)ClassType.Archer, i] = 2 * i; // approx

                _distRate[(int)ClassType.Archer, i] = 20 + (2 * i); // approx
                _hitDef[(int)ClassType.Archer, i] = i; // approx
                bool plus = i > 10 && i < 20 || i > 30 && i < 40 || i > 50 && i < 60 || i > 70 && i < 80 || i > 90 && i < 99;
                archerDefence += plus ? ((i + 1) % 2 == 0 ? 1 : 0) : i % 2 == 0 ? 1 : 0;
                _magicalDef[(int)ClassType.Archer, i] = archerDefence;
                archerDodge += ((i - 2) % 10 == 0 || (i - 4) % 10 == 0 || (i - 5) % 5 == 0 || (i - 7) % 10 == 0 || (i - 9) % 10 == 0 || (i - 10) % 10 == 0) ? 2 : 1;
                _hitDodge[(int)ClassType.Archer, i] = archerDodge;
                archerDistDodge += ((i - 2) % 10 == 0 || (i - 4) % 10 == 0 || (i - 5) % 5 == 0 || (i - 7) % 10 == 0 || (i - 9) % 10 == 0 || (i - 10) % 10 == 0) ? 2 : 1;
                _distDodge[(int)ClassType.Archer, i] = archerDistDodge;
                archerDistDefence += i == 0 || (i - 2) % 10 == 0 || (i - 3) % 10 == 0 || (i - 4) % 10 == 0 || (i - 5) % 5 == 0 || (i - 7) % 10 == 0 || (i - 8) % 10 == 0 || (i - 9) % 10 == 0 ? 1 : 0;
                _distDef[(int)ClassType.Archer, i] = archerDistDefence;

                // MARTIAL ARTIST
                _criticalHitRate[(int)ClassType.MartialArtist, i] = 0; // approx
                _criticalHit[(int)ClassType.MartialArtist, i] = 0; // approx
                _criticalDist[(int)ClassType.MartialArtist, i] = 0; // approx
                _criticalDistRate[(int)ClassType.MartialArtist, i] = 0; // approx
                _minDist[(int)ClassType.MartialArtist, i] = i + 12; // approx
                _maxDist[(int)ClassType.MartialArtist, i] = i + 12; // approx
                _distRate[(int)ClassType.MartialArtist, i] = 2 * (i + 12); // approx
                fighterDodge += ((i - 4) % 10 == 0 || (i - 7) % 10 == 0 || (i - 10) % 10 == 0) ? 2 : 1;
                _hitDodge[(int)ClassType.MartialArtist, i] = fighterDodge;
                fighterDistDodge += ((i - 4) % 10 == 0 || (i - 7) % 10 == 0 || (i - 10) % 10 == 0) ? 2 : 1;
                _distDodge[(int)ClassType.MartialArtist, i] = fighterDistDodge;
                fighterDefence += ((i - 2) % 10 == 0 || (i - 4) % 10 == 0 || (i - 5) % 5 == 0 || (i - 7) % 10 == 0 || (i - 9) % 10 == 0) ? 1 : 0;
                _magicalDef[(int)ClassType.MartialArtist, i] = fighterDefence;
                fighterHitRate += i == 0 || (i - 4) % 10 == 0 || i > 0 && (i - 7) % 10 == 0 || i > 0 && (i - 10) % 10 == 0 ? 2 : 1;
                _hitRate[(int)ClassType.MartialArtist, i] = fighterHitRate;
                _hitDef[(int)ClassType.MartialArtist, i] = i + 2; // approx

                _minHit[(int)ClassType.MartialArtist, i] = (2 * i) + 5; // approx Numbers n such that 10n+9 is prime.
                _maxHit[(int)ClassType.MartialArtist, i] = (2 * i) + 5; // approx Numbers n such that 10n+9 is prime.
                fighterDistDefence += i > 0 && ((i - 1) % 20 == 0 || (i - 3) % 20 == 0 || (i - 6) % 20 == 0 || (i - 9) % 20 == 0 || (i - 12) % 20 == 0 || (i - 15) % 20 == 0 || (i - 18) % 20 == 0) ? 0 : 1;
                _distDef[(int)ClassType.MartialArtist, i] = fighterDistDefence;
            }
        }

        private static void loadXPData()
        {
            // Load XpData
            _xpData = new double[99];
            var v = new long[99];
            double var = 1;
            v[0] = 540;
            v[1] = 960;
            _xpData[0] = 300;

            for (var i = 1; i < _xpData.Length; i++)
            {
                v[i] = i == 1 ? v[i] : v[i - 1] + 420 + 120 * (i - 1);
                var = i switch
                {
                    14 => 6 / 3d,
                    39 => 19 / 3d,
                    59 => 70 / 3d,
                    79 => 5000,
                    82 => 9000,
                    84 => 13000,
                    96 => 15000,
                    _ => var
                };

                _xpData[i] = i < 79
                    ? Convert.ToInt64(_xpData[i - 1] + var * v[i - 1])
                    : Convert.ToInt64(_xpData[i - 1] + var * (i + 2) * (i + 2));
            }
        }

        #endregion
    }
}