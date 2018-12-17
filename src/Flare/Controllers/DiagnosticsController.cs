using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Flare.Controllers
{
    [Produces("application/json")]
    public class DiagnosticsController : Controller
    {
        [HttpGet]
        public async Task<TimeSpan> UpTime()
        {
            return await Task.Run<TimeSpan>( () => 
                {
                    var now = DateTime.UtcNow;
                    var beginning = Process.GetCurrentProcess().StartTime.ToUniversalTime();
                    var uptime = now.Subtract(beginning);

                    return uptime;
                });
        }

        [HttpGet]
        public async Task<string> Version()
        {
            //https://developers.de/blogs/damir_dobric/archive/2017/06/27/how-to-deal-with-assembly-version-in-net-core.aspx
            return await Task.Run<string>(() => 
            {
                var ver = Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
                var attrs = typeof(Startup).GetTypeInfo().Assembly.GetCustomAttributes(); 
                var runtimeVersion = typeof(Startup).GetTypeInfo().Assembly.GetCustomAttribute<AssemblyFileVersionAttribute>(); 
                var tokens = runtimeVersion.Version.Split('.'); 
                return new Version(runtimeVersion.Version).ToString();
            });
        }
    }
}