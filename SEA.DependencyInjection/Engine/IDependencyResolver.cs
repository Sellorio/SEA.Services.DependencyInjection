using System;

namespace SEA.DependencyInjection.Engine
{
    public interface IDependencyResolver : IServiceProvider
    {
        TDependency Get<TDependency>();
        object Get(Type dependencyType);
        TObject Create<TObject>() where TObject : class, new();
    }
}
