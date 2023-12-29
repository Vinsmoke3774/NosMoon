using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using NosByte.Shared.ApiModels;
using NosByte.Web.Bazaar.Managers;
using OpenNos.Core;
using OpenNos.Domain;

namespace NosByte.Web.Bazaar.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class UserController
    {
        private readonly UserManager _userManager;

        public UserController(UserManager userManager)
        {
            _userManager = userManager;
        }

        [HttpGet("GetEvent/{id}")]
        public UserDataModel GetEvent(long id)
        {
            return _userManager.PlayerEvents.FirstOrDefault(s => s.CharacterId == id); ;
        }

        [HttpPost("SetEvent")]
        public bool SetEvent([FromBody] UserDataModel model)
        {
            if (model == null)
            {
                return false;
            }

            var exists = _userManager.PlayerEvents.Any(s => s.CharacterId == model.CharacterId);

            if (exists)
            {
                _userManager.PlayerEvents.RemoveWhere(s => s.CharacterId != model.CharacterId, out var result);
                _userManager.PlayerEvents = result;
            }

            _userManager.PlayerEvents.Add(model);
            return true;
        }

        [HttpPost("GetEvents")]
        public IEnumerable<UserDataModel> GetEvents([FromBody] IEnumerable<long> characterIds)
        {
            return characterIds.Select(id => _userManager.PlayerEvents.FirstOrDefault(s => s.CharacterId == id)).Where(found => found != null);
        }

        [HttpGet("DeleteInstantBattleEvents")]
        public bool DeleteInstantBattleEvents()
        {
            foreach (var ib in _userManager.PlayerEvents.Where(s => s.EventType == EventType.INSTANTBATTLE))
            {
                ib.EventType = EventType.NONE;
            }

            return true;
        }
    }
}
