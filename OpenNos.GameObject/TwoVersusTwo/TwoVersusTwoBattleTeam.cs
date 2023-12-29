using OpenNos.Domain;
using System.Collections.Generic;

namespace OpenNos.GameObject.TwoVersusTwo
{
    public class TwoVersusTwoBattleTeam
    {
        #region Instantiation

        public TwoVersusTwoBattleTeam(List<ClientSession> session, TwoVersusTwoTeamType TvTTeamType, TwoVersusTwoBattleTeam secondTeam)
        {
            Session = session;
            TeamEntity = TvTTeamType;
            SecondTeam = secondTeam;
        }

        #endregion

        #region Properties

        public TwoVersusTwoBattleTeam SecondTeam { get; set; }

        public List<ClientSession> Session { get; set; }

        public TwoVersusTwoTeamType TeamEntity { get; set; }

        #endregion
    }
}