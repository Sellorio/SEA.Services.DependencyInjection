using System;

namespace SEA.DependencyInjection.Engine
{
    public interface IDependencyService : IDependencyResolver, IDisposable
    {
        IScopedDependencyService CreateScope();
    }
}
