using SEA.DependencyInjection.Utility;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace SEA.DependencyInjection.Reflection
{
    internal sealed class ServiceTypeInfo
    {
        private static readonly Cache<Type, ServiceTypeInfo> _cache = new();

        internal Type Type { get; }
        internal IReadOnlyList<PropertyInfo> DependencyProperties { get; }
        internal bool IsDisposable { get; }

        private ServiceTypeInfo(Type type)
        {
            Type = type;
            DependencyProperties = ImmutableArray.CreateRange(GetDependencyProperties(type));
            IsDisposable = typeof(IDisposable).IsAssignableFrom(type);
        }

        internal static ServiceTypeInfo Get(Type type)
        {
            return _cache.GetOrLoad(type, () => new ServiceTypeInfo(type));
        }

        private static IEnumerable<PropertyInfo> GetDependencyProperties(Type type)
        {
            var result = type.GetProperties().Where(x => x.IsInitSetProperty());

            if (type.BaseType != null)
            {
                result = GetDependencyProperties(type).Concat(result);
            }

            return result;
        }
    }
}
