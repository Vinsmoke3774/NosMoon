using OpenNos.Core.Logger;
using System;
using System.Runtime;

namespace OpenNos.GameObject.Helpers
{
    public static class LargeHeapCompactor
    {
        public static void CompactHeap()
        {
            Logger.Log.Warn($"Compacting heap.");
            GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
            GC.Collect();
        }
    }
}
