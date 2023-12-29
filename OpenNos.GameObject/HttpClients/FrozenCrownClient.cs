using Newtonsoft.Json;
using NosByte.Shared;
using NosByte.Shared.ApiModels;
using OpenNos.Core;
using OpenNos.Core.Logger;
using System;
using System.Net.Http;
using System.Text;

namespace OpenNos.GameObject.HttpClients
{
    public class FrozenCrownClient : Singleton<FrozenCrownClient>
    {
        private readonly AnonymousHttpClientFactory _clientFactory = AnonymousHttpClientFactory.Instance;

        public bool SetPercent(FcPercentModel model)
        {
            try
            {
                if (model == null)
                {
                    return false;
                }

                var client = _clientFactory.Create(StaticApiData.SET_FC_PERCENT);

                var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
                var response = client.PostAsync(string.Empty, content).Result;

                if (response.IsSuccessStatusCode)
                {
                    return JsonConvert.DeserializeObject<bool>(response.Content.ReadAsStringAsync().Result);
                }

                return false;
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
                return false;
            }
        }

        public GetPercentModel GetPercent()
        {
            try
            {
                var client = _clientFactory.Create(StaticApiData.GET_FC_PERCENT);

                var response = client.GetAsync(string.Empty).Result;

                if (response.IsSuccessStatusCode)
                {
                    return JsonConvert.DeserializeObject<GetPercentModel>(response.Content.ReadAsStringAsync().Result);
                }

                return null;
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
                return null;
            }
        }
    }
}
