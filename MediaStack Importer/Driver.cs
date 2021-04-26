using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using MediaStack_Importer.Importer;
using MediaStackCore.Controllers;
using MediaStackCore.Data_Access_Layer;
using MediaStackCore.Services.UnitOfWorkService;
using MediaStackCore.Utility;

namespace MediaStack_Importer
{
    class Driver
    {
        static async Task Main(string[] args)
        {
            new MediaStackContext();
            EnvParser.ParseEnvFromFile($@"{Environment.CurrentDirectory}{Path.DirectorySeparatorChar}.env");
            MediaImporter i = new MediaImporter(new MediaFSController(), new UnitOfWorkService());
            await i.Start();
        }
    }
}
