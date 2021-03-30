using Microsoft.Extensions.DependencyInjection;
using SEA.DependencyInjection.Engine;
using SEA.DependencyInjection.Reflection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SEA.DependencyInjection.Configuration
{
    public sealed partial class DependencyBuilder : IDependencyBuilder
    {
        private readonly List<ServiceInfo> _serviceInfos = new();
        private bool _enableServiceOverride;
        private bool _enableServiceAutoDetection;
        private List<Assembly> _serviceAutoDetectionAssemblies = new();

        public IDependencyBuilder EnableServiceOverride(bool enable = true)
        {
            _enableServiceOverride = enable;
            return this;
        }

        public IDependencyBuilder EnableServiceAutoDetection(params Assembly[] searchAssemblies)
        {
            _enableServiceAutoDetection = true;

            if (searchAssemblies.Length == 0)
            {
                _serviceAutoDetectionAssemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();
            }

            foreach (var assembly in searchAssemblies)
            {
                if (!_serviceAutoDetectionAssemblies.Contains(assembly))
                {
                    _serviceAutoDetectionAssemblies.Add(assembly);
                }
            }

            return this;
        }

        public IDependencyBuilder AddServices(IServiceCollection services)
        {
            var asServiceCollection = (IServiceCollection)this;

            foreach (var service in services)
            {
                asServiceCollection.Add(service);
            }

            return this;
        }

        public IDependencyBuilder AddSingleton<TService>()
            where TService : new()
        {
            EnsureUniqueService(typeof(TService), ServiceScope.Singleton);
            EnsureConstructibleType(typeof(TService));
            _serviceInfos.Add(new ServiceInfo(typeof(TService), typeof(TService), ServiceScope.Singleton, null, null));
            return this;
        }

        public IDependencyBuilder AddSingleton<TService, TImplementation>()
            where TImplementation : TService, new()
        {
            EnsureUniqueService(typeof(TService), ServiceScope.Singleton);
            EnsureConstructibleType(typeof(TImplementation));
            _serviceInfos.Add(new ServiceInfo(typeof(TService), typeof(TImplementation), ServiceScope.Singleton, null, null));
            return this;
        }

        public IDependencyBuilder AddSingleton<TService>(TService instance)
        {
            EnsureUniqueService(typeof(TService), ServiceScope.Singleton);
            _serviceInfos.Add(new ServiceInfo(typeof(TService), instance.GetType(), ServiceScope.Singleton, instance, null));
            return this;
        }

        public IDependencyBuilder AddScoped<TService>()
            where TService : new()
        {
            EnsureUniqueService(typeof(TService), ServiceScope.Scoped);
            EnsureConstructibleType(typeof(TService));
            _serviceInfos.Add(new ServiceInfo(typeof(TService), typeof(TService), ServiceScope.Scoped, null, null));
            return this;
        }

        public IDependencyBuilder AddScoped<TService, TImplementation>()
            where TImplementation : TService, new()
        {
            EnsureUniqueService(typeof(TService), ServiceScope.Scoped);
            EnsureConstructibleType(typeof(TImplementation));
            _serviceInfos.Add(new ServiceInfo(typeof(TService), typeof(TImplementation), ServiceScope.Scoped, null, null));
            return this;
        }

        public IDependencyBuilder AddScoped<TService>(Func<IDependencyResolver, TService> serviceBuilder)
        {
            EnsureUniqueService(typeof(TService), ServiceScope.Scoped);
            _serviceInfos.Add(new ServiceInfo(typeof(TService), null, ServiceScope.Scoped, null, x => serviceBuilder.Invoke(x)));
            return this;
        }

        public IDependencyBuilder AddTransient<TService>()
            where TService : new()
        {
            EnsureUniqueService(typeof(TService), ServiceScope.Transient);
            EnsureConstructibleType(typeof(TService));
            _serviceInfos.Add(new ServiceInfo(typeof(TService), typeof(TService), ServiceScope.Transient, null, null));
            return this;
        }

        public IDependencyBuilder AddTransient<TService, TImplementation>()
            where TImplementation : TService, new()
        {
            EnsureUniqueService(typeof(TService), ServiceScope.Transient);
            EnsureConstructibleType(typeof(TImplementation));
            _serviceInfos.Add(new ServiceInfo(typeof(TService), typeof(TImplementation), ServiceScope.Transient, null, null));
            return this;
        }

        public IDependencyBuilder AddTransient<TService>(Func<IDependencyResolver, TService> serviceBuilder)
        {
            EnsureUniqueService(typeof(TService), ServiceScope.Transient);
            _serviceInfos.Add(new ServiceInfo(typeof(TService), null, ServiceScope.Transient, null, x => serviceBuilder.Invoke(x)));
            return this;
        }

        public IDependencyService Build()
        {
            return
                new DependencyService(
                    new DependencyResolutionSettings(
                        _enableServiceAutoDetection,
                        _serviceAutoDetectionAssemblies,
                        _serviceInfos));
        }

        private void EnsureUniqueService(Type type, ServiceScope lifetime)
        {
            var conflictingService = _serviceInfos.FirstOrDefault(x => x.ServiceType == type);

            if (conflictingService != null)
            {
                if (_enableServiceOverride)
                {
                    if (conflictingService.Scope != lifetime)
                    {
                        throw new InvalidOperationException($"Attempting to override {type.Name} dependency with lifetime {conflictingService.Scope} with a new dependency with a different lifetime ({lifetime}) is not permitted.");
                    }
                }
                else
                {
                    throw new InvalidOperationException($"An existing service of type {type.Name} has already been registered. Use {nameof(EnableServiceOverride)} if overriding of existing services is desired.");
                }

                _serviceInfos.Remove(conflictingService);
            }
        }

        private static void EnsureConstructibleType(Type type)
        {
            if (type.IsAbstract || type.IsInterface)
            {
                throw new ArgumentException("The specified type must be a constructible type in order to use it as the implementation type of a service.");
            }

            if (!type.GetConstructors().Any(x => x.GetParameters().Length == 0))
            {
                throw new ArgumentException("The specified type must contain an empty constructor. All dependencies must be specified using init properties.");
            }
        }
    }
}
