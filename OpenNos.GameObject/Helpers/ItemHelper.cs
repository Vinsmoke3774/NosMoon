namespace OpenNos.GameObject.Helpers
{
    public class ItemHelper
    {
        #region Members

        public static readonly byte[] BuyCraftRareRate = new byte[] { 100, 100, 63, 48, 35, 24, 14, 6 };
        public static readonly byte[] ItemUpgradeFailRate = new byte[] { 0, 0, 0, 5, 20, 40, 60, 70, 80, 85 };
        public static readonly byte[] ItemUpgradeFixRate = new byte[] { 0, 0, 10, 15, 20, 20, 20, 20, 15, 14 };
        public static readonly byte[] R8ItemUpgradeFailRate = new byte[] { 50, 40, 60, 50, 60, 70, 75, 77, 83, 89 };
        public static readonly byte[] R8ItemUpgradeFixRate = new byte[] { 50, 40, 70, 65, 80, 90, 95, 97, 98, 99 };
        public static readonly byte[] RareRate = new byte[] { 100, 80, 70, 50, 30, 15, 5, 1 };
        public static readonly byte[] RarifyRate = new byte[] { 80, 70, 60, 40, 30, 15, 10, 5, 3, 2, 1 };
        public static readonly byte[] OlorunFailRate = new byte[] { 0, 0, 0, 0, 40, 40, 45, 85, 85, 89 };
        public static readonly byte[] OlorunFixRate = new byte[] { 20, 20, 20, 30, 25, 35, 35, 10, 14, 10 };

        public static readonly byte[] SpDestroyRate = new byte[] { 0, 0, 5, 10, 15, 20, 25, 30, 35, 40, 45, 50, 55, 60, 70 };
        public static readonly byte[] SpUpFailRate = new byte[] { 20, 25, 30, 40, 50, 60, 65, 70, 75, 80, 90, 93, 95, 97, 99 };

        public static readonly short[] PassiveEventItems =  {5144, 5011, 5142};

        private static PassiveSkillHelper _instance;

        #endregion

        #region Properties

        public static PassiveSkillHelper Instance => _instance ?? (_instance = new PassiveSkillHelper());

        #endregion
    }
}