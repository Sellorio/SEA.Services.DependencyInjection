using SEA.DependencyInjection.Configuration;
using SEA.DependencyInjection.Tests.TestServices;
using System;
using Xunit;

namespace SEA.DependencyInjection.Tests
{
    public class DependencyServiceTestsForSingleton
    {
        [Fact]
        public void Get_Disposed()
        {
            var dependencyService = new DependencyBuilder().Build();
            dependencyService.Dispose();

            Assert.Throws<ObjectDisposedException>(() => dependencyService.Get<IFirst>());
        }

        [Fact]
        public void Get_ByType_Disposed()
        {
            var dependencyService = new DependencyBuilder().Build();
            dependencyService.Dispose();

            Assert.Throws<ObjectDisposedException>(() => dependencyService.Get(typeof(IFirst)));
        }

        [Fact]
        public void Get_Fail_SingletonRequiresNotRegistered()
        {
            var dependencyService =
                new DependencyBuilder()
                    .AddSingleton<IFirst, First.RequiresSecond>()
                    .Build();

            Assert.Throws<InvalidOperationException>(() => dependencyService.Get<IFirst>());
        }

        [Fact]
        public void Get_Fail_SingletonRequiresScoped()
        {
            var dependencyService =
                new DependencyBuilder()
                    .AddSingleton<IFirst, First.RequiresSecond>()
                    .AddScoped<ISecond, Second.RequiresNone>()
                    .Build();

            Assert.Throws<InvalidOperationException>(() => dependencyService.Get<IFirst>());
        }

        [Fact]
        public void Get_Fail_SingletonRequiresTransientRequiresScoped()
        {
            var dependencyService =
                new DependencyBuilder()
                    .AddSingleton<IFirst, First.RequiresSecond>()
                    .AddTransient<ISecond, Second.RequiresThird>()
                    .AddScoped<IThird, Third.RequiresNone>()
                    .Build();

            Assert.Throws<InvalidOperationException>(() => dependencyService.Get<IFirst>());
        }

        [Fact]
        public void Get_Fail_SingletonRequiresFirst()
        {
            var dependencyService =
                new DependencyBuilder()
                    .AddSingleton<IFirst, First.RequiresFirst>()
                    .Build();

            Assert.Throws<InvalidOperationException>(() => dependencyService.Get<IFirst>());
        }

        [Fact]
        public void Get_Fail_SingletonRequiresSingletonRequiresFirst()
        {
            var dependencyService =
                new DependencyBuilder()
                    .AddSingleton<IFirst, First.RequiresSecond>()
                    .AddSingleton<ISecond, Second.RequiresFirst>()
                    .Build();

            Assert.Throws<InvalidOperationException>(() => dependencyService.Get<IFirst>());
        }

        [Fact]
        public void Get_Fail_TransientRequiresScoped()
        {
            var dependencyService =
                new DependencyBuilder()
                    .AddTransient<IFirst, First.RequiresSecond>()
                    .AddScoped<ISecond, Second.RequiresNone>()
                    .Build();

            Assert.Throws<InvalidOperationException>(() => dependencyService.Get<IFirst>());
        }

        [Fact]
        public void Get_Success_SingletonRequiresSingleton()
        {
            var dependencyService =
                new DependencyBuilder()
                    .AddSingleton<IFirst, First.RequiresSecond>()
                    .AddSingleton<ISecond, Second.RequiresNone>()
                    .Build();

            var service = dependencyService.Get<IFirst>();

            Assert.NotNull(service);
            var second = Assert.IsType<First.RequiresSecond>(service);
            Assert.NotNull(second.Second);
            Assert.IsType<Second.RequiresNone>(second.Second);
        }

        [Fact]
        public void Get_Success_SingletonRequiresSingletonInstance()
        {
            var dependencyService =
                new DependencyBuilder()
                    .AddSingleton<IFirst, First.RequiresSecond>()
                    .AddSingleton<ISecond>(new Second.RequiresNone())
                    .Build();

            var service = dependencyService.Get<IFirst>();

            Assert.NotNull(service);
            var second = Assert.IsType<First.RequiresSecond>(service);
            Assert.NotNull(second.Second);
            Assert.IsType<Second.RequiresNone>(second.Second);
        }

        [Fact]
        public void Get_Success_SingletonRequiresTransient()
        {
            var dependencyService =
                new DependencyBuilder()
                    .AddSingleton<IFirst, First.RequiresSecond>()
                    .AddTransient<ISecond, Second.RequiresNone>()
                    .Build();

            var service = dependencyService.Get<IFirst>();

            Assert.NotNull(service);
            var first = Assert.IsType<First.RequiresSecond>(service);
            Assert.NotNull(first.Second);
            Assert.IsType<Second.RequiresNone>(first.Second);
        }

        [Fact]
        public void Get_Success_SingletonRequiresTransientFunction()
        {
            var dependencyService =
                new DependencyBuilder()
                    .AddSingleton<IFirst, First.RequiresSecond>()
                    .AddTransient<ISecond>(x => new Second.RequiresNone())
                    .Build();

            var service = dependencyService.Get<IFirst>();

            Assert.NotNull(service);
            var first = Assert.IsType<First.RequiresSecond>(service);
            Assert.NotNull(first.Second);
            Assert.IsType<Second.RequiresNone>(first.Second);
        }

        [Fact]
        public void Get_Success_SingletonIgnoresSecond()
        {
            var dependencyService =
                new DependencyBuilder()
                    .AddSingleton<IFirst, First.IgnoreSecond>()
                    .Build();

            var service = dependencyService.Get<IFirst>();

            Assert.NotNull(service);
            Assert.IsType<First.IgnoreSecond>(service);
        }

        [Fact]
        public void Get_Success_SingletonAutoDetected()
        {
            var dependencyService =
                new DependencyBuilder()
                    .EnableServiceAutoDetection(GetType().Assembly)
                    .Build();

            var service = dependencyService.Get<IUniqueFirst>();

            Assert.NotNull(service);
            Assert.IsType<First.RequiresNone>(service);
        }

        [Fact]
        public void Get_Success_SingletonRequiresSingletonAutoDetected()
        {
            var dependencyService =
                new DependencyBuilder()
                    .EnableServiceAutoDetection(GetType().Assembly)
                    .AddSingleton<IFirst, First.RequiresUniqueSecond>()
                    .Build();

            var service = dependencyService.Get<IFirst>();

            Assert.NotNull(service);
            var first = Assert.IsType<First.RequiresUniqueSecond>(service);
            Assert.NotNull(first.UniqueSecond);
            Assert.IsType<Second.RequiresNone>(first.UniqueSecond);
        }

        [Fact]
        public void Get_Success_SingletonRequiresSameSingletonAsSingleton()
        {
            var dependencyService =
                new DependencyBuilder()
                    .EnableServiceAutoDetection(GetType().Assembly)
                    .AddSingleton<IFirst, First.RequiresSecondAndThird>()
                    .AddSingleton<ISecond, Second.RequiresThird>()
                    .AddSingleton<IThird, Third.RequiresNone>()
                    .Build();

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
        public void Get_Success_SingletonRequiresSameTransientAsSingleton()
        {
            var dependencyService =
                new DependencyBuilder()
                    .EnableServiceAutoDetection(GetType().Assembly)
                    .AddSingleton<IFirst, First.RequiresSecondAndThird>()
                    .AddSingleton<ISecond, Second.RequiresThird>()
                    .AddTransient<IThird, Third.RequiresNone>()
                    .Build();

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
        public void Get_Success_TransientRequiresSingleton()
        {
            var dependencyService =
                new DependencyBuilder()
                    .EnableServiceAutoDetection(GetType().Assembly)
                    .AddTransient<IFirst, First.RequiresSecond>()
                    .AddSingleton<ISecond, Second.RequiresNone>()
                    .Build();

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
            dependencyBuilder.Add(new Microsoft.Extensions.DependencyInjection.ServiceDescriptor(typeof(IFirst), typeof(First.RequiresSecondInConstructor), Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton));
            dependencyBuilder.AddSingleton<ISecond, Second.RequiresNone>();
            var dependencyService = dependencyBuilder.Build();

            var service = dependencyService.Get<IFirst>();

            Assert.NotNull(service);
            var first = Assert.IsType<First.RequiresSecondInConstructor>(service);
            Assert.NotNull(first.Second);
            Assert.IsType<Second.RequiresNone>(first.Second);
        }

        [Fact]
        public void Dispose_Success_DisposableSingleton()
        {
            var dependencyService =
                new DependencyBuilder()
                    .EnableServiceAutoDetection(GetType().Assembly)
                    .AddSingleton<IFirst, First.RequiresNone>()
                    .Build();

            var service = dependencyService.Get<IFirst>();

            Assert.NotNull(service);
            var first = Assert.IsType<First.RequiresNone>(service);

            dependencyService.Dispose();

            Assert.True(first.IsDisposed);
        }

        [Fact]
        public void Dispose_Success_DisposableSingleton_SuppressesExceptions()
        {
            var dependencyService =
                new DependencyBuilder()
                    .EnableServiceAutoDetection(GetType().Assembly)
                    .AddSingleton<IFirst, First.RequiresNone>()
                    .Build();

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
                    .AddSingleton<ISecond, Second.RequiresNone>()
                    .Build();

            var service = dependencyService.Create<First.RequiresSecond>();

            Assert.NotNull(service);
            var first = Assert.IsType<First.RequiresSecond>(service);
            Assert.NotNull(first.Second);
            Assert.IsType<Second.RequiresNone>(first.Second);
        }

        [Fact]
        public void Create_AutoDetect()
        {
            var dependencyService = new DependencyBuilder().EnableServiceAutoDetection(GetType().Assembly).Build();
            var service = dependencyService.Create<First.RequiresUniqueSecond>();

            Assert.NotNull(service);
            var first = Assert.IsType<First.RequiresUniqueSecond>(service);
            Assert.NotNull(first.UniqueSecond);
            Assert.IsType<Second.RequiresNone>(first.UniqueSecond);
        }
    }
}
