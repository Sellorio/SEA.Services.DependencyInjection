using SEA.DependencyInjection.Configuration;
using SEA.DependencyInjection.Utility;
using System;

namespace SEA.DependencyInjection.Engine
{
    internal sealed class ResolutionContext
    {
        internal IDependencyResolver DependencyResolver { get; init; }
        internal DependencyResolutionSettings Settings { get; init; }
        internal Cache<ServiceInfo, ServiceInstance> SingletonServiceCache { get; init; }
        internal Cache<ServiceInfo, ServiceInstance> ScopedServiceCache { get; init; }
        internal Cache<Type, ServiceInfo> AutoDetectedServiceInfoCache { get; init; }
    }
}
