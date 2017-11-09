namespace CScape.Core.Runtime
{
    public static class Program
    {
        public static void Main()
        {
            var ctx = new ServerContext();
            ctx.RunBlocking();
        }
    }
}