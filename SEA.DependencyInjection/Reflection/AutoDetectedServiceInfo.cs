using SEA.DependencyInjection.Configuration;
using System;
using System.Linq;

namespace SEA.DependencyInjection.Reflection
{
    internal static class AutoDetectedServiceInfo
    {
        internal static ServiceInfo Find(Type expectedType, ServiceScope serviceScope, DependencyResolutionSettings settings)
        {
            if (!settings.IsServiceAutoDetectionEnabled)
            {
                throw new InvalidOperationException("Attempted to auto detect service implementation when auto detection is disabled.");
            }

            var implementations =
                settings.ServiceAutoDetectionAssemblies
                    .SelectMany(x => x.GetTypes())
                    .Where(expectedType.IsAssignableFrom)
                    .Where(x => !x.IsAbstract && !x.IsInterface && x.GetConstructors().Any(x => x.GetParameters().Length == 0))
                    .ToList();

            if (implementations.Count > 1)
            {
                throw new InvalidOperationException(
                    $"Attempted to auto detect service implementation for {expectedType.Name} but multiple types were matched ({string.Join(", ", implementations.Select(x => x.Name))}).");
            }

            if (implementations.Count == 0)
            {
                throw new InvalidOperationException($"Attempted to auto detect service implementation for {expectedType.Name} but no types were matched.");
            }

            return new ServiceInfo(expectedType, implementations[0], serviceScope, null, null);
        }
    }
}
