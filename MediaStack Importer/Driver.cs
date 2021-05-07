using System;
using System.IO;
using System.Threading.Tasks;
using MediaStack_Importer.Importer;
using MediaStackCore.Controllers;
using MediaStackCore.Data_Access_Layer;
using MediaStackCore.Services.UnitOfWorkService;
using MediaStackCore.Utility;
using Microsoft.Extensions.Logging;

namespace MediaStack_Importer
{
    internal class Driver
    {
        #region Methods

        private static async Task Main(string[] args)
        {
            new MediaStackContext();
            EnvParser.ParseEnvFromFile($@"{Environment.CurrentDirectory}{Path.DirectorySeparatorChar}.env");

            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder
                    //.SetMinimumLevel(LogLevel.Debug)
                    .AddConsole();
            });

            await new MediaImporter(
                loggerFactory.CreateLogger<Driver>(),
                new MediaFSController(),
                new UnitOfWorkService()
            ).Start();
        }

        #endregion
    }
}
