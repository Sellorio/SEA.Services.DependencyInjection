using System;
using System.Linq;
using System.Reflection;

namespace SEA.DependencyInjection.Reflection
{
    internal static class PropertyInfoExtensions
    {
        public static bool IsInitSetProperty(this PropertyInfo property)
        {
            if (!property.CanWrite)
            {
                return false;
            }

            return property.SetMethod.ReturnParameter.GetRequiredCustomModifiers().Contains(typeof(System.Runtime.CompilerServices.IsExternalInit));
        }
    }
}
