using System;
using System.Linq;

namespace SEA.DependencyInjection.Reflection
{
    internal static class TypeExtensions
    {
        public static bool HasEmptyConstructor(this Type type)
        {
            return type.GetConstructors().Any(x => x.GetParameters().Length == 0);
        }

        public static bool IsConstructable(this Type type)
        {
            return !type.IsAbstract && !type.IsInterface;
        }
    }
}
