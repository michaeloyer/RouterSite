using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Dapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace RouterSite
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public static async Task Main(string[] args)
        {
            using var host = Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .Build();

            var routeService = host.Services.GetRequiredService<RouteService>();

            await routeService.CreateDbIfMissing();
            await host.RunAsync();
        }

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages()
                .AddRazorRuntimeCompilation();

            services.AddControllers();

            services.AddSingleton(new RouteService(new SqliteConnection(_configuration.GetConnectionString("Database"))));
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }

            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers();

                endpoints.MapGet("/", context =>
                {
                    context.Response.Redirect("/routes");
                    return Task.CompletedTask;
                });

                endpoints.MapFallback(async context =>
                {
                    var routeService = app.ApplicationServices.GetRequiredService<RouteService>();

                    (var success, var destination) = await routeService.GetDestination(context.Request.Path);

                    if (success)
                        context.Response.Redirect(destination);
                    else
                        context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                });
            });
        }
    }
}
