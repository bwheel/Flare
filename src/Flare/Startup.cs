

using System;
using System.IO;
using Flare.Hubs;
using Flare.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Flare
{
    public class Startup
    {
        private IConfiguration Configuration {get;}
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHostedService<PollRunner>();
            services.AddSingleton<IRunner, PollRunner>();
            services.AddMvc();
            services.AddSignalR();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if(env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles(); // wwwroot

            app.UseMvc(routes => {
                routes.MapRoute(
                    name: "default",
                    template: "api/{controller}/{action}/{id?}"
                );
            });

            app.UseSignalR( routes => {
                routes.MapHub<FlareHub>("/streams/flare");
            });

            app.Run(async (context) =>
            {
                await context.Response.WriteAsync("Hello World");
            });
        }
    }
}