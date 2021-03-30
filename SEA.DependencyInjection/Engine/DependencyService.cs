using SEA.DependencyInjection.Configuration;
using SEA.DependencyInjection.Utility;
using System;

namespace SEA.DependencyInjection.Engine
{
    internal class DependencyService : IDependencyService
    {
        private bool _isDisposed;
        private readonly ResolutionContext _resolutionContext;

        internal Cache<ServiceInfo, ServiceInstance> ServiceCache { get; } = new();
        internal Cache<Type, ServiceInfo> AutoDetectedServiceInfoCache = new();
        internal DependencyResolutionSettings Settings { get; }

        internal DependencyService(DependencyResolutionSettings settings)
        {
            Settings = settings;
            _resolutionContext = new()
            {
                AutoDetectedServiceInfoCache = AutoDetectedServiceInfoCache,
                DependencyResolver = this,
                ScopedServiceCache = null,
                Settings = Settings,
                SingletonServiceCache = ServiceCache
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

        public IScopedDependencyService CreateScope()
        {
            return new ScopedDependencyService(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    ServiceCache.Dispose();
                }

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
