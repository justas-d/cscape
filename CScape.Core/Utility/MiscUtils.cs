using System.IO;
using System.Reflection;

namespace CScape.Core.Utility
{
    public static class MiscUtils
    {
        public static string GetExeDir()
        {
            return Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        }
    }
}
