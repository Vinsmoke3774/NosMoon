using System;
using System.Collections.Generic;
using System.Text;
using OpenNos.Domain;

namespace NosByte.Shared.ApiModels
{
    public class FcPercentModel
    {
        public FactionType Faction { get; set; }

        public int Percentage { get; set; }
    }
}
