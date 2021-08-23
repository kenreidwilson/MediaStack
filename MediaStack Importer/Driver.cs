using System;
using System.IO;
using System.IO.Abstractions;
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

            var logger = loggerFactory.CreateLogger<Driver>();

            await new MediaImporter(
                logger,
                new FileSystemController(new FileSystem(), logger, ""),
                new UnitOfWorkService()
            ).Start();
        }

        #endregion
    }
}
