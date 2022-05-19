using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CliCommandCreator.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CliCommandCreator
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            using (var servicescope = host.Services.CreateScope())
            {
                var serviceProvider = servicescope.ServiceProvider;
                try
                {
                    SeedData.Initialize(serviceProvider);
                }
                catch(Exception ex)
                {
                    throw ex;
                }
            }
            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
