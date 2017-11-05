using System.Diagnostics;
using System.Net;
using System.Runtime.CompilerServices;
using CScape.Models.Data;

namespace CScape.Core.Extensions
{
    public static class ConfigurationServiceExtensions
    {

        [DebuggerStepThrough]
        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IPEndPoint GetIpAddress(this IConfigurationService config, string key)
        {
            var data = config.Get(key).Split(':');
            var ip = data[0];
            var port = data[1];
            return new IPEndPoint(IPAddress.Parse(ip), int.Parse(port));
        }
    }
}
