using System;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR;

namespace Flare
{
    class Program
    {
        static void Main(string[] args)
        {
            var host = WebHost
                        .CreateDefaultBuilder()
                        .UseKestrel()
                        .UseStartup<Startup>()
                        .Build();
            host.Run();
        }
    }
}
