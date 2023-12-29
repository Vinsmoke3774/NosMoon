﻿using OpenNos.Domain;
using System;
using System.Collections.Generic;

namespace OpenNos.GameObject.RainbowBattle
{
    public class RainbowBattleTeam
    {
        #region Instantiation

        public RainbowBattleTeam(List<ClientSession> session, RainbowTeamBattleType RmbTeamType, RainbowBattleTeam secondTeam)
        {
            Session = session;
            TeamEntity = RmbTeamType;
            SecondTeam = secondTeam;
            TotalFlag = new List<Tuple<int, RainbowNpcType>>();
        }

        #endregion

        #region Properties

        public long Score { get; set; }

        public long Check { get; set; }

        public RainbowBattleTeam SecondTeam { get; set; }

        public List<ClientSession> Session { get; set; }

        public RainbowTeamBattleType TeamEntity { get; set; }

        public List<Tuple<int, RainbowNpcType>> TotalFlag { get; set; }

        #endregion
    }
}