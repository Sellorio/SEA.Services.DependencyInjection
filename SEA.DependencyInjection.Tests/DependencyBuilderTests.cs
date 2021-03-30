using Microsoft.Extensions.DependencyInjection;
using SEA.DependencyInjection.Configuration;
using SEA.DependencyInjection.Tests.TestServices;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace SEA.DependencyInjection.Tests
{
    public class DependencyBuilderTests
    {
        private readonly DependencyBuilder _dependencyBuilder = new();
        private readonly IServiceCollection _dependencyBuilderAsServiceCollection;

        public DependencyBuilderTests()
        {
            _dependencyBuilderAsServiceCollection = _dependencyBuilder;
        }

        [Fact]
        public void Empty()
        {
            var dependencyService = _dependencyBuilder.Build();
            Assert.NotNull(dependencyService);
        }

        [Fact]
        public void AddSingleton_SingleType_NotConstructible()
        {
            Assert.Throws<ArgumentException>(() => _dependencyBuilder.AddSingleton<IFirst>());
        }

        [Fact]
        public void AddSingleton_SingleType_Success()
        {
            _dependencyBuilder.AddSingleton<First.RequiresNone>();
            Assert.Throws<InvalidOperationException>(() => _dependencyBuilder.AddSingleton<First.RequiresNone>());
        }

        [Fact]
        public void AddSingleton_DifferentImplementation_NotConstructible()
        {
            Assert.Throws<ArgumentException>(() => _dependencyBuilder.AddSingleton<IFirst, IUniqueFirst>());
        }

        [Fact]
        public void AddSingleton_DifferentImplementation_Success()
        {
            _dependencyBuilder.AddSingleton<IFirst, First.RequiresNone>();
            Assert.Throws<InvalidOperationException>(() => _dependencyBuilder.AddSingleton<IFirst, First.RequiresNone>());
        }

        [Fact]
        public void AddSingleton_Instance()
        {
            var first = new First.RequiresNone();
            _dependencyBuilder.AddSingleton<IFirst>(first);
            Assert.Throws<InvalidOperationException>(() => _dependencyBuilder.AddSingleton<IFirst>(first));
        }

        [Fact]
        public void AddScoped_SingleType_NotConstructible()
        {
            Assert.Throws<ArgumentException>(() => _dependencyBuilder.AddScoped<IFirst>());
        }

        [Fact]
        public void AddScoped_SingleType_Success()
        {
            _dependencyBuilder.AddScoped<First.RequiresNone>();
            Assert.Throws<InvalidOperationException>(() => _dependencyBuilder.AddScoped<First.RequiresNone>());
        }

        [Fact]
        public void AddScoped_DifferentImplementation_NotConstructible()
        {
            Assert.Throws<ArgumentException>(() => _dependencyBuilder.AddScoped<IFirst, IUniqueFirst>());
        }

        [Fact]
        public void AddScoped_DifferentImplementation_Success()
        {
            _dependencyBuilder.AddScoped<IFirst, First.RequiresNone>();
            Assert.Throws<InvalidOperationException>(() => _dependencyBuilder.AddScoped<IFirst, First.RequiresNone>());
        }

        [Fact]
        public void AddScoped_Function()
        {
            var first = new First.RequiresNone();
            _dependencyBuilder.AddScoped<IFirst>(x => first);
            Assert.Throws<InvalidOperationException>(() => _dependencyBuilder.AddScoped<IFirst>(x => first));
        }

        [Fact]
        public void AddTransient_SingleType_NotConstructible()
        {
            Assert.Throws<ArgumentException>(() => _dependencyBuilder.AddTransient<IFirst>());
        }

        [Fact]
        public void AddTransient_SingleType_Success()
        {
            _dependencyBuilder.AddTransient<First.RequiresNone>();
            Assert.Throws<InvalidOperationException>(() => _dependencyBuilder.AddTransient<First.RequiresNone>());
        }

        [Fact]
        public void AddTransient_DifferentImplementation_NotConstructible()
        {
            Assert.Throws<ArgumentException>(() => _dependencyBuilder.AddTransient<IFirst, IUniqueFirst>());
        }

        [Fact]
        public void AddTransient_DifferentImplementation_Success()
        {
            _dependencyBuilder.AddTransient<IFirst, First.RequiresNone>();
            Assert.Throws<InvalidOperationException>(() => _dependencyBuilder.AddTransient<IFirst, First.RequiresNone>());
        }

        [Fact]
        public void AddTransient_Function()
        {
            var first = new First.RequiresNone();
            _dependencyBuilder.AddTransient<IFirst>(x => first);
            Assert.Throws<InvalidOperationException>(() => _dependencyBuilder.AddTransient<IFirst>(x => first));
        }

        [Fact]
        public void EnableServiceOverride()
        {
            _dependencyBuilder.AddSingleton<First.RequiresNone>();
            Assert.Throws<InvalidOperationException>(() => _dependencyBuilder.AddSingleton<First.RequiresNone>());

            _dependencyBuilder.EnableServiceOverride();
            _dependencyBuilder.AddSingleton<First.RequiresNone>();

            _dependencyBuilder.EnableServiceOverride(false);
            Assert.Throws<InvalidOperationException>(() => _dependencyBuilder.AddSingleton<First.RequiresNone>());
        }

        [Fact]
        public void EnableServiceAutoDetection()
        {
            _dependencyBuilder.EnableServiceOverride();
            Assert.True(true);
        }

        [Fact]
        public void AddServices()
        {
            var services = new ServiceCollection();
            services.AddScoped<IFirst, First.RequiresSecond>();
            services.AddScoped<ISecond, Second.RequiresNone>();

            _dependencyBuilder.AddServices(services);

            Assert.True(true);
        }

        [Fact]
        public void ServiceCollection_Count()
        {
            Assert.Equal(0, _dependencyBuilderAsServiceCollection.Count);
            _dependencyBuilder.AddScoped<IFirst, First.RequiresNone>();
            Assert.Equal(1, _dependencyBuilderAsServiceCollection.Count);
        }

        [Fact]
        public void ServiceCollection_IsReadOnly()
        {
            Assert.False(_dependencyBuilderAsServiceCollection.IsReadOnly);
        }

        [Fact]
        public void ServiceCollection_Index_Get_OutOfRange()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _dependencyBuilderAsServiceCollection[0]);
        }

        [Fact]
        public void ServiceCollection_Index_Get()
        {
            _dependencyBuilder.AddScoped<IFirst, First.RequiresNone>();
            var serviceDescriptor = _dependencyBuilderAsServiceCollection[0];

            Assert.Equal(typeof(IFirst), serviceDescriptor.ServiceType);
            Assert.Equal(typeof(First.RequiresNone), serviceDescriptor.ImplementationType);
        }

        [Fact]
        public void ServiceCollection_Index_Set_OutOfRange()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _dependencyBuilderAsServiceCollection[0] = null);
        }

        [Fact]
        public void ServiceCollection_IndexOf_NoMatch_NoItems()
        {
            var serviceDescriptor = new ServiceDescriptor(typeof(IFirst), typeof(First.RequiresNone), ServiceLifetime.Singleton);
            var index = _dependencyBuilderAsServiceCollection.IndexOf(serviceDescriptor);
            Assert.Equal(-1, index);
        }

        [Fact]
        public void ServiceCollection_IndexOf_NoMatch_MismatchingScope()
        {
            var serviceDescriptor = new ServiceDescriptor(typeof(IFirst), typeof(First.RequiresNone), ServiceLifetime.Singleton);
            _dependencyBuilder.AddScoped<IFirst, First.RequiresNone>();
            var index = _dependencyBuilderAsServiceCollection.IndexOf(serviceDescriptor);
            Assert.Equal(-1, index);
        }

        [Fact]
        public void ServiceCollection_IndexOf_NoMatch_MismatchingServiceType()
        {
            var serviceDescriptor = new ServiceDescriptor(typeof(IFirst), typeof(First.RequiresNone), ServiceLifetime.Singleton);
            _dependencyBuilder.AddScoped<IUniqueFirst, First.RequiresNone>();
            var index = _dependencyBuilderAsServiceCollection.IndexOf(serviceDescriptor);
            Assert.Equal(-1, index);
        }

        [Fact]
        public void ServiceCollection_IndexOf_NoMatch_MismatchingImplementationType()
        {
            var serviceDescriptor = new ServiceDescriptor(typeof(IFirst), typeof(First.RequiresNone), ServiceLifetime.Singleton);
            _dependencyBuilder.AddScoped<IFirst, First.IgnoreSecond>();
            var index = _dependencyBuilderAsServiceCollection.IndexOf(serviceDescriptor);
            Assert.Equal(-1, index);
        }

        [Fact]
        public void ServiceCollection_IndexOf_Found()
        {
            var serviceDescriptor = new ServiceDescriptor(typeof(IFirst), typeof(First.RequiresNone), ServiceLifetime.Scoped);
            _dependencyBuilder.AddScoped<IFirst, First.RequiresNone>();
            var index = _dependencyBuilderAsServiceCollection.IndexOf(serviceDescriptor);
            Assert.Equal(0, index);
        }

        [Fact]
        public void ServiceCollection_Insert_OutOfRange()
        {
            var serviceDescriptor = new ServiceDescriptor(typeof(IFirst), typeof(First.RequiresNone), ServiceLifetime.Singleton);
            Assert.Throws<ArgumentOutOfRangeException>(() => _dependencyBuilderAsServiceCollection.Insert(1, serviceDescriptor));
        }

        [Fact]
        public void ServiceCollection_Insert_Duplicate()
        {
            var serviceDescriptor = new ServiceDescriptor(typeof(IFirst), typeof(First.RequiresNone), ServiceLifetime.Singleton);
            _dependencyBuilder.AddSingleton<IFirst, First.RequiresNone>();
            Assert.Throws<InvalidOperationException>(() => _dependencyBuilderAsServiceCollection.Insert(0, serviceDescriptor));
        }

        [Fact]
        public void ServiceCollection_Insert_NotConstructible()
        {
            var serviceDescriptor = new ServiceDescriptor(typeof(IFirst), typeof(IUniqueFirst), ServiceLifetime.Singleton);
            Assert.Throws<ArgumentException>(() => _dependencyBuilderAsServiceCollection.Insert(0, serviceDescriptor));
        }

        [Fact]
        public void ServiceCollection_Insert_Success()
        {
            var serviceDescriptor = new ServiceDescriptor(typeof(IFirst), typeof(First.RequiresNone), ServiceLifetime.Singleton);
            _dependencyBuilderAsServiceCollection.Insert(0, serviceDescriptor);
            Assert.Equal(0, _dependencyBuilderAsServiceCollection.IndexOf(serviceDescriptor));
        }

        [Fact]
        public void ServiceCollection_RemoveAt_OutOfRange()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _dependencyBuilderAsServiceCollection.RemoveAt(0));
        }

        [Fact]
        public void ServiceCollection_RemoveAt_Success()
        {
            _dependencyBuilder.AddSingleton<IFirst, First.RequiresNone>();
            _dependencyBuilderAsServiceCollection.RemoveAt(0);
            Assert.Equal(0, _dependencyBuilderAsServiceCollection.Count);
        }

        [Fact]
        public void ServiceCollection_Add_Duplicate()
        {
            var serviceDescriptor = new ServiceDescriptor(typeof(IFirst), typeof(First.RequiresNone), ServiceLifetime.Singleton);
            _dependencyBuilder.AddSingleton<IFirst, First.RequiresNone>();
            Assert.Throws<InvalidOperationException>(() => _dependencyBuilderAsServiceCollection.Add(serviceDescriptor));
        }

        [Fact]
        public void ServiceCollection_Add_NotConstructible()
        {
            var serviceDescriptor = new ServiceDescriptor(typeof(IFirst), typeof(IUniqueFirst), ServiceLifetime.Singleton);
            Assert.Throws<ArgumentException>(() => _dependencyBuilderAsServiceCollection.Add(serviceDescriptor));
        }

        [Fact]
        public void ServiceCollection_Add_Success()
        {
            var serviceDescriptor = new ServiceDescriptor(typeof(IFirst), typeof(First.RequiresNone), ServiceLifetime.Singleton);
            _dependencyBuilderAsServiceCollection.Add(serviceDescriptor);
            Assert.Equal(0, _dependencyBuilderAsServiceCollection.IndexOf(serviceDescriptor));
        }

        [Fact]
        public void ServiceCollection_Clear()
        {
            _dependencyBuilder.AddSingleton<IFirst, First.RequiresNone>();
            _dependencyBuilderAsServiceCollection.Clear();
            Assert.Equal(0, _dependencyBuilderAsServiceCollection.Count);
        }

        [Fact]
        public void ServiceCollection_Contains_NoMatch_NoItems()
        {
            var serviceDescriptor = new ServiceDescriptor(typeof(IFirst), typeof(First.RequiresNone), ServiceLifetime.Singleton);
            var contains = _dependencyBuilderAsServiceCollection.Contains(serviceDescriptor);
            Assert.False(contains);
        }

        [Fact]
        public void ServiceCollection_Contains_NoMatch_MismatchingScope()
        {
            var serviceDescriptor = new ServiceDescriptor(typeof(IFirst), typeof(First.RequiresNone), ServiceLifetime.Singleton);
            _dependencyBuilder.AddScoped<IFirst, First.RequiresNone>();
            var contains = _dependencyBuilderAsServiceCollection.Contains(serviceDescriptor);
            Assert.False(contains);
        }

        [Fact]
        public void ServiceCollection_Contains_NoMatch_MismatchingServiceType()
        {
            var serviceDescriptor = new ServiceDescriptor(typeof(IFirst), typeof(First.RequiresNone), ServiceLifetime.Singleton);
            _dependencyBuilder.AddScoped<IUniqueFirst, First.RequiresNone>();
            var contains = _dependencyBuilderAsServiceCollection.Contains(serviceDescriptor);
            Assert.False(contains);
        }

        [Fact]
        public void ServiceCollection_Contains_NoMatch_MismatchingImplementationType()
        {
            var serviceDescriptor = new ServiceDescriptor(typeof(IFirst), typeof(First.RequiresNone), ServiceLifetime.Singleton);
            _dependencyBuilder.AddScoped<IFirst, First.IgnoreSecond>();
            var contains = _dependencyBuilderAsServiceCollection.Contains(serviceDescriptor);
            Assert.False(contains);
        }

        [Fact]
        public void ServiceCollection_Contains_Found()
        {
            var serviceDescriptor = new ServiceDescriptor(typeof(IFirst), typeof(First.RequiresNone), ServiceLifetime.Scoped);
            _dependencyBuilder.AddScoped<IFirst, First.RequiresNone>();
            var contains = _dependencyBuilderAsServiceCollection.Contains(serviceDescriptor);
            Assert.True(contains);
        }

        [Fact]
        public void ServiceCollection_CopyTo()
        {
            _dependencyBuilder.AddScoped<IFirst, First.RequiresNone>();
            var target = new ServiceDescriptor[1];
            _dependencyBuilderAsServiceCollection.CopyTo(target, 0);

            Assert.Collection(
                target,
                x =>
                {
                    Assert.Equal(typeof(IFirst), x.ServiceType);
                    Assert.Equal(typeof(First.RequiresNone), x.ImplementationType);
                    Assert.Equal(ServiceLifetime.Scoped, x.Lifetime);
                });
        }

        [Fact]
        public void ServiceCollection_Remove_NoMatch()
        {
            var serviceDescriptor = new ServiceDescriptor(typeof(IFirst), typeof(First.RequiresNone), ServiceLifetime.Scoped);
            var result = _dependencyBuilderAsServiceCollection.Remove(serviceDescriptor);
            Assert.False(result);
        }

        [Fact]
        public void ServiceCollection_Remove_Exists()
        {
            var serviceDescriptor = new ServiceDescriptor(typeof(IFirst), typeof(First.RequiresNone), ServiceLifetime.Scoped);
            _dependencyBuilder.AddScoped<IFirst, First.RequiresNone>();
            var result = _dependencyBuilderAsServiceCollection.Remove(serviceDescriptor);
            Assert.True(result);
            Assert.Equal(0, _dependencyBuilderAsServiceCollection.Count);
        }

        [Fact]
        public void ServiceCollection_GetEnumerator_Object()
        {
            _dependencyBuilder.AddScoped<IFirst, First.RequiresNone>();
            var enumerable = (IEnumerable)_dependencyBuilder;

            Assert.Collection(
                enumerable.Cast<object>(),
                x =>
                {
                    var serviceDescriptor = Assert.IsType<ServiceDescriptor>(x);
                    Assert.Equal(typeof(IFirst), serviceDescriptor.ServiceType);
                    Assert.Equal(typeof(First.RequiresNone), serviceDescriptor.ImplementationType);
                    Assert.Equal(ServiceLifetime.Scoped, serviceDescriptor.Lifetime);
                });
        }

        [Fact]
        public void ServiceCollection_GetEnumerator_Typed()
        {
            _dependencyBuilder.AddScoped<IFirst, First.RequiresNone>();
            var enumerable = (IEnumerable<ServiceDescriptor>)_dependencyBuilder;

            Assert.Collection(
                enumerable,
                x =>
                {
                    Assert.Equal(typeof(IFirst), x.ServiceType);
                    Assert.Equal(typeof(First.RequiresNone), x.ImplementationType);
                    Assert.Equal(ServiceLifetime.Scoped, x.Lifetime);
                });
        }
    }
}
