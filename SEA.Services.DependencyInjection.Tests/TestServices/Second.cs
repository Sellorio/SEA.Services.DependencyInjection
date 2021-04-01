namespace SEA.DependencyInjection.Tests.TestServices
{
    interface ISecond
    {
    }

    interface IUniqueSecond : ISecond
    {
    }

    static class Second
    {
        public class RequiresNone : IUniqueSecond
        {
        }

        public class RequiresFirst : ISecond
        {
            public IFirst First { get; init; }
        }

        public class RequiresThird : ISecond
        {
            public IThird Third { get; init; }
        }
    }
}
