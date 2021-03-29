using SEA.DependencyInjection.Configuration;
using SEA.DependencyInjection.Reflection;
using SEA.DependencyInjection.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SEA.DependencyInjection.Engine
{
    internal static class DependencyResolveHelper
    {
        internal static TObject ResolveObject<TObject>(
            IDependencyResolver dependencyResolver,
            DependencyResolutionSettings settings,
            Cache<ServiceInfo, ServiceInstance> singletonServiceCache,
            Cache<ServiceInfo, ServiceInstance> scopedServiceCache,
            Cache<Type, ServiceTypeInfo> autoDetectedServiceInfoCache)
        {
            return
                (TObject)ResolveObject(
                    typeof(TObject),
                    dependencyResolver,
                    settings,
                    singletonServiceCache,
                    scopedServiceCache,
                    autoDetectedServiceInfoCache);
        }

        internal static object ResolveObject(
            Type type,
            IDependencyResolver dependencyResolver,
            DependencyResolutionSettings settings,
            Cache<ServiceInfo, ServiceInstance> singletonServiceCache,
            Cache<ServiceInfo, ServiceInstance> scopedServiceCache,
            Cache<Type, ServiceTypeInfo> autoDetectedServiceInfoCache)
        {
            return
                ResolveObject(
                    type,
                    dependencyResolver,
                    settings,
                    singletonServiceCache,
                    scopedServiceCache,
                    autoDetectedServiceInfoCache,
                    scopedServiceCache == null
                        ? new[] { ServiceScope.Singleton, ServiceScope.Transient }
                        : new[] { ServiceScope.Singleton, ServiceScope.Scoped, ServiceScope.Transient },
                    Enumerable.Empty<Type>());
        }

        internal static TObject CreateObject<TObject>(IDependencyResolver dependencyResolver)
            where TObject : class, new()
        {
            var info = ServiceTypeInfo.Get(typeof(TObject));
            var dependencies = info.DependencyProperties.Select(x => dependencyResolver.Get(x.PropertyType)).ToArray();
            var instance = new TObject();

            for (var i = 0; i < dependencies.Length; i++)
            {
                info.DependencyProperties[i].SetValue(instance, dependencies[i]);
            }

            return instance;
        }

        private static object ResolveObject(
            Type type,
            IDependencyResolver dependencyResolver,
            DependencyResolutionSettings settings,
            Cache<ServiceInfo, ServiceInstance> singletonServiceCache,
            Cache<ServiceInfo, ServiceInstance> scopedServiceCache,
            Cache<Type, ServiceTypeInfo> autoDetectedServiceInfoCache,
            ServiceScope[] permittedScopes,
            IEnumerable<Type> typeResolutionChain)
        {
            var serviceInfo = settings.Services.FirstOrDefault(x => x.ServiceType == type);

            if (serviceInfo == null)
            {
                throw new InvalidOperationException(
                    $"There is no service configured for the type {type.Name}.");
            }

            if (!permittedScopes.Contains(serviceInfo.Scope))
            {
                throw new InvalidOperationException(
                    $"{type.Name} could not be injected since only {string.Join("/", permittedScopes)} are allowed in this context.");
            }

            if (typeResolutionChain.Contains(type))
            {
                throw new InvalidOperationException(
                    $"A dependency loop has been encountered: {string.Join(" => ", typeResolutionChain.Select(x => x.Name))} => {type.Name}.");
            }

            if (serviceInfo.Scope == ServiceScope.Singleton)
            {
                return
                    singletonServiceCache.GetOrLoad(
                        serviceInfo,
                        () => CreateInstance(serviceInfo, dependencyResolver, settings, singletonServiceCache, scopedServiceCache, autoDetectedServiceInfoCache, typeResolutionChain));
            }

            if (serviceInfo.Scope == ServiceScope.Scoped)
            {
                return
                    scopedServiceCache.GetOrLoad(
                        serviceInfo,
                        () => CreateInstance(serviceInfo, dependencyResolver, settings, singletonServiceCache, scopedServiceCache, autoDetectedServiceInfoCache, typeResolutionChain));
            }

            return CreateInstance(serviceInfo, dependencyResolver, settings, singletonServiceCache, scopedServiceCache, autoDetectedServiceInfoCache, typeResolutionChain);
        }

        private static ServiceInstance CreateInstance(
            ServiceInfo serviceInfo,
            IDependencyResolver dependencyResolver,
            DependencyResolutionSettings settings,
            Cache<ServiceInfo, ServiceInstance> singletonServiceCache,
            Cache<ServiceInfo, ServiceInstance> scopedServiceCache,
            Cache<Type, ServiceTypeInfo> autoDetectedServiceInfoCache,
            IEnumerable<Type> typeResolutionChain)
        {
            if (serviceInfo.Instance != null)
            {
                return new ServiceInstance(serviceInfo, serviceInfo.Instance);
            }

            if (serviceInfo.CreationFunction != null)
            {
                return new ServiceInstance(serviceInfo, serviceInfo.CreationFunction.Invoke(dependencyResolver));
            }

            var implementationTypeInfo = serviceInfo.ImplementationTypeInfo;

            if (implementationTypeInfo == null)
            {
                implementationTypeInfo = autoDetectedServiceInfoCache.GetOrLoad(serviceInfo.ServiceType, () => AutoDetectedServiceInfo.Find(serviceInfo.ServiceType, settings));
            }

            var newTypeResolutionChain = typeResolutionChain.Append(serviceInfo.ServiceType);
            var greaterOrEqualScopes = typeof(ServiceScope).GetEnumValues().Cast<ServiceScope>().Where(x => (int)x >= (int)serviceInfo.Scope).ToArray();
            var dependencies =
                implementationTypeInfo.DependencyProperties
                    .Select(x =>
                        ResolveObject(x.PropertyType, dependencyResolver, settings, singletonServiceCache, scopedServiceCache, autoDetectedServiceInfoCache, greaterOrEqualScopes, newTypeResolutionChain))
                    .ToArray();

            var rawInstance = Activator.CreateInstance(implementationTypeInfo.Type);

            for (var i = 0; i < dependencies.Length; i++)
            {
                implementationTypeInfo.DependencyProperties[i].SetValue(rawInstance, dependencies[i]);
            }

            var instance = new ServiceInstance(serviceInfo, rawInstance);

            return instance;
        }
    }
}
