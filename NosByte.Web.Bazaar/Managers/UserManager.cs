using System.Collections.Concurrent;
using NosByte.Shared.ApiModels;

namespace NosByte.Web.Bazaar.Managers
{
    public class UserManager
    {
        public UserManager()
        {
            PlayerEvents = new ConcurrentBag<UserDataModel>();
        }

        public ConcurrentBag<UserDataModel> PlayerEvents { get; set; }
    }
}
