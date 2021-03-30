using System;

namespace SEA.DependencyInjection.Tests.TestServices
{
    interface IFirst
    {
    }

    interface IUniqueFirst : IFirst
    {
    }

    static class First
    {
        public class RequiresNone : IUniqueFirst, IDisposable
        {
            public bool IsDisposed { get; private set; }

            public void Dispose()
            {
                if (IsDisposed)
                {
                    throw new ObjectDisposedException(nameof(RequiresNone));
                }

                IsDisposed = true;
            }
        }

        public class RequiresSecond : IFirst
        {
            public ISecond Second { get; init; }
        }

        public class RequiresUniqueSecond : IFirst
        {
            public IUniqueSecond UniqueSecond { get; init; }
        }

        public class RequiresSecondAndThird : IFirst
        {
            public ISecond Second { get; init; }
            public IThird Third { get; init; }
        }

        public class IgnoreSecond : IFirst
        {
            [NotInjected]
            public ISecond Second { get; init; }
        }

        public class RequiresFirst : IFirst
        {
            public IFirst First { get; init; }
        }
    }
}
