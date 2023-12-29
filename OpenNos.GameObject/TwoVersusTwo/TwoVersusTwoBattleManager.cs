using OpenNos.Domain;

namespace OpenNos.GameObject.TwoVersusTwo
{
    public class TwoVersusTwoBattleManager
    {
        #region Methods

        public static bool AreNotInMap(ClientSession ses)
        {
            if (ses == null)
            {
                return false;
            }

            if (ses.CurrentMapInstance.MapInstanceType != MapInstanceType.TwoVersusTwo)
            {
                return true;
            }

            return false;
        }

        #endregion
    }
}