using System.Collections.Generic;
using NosTale.Configuration.Configuration.Item;

namespace NosTale.Configuration
{
    public class GameConfiguration
    {
        #region Properties

        public static RemoveRuneConfiguration RRemove { get; set; } = new RemoveRuneConfiguration
        {
            ItemVnum = 5812
        };

        public static UpgradeRuneConfiguration RUpgrade { get; set; } = new UpgradeRuneConfiguration
        {
            GoldPrice = new[]
            {
                3000, 16000, 99000, 55000, 110000, 280000, 220000, 310000, 500000, 450000, 560000, 790000, 700000,
                880000, 1100000
            },
            PercentSucess = new[] { 100, 90, 75, 65, 45, 25, 20, 15, 7, 7, 5, 1, 3, 2, 0.1 },
            PercentFail = new[] { 0, 10, 17, 30, 49, 68, 72, 76, 83, 81, 81, 83, 79, 78, 74.9 },
            PercentBreaked = new double[] { 0, 0, 4, 5, 6, 7, 8, 9, 10, 12, 14, 16, 18, 20, 25 },

            // 2416 2411 2430 2475 2413 2462
            Item = new[]
            {
                new List<RequiredItem>
                {
                    new RequiredItem
                    {
                        Id = 2416,
                        Quantity = 10
                    },
                    new RequiredItem
                    {
                        Id = 2411,
                        Quantity = 5
                    }
                },
                new List<RequiredItem>
                {
                    new RequiredItem
                    {
                        Id = 2416,
                        Quantity = 12
                    },
                    new RequiredItem
                    {
                        Id = 2411,
                        Quantity = 7
                    }
                },
                new List<RequiredItem>
                {
                    new RequiredItem
                    {
                        Id = 2416,
                        Quantity = 16
                    },
                    new RequiredItem
                    {
                        Id = 2411,
                        Quantity = 11
                    },
                    new RequiredItem
                    {
                        Id = 2430,
                        Quantity = 10
                    },
                    new RequiredItem
                    {
                        Id = 2475,
                        Quantity = 1
                    }
                },
                new List<RequiredItem>
                {
                    new RequiredItem
                    {
                        Id = 2416,
                        Quantity = 14
                    },
                    new RequiredItem
                    {
                        Id = 2411,
                        Quantity = 9
                    }
                },
                new List<RequiredItem>
                {
                    new RequiredItem
                    {
                        Id = 2416,
                        Quantity = 16
                    },
                    new RequiredItem
                    {
                        Id = 2411,
                        Quantity = 11
                    }
                },
                new List<RequiredItem>
                {
                    new RequiredItem
                    {
                        Id = 2416,
                        Quantity = 20
                    },
                    new RequiredItem
                    {
                        Id = 2411,
                        Quantity = 15
                    },
                    new RequiredItem
                    {
                        Id = 2430,
                        Quantity = 15
                    },
                    new RequiredItem
                    {
                        Id = 2475,
                        Quantity = 2
                    }
                },
                new List<RequiredItem>
                {
                    new RequiredItem
                    {
                        Id = 2416,
                        Quantity = 18
                    },
                    new RequiredItem
                    {
                        Id = 2411,
                        Quantity = 13
                    }
                },
                new List<RequiredItem>
                {
                    new RequiredItem
                    {
                        Id = 2416,
                        Quantity = 20
                    },
                    new RequiredItem
                    {
                        Id = 2411,
                        Quantity = 15
                    }
                },
                new List<RequiredItem>
                {
                    new RequiredItem
                    {
                        Id = 2416,
                        Quantity = 50
                    },
                    new RequiredItem
                    {
                        Id = 2411,
                        Quantity = 40
                    },
                    new RequiredItem
                    {
                        Id = 2430,
                        Quantity = 40
                    },
                    new RequiredItem
                    {
                        Id = 2475,
                        Quantity = 1
                    },
                    new RequiredItem
                    {
                        Id = 2413,
                        Quantity = 1
                    }
                },
                new List<RequiredItem>
                {
                    new RequiredItem
                    {
                        Id = 2416,
                        Quantity = 44
                    },
                    new RequiredItem
                    {
                        Id = 2411,
                        Quantity = 34
                    }
                },
                new List<RequiredItem>
                {
                    new RequiredItem
                    {
                        Id = 2416,
                        Quantity = 48
                    },
                    new RequiredItem
                    {
                        Id = 2411,
                        Quantity = 38
                    }
                },
                new List<RequiredItem>
                {
                    new RequiredItem
                    {
                        Id = 2416,
                        Quantity = 60
                    },
                    new RequiredItem
                    {
                        Id = 2411,
                        Quantity = 50
                    },
                    new RequiredItem
                    {
                        Id = 2430,
                        Quantity = 50
                    },
                    new RequiredItem
                    {
                        Id = 2413,
                        Quantity = 1
                    },
                    new RequiredItem
                    {
                        Id = 2462,
                        Quantity = 1
                    }
                },
                new List<RequiredItem>
                {
                    new RequiredItem
                    {
                        Id = 2416,
                        Quantity = 52
                    },
                    new RequiredItem
                    {
                        Id = 2411,
                        Quantity = 42
                    }
                },
                new List<RequiredItem>
                {
                    new RequiredItem
                    {
                        Id = 2416,
                        Quantity = 56
                    },
                    new RequiredItem
                    {
                        Id = 2411,
                        Quantity = 46
                    }
                },
                new List<RequiredItem>
                {
                    new RequiredItem
                    {
                        Id = 2416,
                        Quantity = 80
                    },
                    new RequiredItem
                    {
                        Id = 2411,
                        Quantity = 60
                    },
                    new RequiredItem
                    {
                        Id = 2430,
                        Quantity = 60
                    },
                    new RequiredItem
                    {
                        Id = 2413,
                        Quantity = 2
                    },
                    new RequiredItem
                    {
                        Id = 2462,
                        Quantity = 2
                    }
                }
            }
        };

        public static RemoveTattooConfiguration TRemove { get; set; } = new RemoveTattooConfiguration
        {
            ItemVnum = 5799
        };

        public static UpgradeTattooConfiguration TUpgrade { get; set; } = new UpgradeTattooConfiguration
        {
            GoldPrice = new[] { 30000, 67000, 140000, 230000, 380000, 540000, 770000, 960000, 1200000 },
            PercentSucess = new[] { 80, 60, 50, 35, 20, 10, 5, 2, 1 },
            PercentFail = new[] { 20, 30, 35, 40, 50, 55, 45, 28, 9 },
            PercentDestroyed = new[] { 0, 10, 15, 25, 30, 35, 50, 70, 90 },
            Item = new[]
            {
                new List<RequiredItem>
                {
                    new RequiredItem
                    {
                        Id = 2411,
                        Quantity = 17
                    },
                    new RequiredItem
                    {
                        Id = 2416,
                        Quantity = 7
                    },
                    new RequiredItem
                    {
                        Id = 2408,
                        Quantity = 6
                    }
                },
                new List<RequiredItem>
                {
                    new RequiredItem
                    {
                        Id = 2411,
                        Quantity = 19
                    },
                    new RequiredItem
                    {
                        Id = 2416,
                        Quantity = 10
                    },
                    new RequiredItem
                    {
                        Id = 2408,
                        Quantity = 7
                    }
                },
                new List<RequiredItem>
                {
                    new RequiredItem
                    {
                        Id = 2411,
                        Quantity = 21
                    },
                    new RequiredItem
                    {
                        Id = 2416,
                        Quantity = 13
                    },
                    new RequiredItem
                    {
                        Id = 2408,
                        Quantity = 8
                    }
                },
                new List<RequiredItem>
                {
                    new RequiredItem
                    {
                        Id = 2411,
                        Quantity = 25
                    },
                    new RequiredItem
                    {
                        Id = 2416,
                        Quantity = 16
                    },
                    new RequiredItem
                    {
                        Id = 2408,
                        Quantity = 9
                    },
                    new RequiredItem
                    {
                        Id = 2410,
                        Quantity = 15
                    }
                },
                new List<RequiredItem>
                {
                    new RequiredItem
                    {
                        Id = 2411,
                        Quantity = 30
                    },
                    new RequiredItem
                    {
                        Id = 2416,
                        Quantity = 20
                    },
                    new RequiredItem
                    {
                        Id = 2408,
                        Quantity = 10
                    },
                    new RequiredItem
                    {
                        Id = 2410,
                        Quantity = 20
                    }
                },
                new List<RequiredItem>
                {
                    new RequiredItem
                    {
                        Id = 2411,
                        Quantity = 35
                    },
                    new RequiredItem
                    {
                        Id = 2416,
                        Quantity = 25
                    },
                    new RequiredItem
                    {
                        Id = 2408,
                        Quantity = 12
                    },
                    new RequiredItem
                    {
                        Id = 2410,
                        Quantity = 25
                    },
                    new RequiredItem
                    {
                        Id = 2474,
                        Quantity = 3
                    }
                },
                new List<RequiredItem>
                {
                    new RequiredItem
                    {
                        Id = 2411,
                        Quantity = 60
                    },
                    new RequiredItem
                    {
                        Id = 2416,
                        Quantity = 30
                    },
                    new RequiredItem
                    {
                        Id = 2408,
                        Quantity = 20
                    },
                    new RequiredItem
                    {
                        Id = 2410,
                        Quantity = 20
                    },
                    new RequiredItem
                    {
                        Id = 2474,
                        Quantity = 4
                    }
                },
                new List<RequiredItem>
                {
                    new RequiredItem
                    {
                        Id = 2411,
                        Quantity = 80
                    },
                    new RequiredItem
                    {
                        Id = 2416,
                        Quantity = 40
                    },
                    new RequiredItem
                    {
                        Id = 2408,
                        Quantity = 30
                    },
                    new RequiredItem
                    {
                        Id = 2410,
                        Quantity = 25
                    },
                    new RequiredItem
                    {
                        Id = 2412,
                        Quantity = 3
                    }
                },
                new List<RequiredItem>
                {
                    new RequiredItem
                    {
                        Id = 2411,
                        Quantity = 100
                    },
                    new RequiredItem
                    {
                        Id = 2416,
                        Quantity = 80
                    },
                    new RequiredItem
                    {
                        Id = 2408,
                        Quantity = 40
                    },
                    new RequiredItem
                    {
                        Id = 2410,
                        Quantity = 40
                    },
                    new RequiredItem
                    {
                        Id = 2412,
                        Quantity = 4
                    }
                }
            }
        };

        #endregion
    }
}