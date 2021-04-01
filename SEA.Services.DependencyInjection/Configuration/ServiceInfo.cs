using SEA.DependencyInjection.Engine;
using SEA.DependencyInjection.Reflection;
using System;

namespace SEA.DependencyInjection.Configuration
{
    /// <summary>
    /// The information relating to a service as defined when setting up dependencies.
    /// </summary>
    internal class ServiceInfo
    {
        /// <summary>
        /// The type that dependants will use on their properties when specifying a service to be injected. This
        /// will usually be an interface.
        /// </summary>
        internal Type ServiceType { get; }

        /// <summary>
        /// The information about the implementation type. This is not set when <see cref="CreationFunction"/> is used
        /// since the implementation type is not known.
        /// </summary>
        internal ServiceTypeInfo ImplementationTypeInfo { get; }

        /// <summary>
        /// The scope of the service. Services may only depend on services with an equal or greater scope.
        /// </summary>
        internal ServiceScope Scope { get; }

        /// <summary>
        /// This may be set for Singleton scoped services to have a specific instance injected.
        /// </summary>
        internal object Instance { get; }
        
        /// <summary>
        /// This function can be used to customize the creation of a <see cref="ServiceScope.Scoped"/> or
        /// <see cref="ServiceScope.Transient"/> service.
        /// </summary>
        internal Func<IDependencyResolver, object> CreationFunction { get; }

        /// <summary>
        /// How services are injected into the service. Defaults to only using init properties. Can be changed to use
        /// constructor parameters either exclusively or in addition to properties.
        /// </summary>
        internal InjectionMode InjectionMode { get; }

        internal ServiceInfo(Type serviceType, Type implementationType, ServiceScope scope, object instance, Func<IDependencyResolver, object> creationFunction, InjectionMode injectionMode)
        {
            ServiceType = serviceType;

            if (implementationType != null)
            {
                ImplementationTypeInfo = ServiceTypeInfo.Get(implementationType);
            }

            Scope = scope;
            Instance = instance;
            CreationFunction = creationFunction;
            InjectionMode = injectionMode;
        }
    }
}
