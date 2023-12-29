using OpenNos.Core.Logger;
using System;
using System.Reactive.Linq;

namespace NosByte.Shared
{
    public static class EventSubscriber
    {
        public static IDisposable SafeSubscribe(this IObservable<long> obs, Action<long> callback)
        {
            IDisposable observable = null;

            try
            {
                observable = obs.Subscribe(x =>
                {
                    try
                    {
                        callback(x);
                    }
                    catch
                    {
                        observable?.Dispose();
                        // Nothing happens, don't crash fdp
                    }
                });

                return observable;
            }
            catch (Exception e)
            {
                observable?.Dispose();
                Logger.Log.Error(null, e);
                return null;
            }
        }
    }

}
