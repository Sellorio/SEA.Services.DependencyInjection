using SEA.DependencyInjection.Configuration;
using SEA.DependencyInjection.Tests.TestServices;
using System;
using Xunit;

namespace SEA.DependencyInjection.Tests
{
    public class DependencyServiceTestsForScoped
    {
        [Fact]
        public void Get_Disposed()
        {
            var dependencyService = new DependencyBuilder().Build().CreateScope();
            dependencyService.Dispose();

            Assert.Throws<ObjectDisposedException>(() => dependencyService.Get<IFirst>());
        }

        [Fact]
        public void Get_ByType_Disposed()
        {
            var dependencyService = new DependencyBuilder().Build().CreateScope();
            dependencyService.Dispose();

            Assert.Throws<ObjectDisposedException>(() => dependencyService.Get(typeof(IFirst)));
        }

        [Fact]
        public void Get_Fail_ScopedRequiresNotRegistered()
        {
            var dependencyService =
                new DependencyBuilder()
                    .AddScoped<IFirst, First.RequiresSecond>()
                    .Build().CreateScope();

            Assert.Throws<InvalidOperationException>(() => dependencyService.Get<IFirst>());
        }

        [Fact]
        public void Get_Fail_ScopedRequiresFirst()
        {
            var dependencyService =
                new DependencyBuilder()
                    .AddScoped<IFirst, First.RequiresFirst>()
                    .Build().CreateScope();

            Assert.Throws<InvalidOperationException>(() => dependencyService.Get<IFirst>());
        }

        [Fact]
        public void Get_Fail_ScopedRequiresScopedRequiresFirst()
        {
            var dependencyService =
                new DependencyBuilder()
                    .AddScoped<IFirst, First.RequiresSecond>()
                    .AddScoped<ISecond, Second.RequiresFirst>()
                    .Build().CreateScope();

            Assert.Throws<InvalidOperationException>(() => dependencyService.Get<IFirst>());
        }

        [Fact]
        public void Get_Success_ScopedRequiresSingleton()
        {
            var dependencyService =
                new DependencyBuilder()
                    .AddScoped<IFirst, First.RequiresSecond>()
                    .AddSingleton<ISecond, Second.RequiresNone>()
                    .Build().CreateScope();

            var service = dependencyService.Get<IFirst>();

            Assert.NotNull(service);
            var first = Assert.IsType<First.RequiresSecond>(service);
            Assert.NotNull(first.Second);
            Assert.IsType<Second.RequiresNone>(first.Second);
        }

        [Fact]
        public void Get_Success_ScopedRequiresTransientRequiresSingleton()
        {
            var dependencyService =
                new DependencyBuilder()
                    .AddScoped<IFirst, First.RequiresSecond>()
                    .AddTransient<ISecond, Second.RequiresThird>()
                    .AddSingleton<IThird, Third.RequiresNone>()
                    .Build().CreateScope();

            var service = dependencyService.Get<IFirst>();

            Assert.NotNull(service);
            var first = Assert.IsType<First.RequiresSecond>(service);
            Assert.NotNull(first.Second);
            var second = Assert.IsType<Second.RequiresThird>(first.Second);
            Assert.IsType<Third.RequiresNone>(second.Third);
        }

        [Fact]
        public void Get_Success_ScopedRequiresScoped()
        {
            var dependencyService =
                new DependencyBuilder()
                    .AddScoped<IFirst, First.RequiresSecond>()
                    .AddScoped<ISecond, Second.RequiresNone>()
                    .Build().CreateScope();

            var service = dependencyService.Get<IFirst>();

            Assert.NotNull(service);
            var first = Assert.IsType<First.RequiresSecond>(service);
            Assert.NotNull(first.Second);
            Assert.IsType<Second.RequiresNone>(first.Second);
        }

        [Fact]
        public void Get_Success_TransientRequiresSingleton()
        {
            var dependencyService =
                new DependencyBuilder()
                    .AddTransient<IFirst, First.RequiresSecond>()
                    .AddSingleton<ISecond, Second.RequiresNone>()
                    .Build().CreateScope();

            var service = dependencyService.Get<IFirst>();

            Assert.NotNull(service);
            var first = Assert.IsType<First.RequiresSecond>(service);
            Assert.NotNull(first.Second);
            Assert.IsType<Second.RequiresNone>(first.Second);
        }

        [Fact]
        public void Get_Success_ScopedRequiresScopedFunction()
        {
            var dependencyService =
                new DependencyBuilder()
                    .AddScoped<IFirst, First.RequiresSecond>()
                    .AddScoped<ISecond>(x => new Second.RequiresNone())
                    .Build().CreateScope();

            var service = dependencyService.Get<IFirst>();

            Assert.NotNull(service);
            var first = Assert.IsType<First.RequiresSecond>(service);
            Assert.NotNull(first.Second);
            Assert.IsType<Second.RequiresNone>(first.Second);
        }

        [Fact]
        public void Get_Success_ScopedRequiresTransient()
        {
            var dependencyService =
                new DependencyBuilder()
                    .AddScoped<IFirst, First.RequiresSecond>()
                    .AddTransient<ISecond, Second.RequiresNone>()
                    .Build().CreateScope();

            var service = dependencyService.Get<IFirst>();

            Assert.NotNull(service);
            var first = Assert.IsType<First.RequiresSecond>(service);
            Assert.NotNull(first.Second);
            Assert.IsType<Second.RequiresNone>(first.Second);
        }

        [Fact]
        public void Get_Success_ScopedRequiresTransientFunction()
        {
            var dependencyService =
                new DependencyBuilder()
                    .AddScoped<IFirst, First.RequiresSecond>()
                    .AddTransient<ISecond>(x => new Second.RequiresNone())
                    .Build().CreateScope();

            var service = dependencyService.Get<IFirst>();

            Assert.NotNull(service);
            var first = Assert.IsType<First.RequiresSecond>(service);
            Assert.NotNull(first.Second);
            Assert.IsType<Second.RequiresNone>(first.Second);
        }

        [Fact]
        public void Get_Success_ScopedIgnoresSecond()
        {
            var dependencyService =
                new DependencyBuilder()
                    .AddScoped<IFirst, First.IgnoreSecond>()
                    .Build().CreateScope();

            var service = dependencyService.Get<IFirst>();

            Assert.NotNull(service);
            Assert.IsType<First.IgnoreSecond>(service);
        }

        [Fact]
        public void Get_Success_ScopedAutoDetected()
        {
            var dependencyService =
                new DependencyBuilder()
                    .EnableServiceAutoDetection(GetType().Assembly)
                    .Build().CreateScope();

            var service = dependencyService.Get<IUniqueFirst>();

            Assert.NotNull(service);
            Assert.IsType<First.RequiresNone>(service);
        }

        [Fact]
        public void Get_Success_ScopedRequiresScopedAutoDetected()
        {
            var dependencyService =
                new DependencyBuilder()
                    .EnableServiceAutoDetection(GetType().Assembly)
                    .AddScoped<IFirst, First.RequiresUniqueSecond>()
                    .Build().CreateScope();

            var service = dependencyService.Get<IFirst>();

            Assert.NotNull(service);
            var first = Assert.IsType<First.RequiresUniqueSecond>(service);
            Assert.NotNull(first.UniqueSecond);
            Assert.IsType<Second.RequiresNone>(first.UniqueSecond);
        }

        [Fact]
        public void Get_Success_ScopedRequiresSameSingletonAsSingleton()
        {
            var dependencyService =
                new DependencyBuilder()
                    .EnableServiceAutoDetection(GetType().Assembly)
                    .AddScoped<IFirst, First.RequiresSecondAndThird>()
                    .AddScoped<ISecond, Second.RequiresThird>()
                    .AddSingleton<IThird, Third.RequiresNone>()
                    .Build().CreateScope();

            var service = dependencyService.Get<IFirst>();

            Assert.NotNull(service);
            var first = Assert.IsType<First.RequiresSecondAndThird>(service);
            Assert.NotNull(first.Second);
            var second = Assert.IsType<Second.RequiresThird>(first.Second);
            Assert.NotNull(first.Third);
            Assert.IsType<Third.RequiresNone>(first.Third);

            Assert.Same(first.Third, second.Third);
        }

        [Fact]
        public void Get_Success_ScopedRequiresSameTransientAsScoped()
        {
            var dependencyService =
                new DependencyBuilder()
                    .EnableServiceAutoDetection(GetType().Assembly)
                    .AddScoped<IFirst, First.RequiresSecondAndThird>()
                    .AddScoped<ISecond, Second.RequiresThird>()
                    .AddTransient<IThird, Third.RequiresNone>()
                    .Build().CreateScope();

            var service = dependencyService.Get<IFirst>();

            Assert.NotNull(service);
            var first = Assert.IsType<First.RequiresSecondAndThird>(service);
            Assert.NotNull(first.Second);
            var second = Assert.IsType<Second.RequiresThird>(first.Second);
            Assert.NotNull(first.Third);
            Assert.IsType<Third.RequiresNone>(first.Third);

            Assert.NotSame(first.Third, second.Third);
        }

        [Fact]
        public void Get_Success_TransientRequiresScoped()
        {
            var dependencyService =
                new DependencyBuilder()
                    .EnableServiceAutoDetection(GetType().Assembly)
                    .AddTransient<IFirst, First.RequiresSecond>()
                    .AddScoped<ISecond, Second.RequiresNone>()
                    .Build().CreateScope();

            var service = dependencyService.Get<IFirst>();

            Assert.NotNull(service);
            var first = Assert.IsType<First.RequiresSecond>(service);
            Assert.NotNull(first.Second);
            Assert.IsType<Second.RequiresNone>(first.Second);
        }

        [Fact]
        public void Get_Success_ConstructorCompatabilityMode()
        {
            var dependencyBuilder = (IDependencyBuilder)new DependencyBuilder();
            dependencyBuilder.Add(new Microsoft.Extensions.DependencyInjection.ServiceDescriptor(typeof(IFirst), typeof(First.RequiresSecondInConstructor), Microsoft.Extensions.DependencyInjection.ServiceLifetime.Scoped));
            dependencyBuilder.AddScoped<ISecond, Second.RequiresNone>();
            var dependencyService = dependencyBuilder.Build().CreateScope();

            var service = dependencyService.Get<IFirst>();

            Assert.NotNull(service);
            var first = Assert.IsType<First.RequiresSecondInConstructor>(service);
            Assert.NotNull(first.Second);
            Assert.IsType<Second.RequiresNone>(first.Second);
        }

        [Fact]
        public void Dispose_Success_DisposableScoped()
        {
            var dependencyService =
                new DependencyBuilder()
                    .EnableServiceAutoDetection(GetType().Assembly)
                    .AddScoped<IFirst, First.RequiresNone>()
                    .Build().CreateScope();

            var service = dependencyService.Get<IFirst>();

            Assert.NotNull(service);
            var first = Assert.IsType<First.RequiresNone>(service);

            dependencyService.Dispose();

            Assert.True(first.IsDisposed);
        }

        [Fact]
        public void Dispose_Success_DisposableScoped_SuppressesExceptions()
        {
            var dependencyService =
                new DependencyBuilder()
                    .EnableServiceAutoDetection(GetType().Assembly)
                    .AddScoped<IFirst, First.RequiresNone>()
                    .Build().CreateScope();

            var service = dependencyService.Get<IFirst>();

            Assert.NotNull(service);
            var first = Assert.IsType<First.RequiresNone>(service);
            first.Dispose();

            dependencyService.Dispose();
        }

        [Fact]
        public void Create_Standard()
        {
            var dependencyService =
                new DependencyBuilder()
                    .AddScoped<ISecond, Second.RequiresNone>()
                    .Build().CreateScope();

            var service = dependencyService.Create<First.RequiresSecond>();

            Assert.NotNull(service);
            var first = Assert.IsType<First.RequiresSecond>(service);
            Assert.NotNull(first.Second);
            Assert.IsType<Second.RequiresNone>(first.Second);
        }

        [Fact]
        public void Create_AutoDetect()
        {
            var dependencyService = new DependencyBuilder().EnableServiceAutoDetection(GetType().Assembly).Build().CreateScope();
            var service = dependencyService.Create<First.RequiresUniqueSecond>();

            Assert.NotNull(service);
            var first = Assert.IsType<First.RequiresUniqueSecond>(service);
            Assert.NotNull(first.UniqueSecond);
            Assert.IsType<Second.RequiresNone>(first.UniqueSecond);
        }
    }
}
