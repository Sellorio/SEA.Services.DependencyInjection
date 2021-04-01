using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;

namespace SEA.DependencyInjection.Configuration
{
    internal sealed class DependencyResolutionSettings
    {
        internal bool IsServiceAutoDetectionEnabled { get; }
        internal IReadOnlyList<Assembly> ServiceAutoDetectionAssemblies { get; }
        internal IReadOnlyList<ServiceInfo> Services { get; }

        internal DependencyResolutionSettings(
            bool isServiceAutoDetectionEnabled,
            IEnumerable<Assembly> serviceAutoDetectionAssemblies,
            IEnumerable<ServiceInfo> services)
        {
            IsServiceAutoDetectionEnabled = isServiceAutoDetectionEnabled;
            ServiceAutoDetectionAssemblies = ImmutableArray.CreateRange(serviceAutoDetectionAssemblies);
            Services = ImmutableArray.CreateRange(services);
        }
    }
}
