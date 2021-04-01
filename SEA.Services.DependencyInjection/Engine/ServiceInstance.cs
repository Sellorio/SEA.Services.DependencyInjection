using SEA.DependencyInjection.Configuration;
using System;
namespace SEA.DependencyInjection.Engine
{
    internal sealed class ServiceInstance : IDisposable
    {
        private bool _isDisposed;

        internal ServiceInfo ServiceInfo { get; }
        internal object Instance { get; }

        internal ServiceInstance(ServiceInfo serviceInfo, object instance)
        {
            ServiceInfo = serviceInfo;
            Instance = instance;
        }

        private void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing && ServiceInfo.ImplementationTypeInfo.IsDisposable)
                {
                    try
                    {
                        ((IDisposable)Instance).Dispose();
                    }
                    catch
                    {
                        //TODO: Add logging later
                    }
                }

                _isDisposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
