﻿using System;
using System.Xml.Serialization;

namespace OpenNos.XMLModel.Events
{
    [Serializable]
    public class Effect
    {
        #region Properties

        [XmlAttribute]
        public int Delay { get; set; }

        [XmlAttribute]
        public short Value { get; set; }

        #endregion
    }
}