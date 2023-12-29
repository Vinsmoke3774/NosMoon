using System;
using System.Collections.Generic;
using System.Text;
using OpenNos.Domain;

namespace NosByte.Shared.ApiModels
{
    public class UserDataModel
    {
        public long CharacterId { get; set; }

        public string Name { get; set; }

        public short LevelSum { get; set; }

        public EventType EventType { get; set; }
    }
}
