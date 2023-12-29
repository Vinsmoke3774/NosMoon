using System;

namespace OpenNos.GameObject.Extension
{
    public static class MathsExtensions
    {
        public static double CalculateAngle(short startX, short startY, short arrivalX, short arrivalY)
        {
            return Math.Atan2(startY - arrivalY, startX - arrivalX);
        }
    }
}
