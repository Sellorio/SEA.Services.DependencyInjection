using SEA.DependencyInjection.Configuration;
using SEA.DependencyInjection.Utility;
using System;
using System.Collections.Generic;

namespace SEA.DependencyInjection.Engine
{
    internal class ScopedDependencyService : IScopedDependencyService
    {
        private readonly ResolutionContext _resolutionContext;
        private List<ServiceInstance> _transientServices = new();
        private bool _isDisposed;

        internal Cache<ServiceInfo, ServiceInstance> ServiceCache { get; } = new();

        internal ScopedDependencyService(DependencyService parent)
        {
            _resolutionContext = new()
            {
                AutoDetectedServiceInfoCache = parent.AutoDetectedServiceInfoCache,
                DependencyResolver = this,
                ScopedServiceCache = ServiceCache,
                Settings = parent.Settings,
                SingletonServiceCache = parent.ServiceCache
            };
        }

        public TDependency Get<TDependency>()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(ScopedDependencyService));
            }

            return DependencyResolveHelper.ResolveObject<TDependency>(_resolutionContext);
        }

        public object Get(Type dependencyType)
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(ScopedDependencyService));
            }

            return DependencyResolveHelper.ResolveObject(dependencyType, _resolutionContext);
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
