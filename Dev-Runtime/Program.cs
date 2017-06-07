using CScape.Basic.Cache;

namespace CScape.Dev.Runtime
{
    public static class Program
    {
        public static void Main()
        {
            var data = new ClientDataReader(@"C:\Users\no\cache");
            var folder = data.GetFolder(0, 2);
            var file = folder.GetFile("obj.dat");

            var ctx = new ServerContext();
            ctx.RunBlocking();
        }
    }
}