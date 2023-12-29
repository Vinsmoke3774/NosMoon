using OpenNos.GameObject.Algorithms.Geography;
using System;

namespace OpenNos.GameObject.Algorithms
{
    public static class Heuristic
    {
        public static readonly Func<Tile, Tile, float> _heuristic = (from, to) =>
            (Math.Abs(from.X - to.X) + Math.Abs(from.Y - to.Y) + Math.Abs(from.Z - to.Z)) * 1.001f;

        public static double GetDistance((short X, short Y) fromValuedCell, (short X, short Y) toValuedCell)
        {
            var iDx = Math.Abs(fromValuedCell.X - toValuedCell.X);
            var iDy = Math.Abs(fromValuedCell.Y - toValuedCell.Y);
            var min = Math.Min(iDx, iDy);
            var max = Math.Max(iDx, iDy);
            return min * Math.Sqrt(2) + max - min;
        }

    }
}