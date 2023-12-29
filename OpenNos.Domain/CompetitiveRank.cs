using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenNos.Domain
{
    public class CompetitiveRank
    {
        private CompetitiveRank(int diamondWin, int diamondLoss, int leavePenalty, CompetitiveRankType type)
            => (DiamondWin, DiamondLoss, LeavePenalty, _type, _subRank)
            = (diamondWin, diamondLoss, leavePenalty, type, -1);

        private CompetitiveRank(CompetitiveRank rank, int subRank) : this(rank.DiamondWin, rank.DiamondLoss, rank.LeavePenalty, rank._type)
        {
            _subRank = subRank;
        }

        private static Dictionary<CompetitiveRankType, CompetitiveRank> _competitiveRanks = new()
        {
            { CompetitiveRankType.BRONZE, new(400, 50, 0, CompetitiveRankType.BRONZE) },
            { CompetitiveRankType.SILVER, new(250, 50, 500, CompetitiveRankType.SILVER) },
            { CompetitiveRankType.GOLD, new(130, 30, 260, CompetitiveRankType.GOLD) },
            { CompetitiveRankType.PLATINUM, new(80, 25, 160, CompetitiveRankType.PLATINUM) },
            { CompetitiveRankType.DIAMOND, new(40, 15, 80, CompetitiveRankType.DIAMOND) },
            { CompetitiveRankType.MASTER, new(85, 40, 170, CompetitiveRankType.MASTER) },
            { CompetitiveRankType.NOSMOON, new(85, 40, 170, CompetitiveRankType.NOSMOON) }
        };

        public static CompetitiveRank GetRankFromRp(int rp) // TODO: Manage NosMoon rank
        {
            var maxEnumVal = (int)Enum.GetValues(typeof(CompetitiveRankType)).Cast<CompetitiveRankType>().Max();
            var rank = rp / 2000; // Need 2000 RP to reach next rank
            if ((CompetitiveRankType)rank == CompetitiveRankType.NOSMOON)
            {
                return new(_competitiveRanks[CompetitiveRankType.MASTER], 1);
            }
            var subRank = 4 - (maxEnumVal % 2000) / 4; // Get value between 0 and 2000, we then have 4 categories
            return new(_competitiveRanks[(CompetitiveRankType)rank], subRank);
        }

        public static int Win(int rp)
        {
            return rp + GetRankFromRp(rp).DiamondWin;
        }

        public static int Loose(int rp)
        {
            var info = GetRankFromRp(rp);
            var tier = rp / 2000;
            var finalRp = rp - info.DiamondLoss;
            if (tier != (finalRp / 2000)) // Can't demote...
            {
                return tier * 2000; // ...So we get lowest RP possible
            }
            return finalRp;
        }

        public int DiamondWin { get; }
        public int DiamondLoss { get; }
        public int LeavePenalty { get; }

        private readonly CompetitiveRankType _type;
        private readonly int _subRank;

        public override string ToString()
        {
            var t = _type.ToString();
            return $"{t[0]}{t.Substring(1).ToLowerInvariant()} {_subRank}".Replace(' ', '_');
        }

        public bool CanCompeteAgainst(CompetitiveRank r)
        {
            return (_type <= CompetitiveRankType.GOLD && r._type <= CompetitiveRankType.GOLD)
                || ((_type == CompetitiveRankType.PLATINUM || _type == CompetitiveRankType.DIAMOND) && (r._type == CompetitiveRankType.PLATINUM || r._type == CompetitiveRankType.DIAMOND))
                || (_type >= CompetitiveRankType.DIAMOND && r._type >= CompetitiveRankType.DIAMOND);
        }

        private enum CompetitiveRankType
        {
            BRONZE,
            SILVER,
            GOLD,
            PLATINUM,
            DIAMOND,
            MASTER,
            NOSMOON
        }
    }
}
