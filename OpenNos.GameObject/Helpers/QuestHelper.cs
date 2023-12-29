﻿using System.Collections.Generic;

namespace OpenNos.GameObject.Helpers
{
    public class QuestHelper
    {
        #region Members

        private static QuestHelper _instance;

        #endregion

        #region Instantiation

        public QuestHelper()
        {
            LoadSkipQuests();
        }

        #endregion

        #region Properties

        public static QuestHelper Instance
        {
            get { return _instance ?? (_instance = new QuestHelper()); }
        }

        public List<int> SkipQuests { get; set; }

        #endregion

        #region Methods

        public void LoadSkipQuests()
        {
            SkipQuests = new List<int>();
            SkipQuests.AddRange(new List<int> { 1676, 1677, 1698, 1714, 1715, 1719, 2123, 2079, 3014, 2084, 3019 });
        }

        #endregion
    }
}