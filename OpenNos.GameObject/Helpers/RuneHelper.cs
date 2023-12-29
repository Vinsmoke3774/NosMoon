using OpenNos.Data;
using System.Collections.Generic;

namespace OpenNos.GameObject.Helpers
{
    public class RuneHelper
    {
        #region Members

        private static RuneHelper _instance;

        #endregion

        #region Properties

        public static RuneHelper Instance => _instance ?? (_instance = new RuneHelper());

        #endregion

        #region Methods

        public List<BCard> RuneItemToBCards(IEnumerable<ShellEffectDTO> runes)
        {
            var bcards = new List<BCard>();

            if (runes == null)
            {
                return bcards;
            }

            foreach (var rune in runes)
            {
                bcards.Add(new BCard
                {
                    Type = rune.Effect,
                    FirstData = rune.Value,
                    SubType = (byte)(10 * (1 + (byte)rune.EffectLevel) + 1)
                });
            }

            return bcards;
        }

        #endregion
    }
}