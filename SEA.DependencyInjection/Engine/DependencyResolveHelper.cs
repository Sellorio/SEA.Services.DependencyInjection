using SEA.DependencyInjection.Configuration;
using SEA.DependencyInjection.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SEA.DependencyInjection.Engine
{
    internal static class DependencyResolveHelper
    {
        internal static TObject ResolveObject<TObject>(ResolutionContext resolutionContext)
        {
            return (TObject)ResolveObject(typeof(TObject), resolutionContext);
        }

        internal static object ResolveObject(Type type, ResolutionContext resolutionContext)
        {
            return
                ResolveObject(
                    type,
                    resolutionContext,
                    resolutionContext.ScopedServiceCache == null ? ServiceScope.Singleton : ServiceScope.Scoped,
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

        private static object ResolveObject(Type type, ResolutionContext resolutionContext, ServiceScope ownerScope, IEnumerable<Type> typeResolutionChain)
        {
            var serviceInfo = resolutionContext.Settings.Services.FirstOrDefault(x => x.ServiceType == type);

            if (serviceInfo == null)
            {
                if (!resolutionContext.Settings.IsServiceAutoDetectionEnabled)
                {
                    throw new InvalidOperationException(
                        $"There is no service configured for the type {type.Name}.");
                }

                serviceInfo =
                    resolutionContext.AutoDetectedServiceInfoCache.GetOrLoad(
                        type,
                        () => AutoDetectedServiceInfo.Find(type, ownerScope, resolutionContext.Settings));
            }

            if (ownerScope == ServiceScope.Singleton && serviceInfo.Scope == ServiceScope.Scoped)
            {
                throw new InvalidOperationException($"Attempted to inject {serviceInfo.ServiceType.Name} (a scoped service) in a singleton.");
            }

            if (typeResolutionChain.Contains(type))
            {
                throw new InvalidOperationException(
                    $"A dependency loop has been encountered: {string.Join(" => ", typeResolutionChain.Select(x => x.Name))} => {type.Name}.");
            }

            if (serviceInfo.Scope == ServiceScope.Singleton)
            {
                return
                    resolutionContext.SingletonServiceCache.GetOrLoad(
                        serviceInfo,
                        () => CreateInstance(serviceInfo, resolutionContext, ServiceScope.Singleton, typeResolutionChain)).Instance;
            }

            if (serviceInfo.Scope == ServiceScope.Scoped)
            {
                return
                    resolutionContext.ScopedServiceCache.GetOrLoad(
                        serviceInfo,
                        () => CreateInstance(serviceInfo, resolutionContext, ServiceScope.Scoped, typeResolutionChain)).Instance;
            }

            return CreateInstance(serviceInfo, resolutionContext, ownerScope, typeResolutionChain).Instance;
        }

        private static ServiceInstance CreateInstance(ServiceInfo serviceInfo, ResolutionContext resolutionContext, ServiceScope ownerScope, IEnumerable<Type> typeResolutionChain)
        {
            if (serviceInfo.Instance != null)
            {
                return new ServiceInstance(serviceInfo, serviceInfo.Instance);
            }

            if (serviceInfo.CreationFunction != null)
            {
                return new ServiceInstance(serviceInfo, serviceInfo.CreationFunction.Invoke(resolutionContext.DependencyResolver));
            }

            var implementationTypeInfo = serviceInfo.ImplementationTypeInfo;
            var newTypeResolutionChain = typeResolutionChain.Append(serviceInfo.ServiceType);
            var dependencies =
                implementationTypeInfo.DependencyProperties
                    .Select(x => ResolveObject(x.PropertyType, resolutionContext, ownerScope, newTypeResolutionChain))
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
