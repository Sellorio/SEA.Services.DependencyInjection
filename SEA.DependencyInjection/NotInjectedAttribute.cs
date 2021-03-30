using System;

namespace SEA.DependencyInjection
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class NotInjectedAttribute : Attribute
    {
    }
}
