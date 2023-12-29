using Microsoft.AspNetCore.Mvc;
using NosByte.Shared.ApiModels;
using NosByte.Web.Bazaar.Managers;
using OpenNos.Core.Logger;
using OpenNos.Domain;

namespace NosByte.Web.Bazaar.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FrozenCrownController
    {
        private readonly FrozenCrownManager _fcManager;

        public FrozenCrownController(FrozenCrownManager manager)
        {
            _fcManager = manager;
        }

        [HttpPost]
        public bool SetPercent([FromBody] FcPercentModel model)
        {
            if (model == null)
            {
                return false;
            }

            Logger.Log.Debug($"Setting Percentage for faction: {model.Faction}");

            switch (model.Faction)
            {
                case FactionType.Angel:
                    _fcManager.Model.AngelPercent = model.Percentage;
                    break;
                case FactionType.Demon:
                    _fcManager.Model.DemonPercent = model.Percentage;
                    break;
            }

            return true;
        }

        [HttpGet]
        public GetPercentModel GetPercent(FactionType faction)
        {
            Logger.Log.Debug("Getting FC percentage.");
            return _fcManager.Model;
        }
    }
}
