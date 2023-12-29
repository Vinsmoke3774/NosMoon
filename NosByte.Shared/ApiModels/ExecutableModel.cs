using System;
using System.Collections.Generic;
using System.Text;
using OpenNos.Domain;

namespace NosByte.Shared.ApiModels
{
    public class ExecutableModel
    {
        public int Pid { get; set; }

        public ExecutableType Type { get; set; }
    }
}
