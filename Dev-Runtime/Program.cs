namespace CScape.Dev.Runtime
{
    public static class Program
    {
        public static void Main()
        {
            var ctx = new ServerContext();
            ctx.Start();
        }
    }
}