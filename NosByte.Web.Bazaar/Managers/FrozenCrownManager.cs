using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NosByte.Shared.ApiModels;

namespace NosByte.Web.Bazaar.Managers
{
    public class FrozenCrownManager
    {
        public FrozenCrownManager()
        {
            Model = new GetPercentModel();
        }

        public GetPercentModel Model { get; set; }
    }
}
