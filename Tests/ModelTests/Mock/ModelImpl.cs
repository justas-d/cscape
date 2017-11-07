namespace CScape.Dev.Tests.ModelTests.Mock
{
    public static class ModelImpl
    {
        public static IModelImplementation Active { get; } = new CoreModelImpl();
    }
}