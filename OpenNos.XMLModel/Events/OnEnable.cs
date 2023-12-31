﻿using System;
using System.Xml.Serialization;

namespace OpenNos.XMLModel.Events
{
    [Serializable]
    public class OnEnable
    {
        #region Properties

        [XmlElement]
        public NpcDialog[] NpcDialog { get; set; }

        [XmlElement]
        public Teleport Teleport { get; set; }

        #endregion
    }
}