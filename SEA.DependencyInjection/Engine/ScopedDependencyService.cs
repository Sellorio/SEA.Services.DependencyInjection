using SEA.DependencyInjection.Configuration;
using SEA.DependencyInjection.Utility;
using System;
using System.Collections.Generic;

namespace SEA.DependencyInjection.Engine
{
    internal class ScopedDependencyService : IScopedDependencyService
    {
        private List<ServiceInstance> _transientServices = new();
        private DependencyService _parent;
        private bool _isDisposed;

        internal Cache<ServiceInfo, ServiceInstance> ServiceCache { get; } = new();

        internal ScopedDependencyService(DependencyService parent)
        {
            _parent = parent;
        }

        public TDependency Get<TDependency>()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(ScopedDependencyService));
            }

            return DependencyResolveHelper.ResolveObject<TDependency>(this, _parent.Settings, _parent.ServiceCache, ServiceCache, _parent.AutoDetectedServiceInfoCache);
        }

        public object Get(Type dependencyType)
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(ScopedDependencyService));
            }

            return DependencyResolveHelper.ResolveObject(dependencyType, this, _parent.Settings, _parent.ServiceCache, ServiceCache, _parent.AutoDetectedServiceInfoCache);
        }

        public TObject Create<TObject>()
            where TObject : class, new()
        {
            return DependencyResolveHelper.CreateObject<TObject>(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    ServiceCache.Dispose();

                    foreach (var service in _transientServices)
                    {
                        service.Dispose();
                    }
                }

                _transientServices = null;
                _parent = null;
                _isDisposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        object IServiceProvider.GetService(Type serviceType)
        {
            try
            {
                return Get(serviceType);
            }
            catch (InvalidOperationException) //TODO: replace with specific exception types
            {
                return null; // return null if the service is not configured
            }
        }
    }
}
