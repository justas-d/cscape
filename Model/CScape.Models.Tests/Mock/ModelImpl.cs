namespace CScape.Models.Tests.Mock
{
    public static class ModelImpl
    {
        public static IModelImplementation Active { get; } = new CoreModelImpl();
    }
}