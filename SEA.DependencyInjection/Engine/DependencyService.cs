using SEA.DependencyInjection.Configuration;
using SEA.DependencyInjection.Reflection;
using SEA.DependencyInjection.Utility;
using System;

namespace SEA.DependencyInjection.Engine
{
    internal class DependencyService : IDependencyService
    {
        private bool _isDisposed;

        internal Cache<ServiceInfo, ServiceInstance> ServiceCache { get; } = new();
        internal Cache<Type, ServiceTypeInfo> AutoDetectedServiceInfoCache = new();
        internal DependencyResolutionSettings Settings { get; }

        internal DependencyService(DependencyResolutionSettings settings)
        {
            Settings = settings;
        }

        public TDependency Get<TDependency>()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(ScopedDependencyService));
            }

            return DependencyResolveHelper.ResolveObject<TDependency>(this, Settings, ServiceCache, null, AutoDetectedServiceInfoCache);
        }

        public object Get(Type dependencyType)
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(ScopedDependencyService));
            }

            return DependencyResolveHelper.ResolveObject(dependencyType, this, Settings, ServiceCache, null, AutoDetectedServiceInfoCache);
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
