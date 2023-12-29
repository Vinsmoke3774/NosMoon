using System;
using System.Xml.Serialization;

namespace OpenNos.XMLModel.Objects
{
    [Serializable]
    public class FamGold
    {
        #region Properties

        [XmlAttribute]
        public long Value { get; set; }

        #endregion
    }
}