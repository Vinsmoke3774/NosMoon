namespace OpenNos.GameObject.Helpers
{
    public class RewardsHelper
    {
        #region Methods

        public static int ArenaXpReward(byte characterLevel)
        {
            if (characterLevel <= 40)
            {
                // 3%
                return (int)(CharacterHelper.HeroXpData[characterLevel - 1] / 33);
            }

            if (characterLevel <= 50)
            {
                // 2%
                return (int)(CharacterHelper.HeroXpData[characterLevel - 1] / 50);
            }

            if (characterLevel <= 60)
            {
                // 1%
                return (int)(CharacterHelper.HeroXpData[characterLevel - 1] / 100);
            }

            return 0;
        }

        #endregion
    }
}