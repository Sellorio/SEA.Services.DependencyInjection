using Microsoft.Extensions.DependencyInjection;
using SEA.DependencyInjection.Engine;
using System;
using System.Reflection;

namespace SEA.DependencyInjection.Configuration
{
    public interface IDependencyBuilder : IServiceCollection
    {
        IDependencyBuilder EnableServiceOverride(bool enable = true);
        IDependencyBuilder EnableServiceAutoDetection(params Assembly[] searchAssemblies);

        IDependencyBuilder AddServices(IServiceCollection services);

        IDependencyBuilder AddSingleton<TService>();
        IDependencyBuilder AddSingleton<TService, TImplementation>() where TImplementation : TService, new();
        IDependencyBuilder AddSingleton<TService>(TService instance);

        IDependencyBuilder AddScoped<TService>();
        IDependencyBuilder AddScoped<TService, TImplementation>() where TImplementation : TService, new();
        IDependencyBuilder AddScoped<TService>(Func<IDependencyResolver, TService> serviceBuilder);

        IDependencyBuilder AddTransient<TService>();
        IDependencyBuilder AddTransient<TService, TImplementation>() where TImplementation : TService, new();
        IDependencyBuilder AddTransient<TService>(Func<IDependencyResolver, TService> serviceBuilder);

        IDependencyService Build();
    }
}
