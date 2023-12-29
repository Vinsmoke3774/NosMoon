﻿using System;
using System.Xml.Serialization;

namespace OpenNos.XMLModel.Events
{
    [Serializable]
    public class UseSkillOnDamage
    {
        #region Properties

        [XmlAttribute]
        public byte HpPercent { get; set; }

        [XmlAttribute]
        public short SkillVNum { get; set; }

        #endregion
    }
}