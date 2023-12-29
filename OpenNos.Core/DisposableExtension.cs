using System;

namespace OpenNos.Core
{
    public class DisposableExtension : IDisposable
    {
        private bool disposed = false;

        public bool IsDisposed
        {
            get
            {
                return disposed;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // don't care
                }

                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}
