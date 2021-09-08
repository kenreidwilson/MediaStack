using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using MediaStackCore.Utility;
using Microsoft.Extensions.Configuration;

namespace MediaStack_API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            EnvParser.ParseEnvFromFile($@"{Environment.CurrentDirectory}{Path.DirectorySeparatorChar}.env");
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseConfiguration(
                        new ConfigurationBuilder()
                            .AddCommandLine(args)
                            .Build()
                    );
                    webBuilder.UseStartup<Startup>();
                });
    }
}
