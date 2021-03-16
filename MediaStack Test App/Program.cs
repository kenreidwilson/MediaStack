using MediaStack_Importer.Importer;
using MediaStack_Library.Data_Access_Layer;
using MediaStack_Library.Utility;
using System;
using System.IO;
using System.Threading.Tasks;

namespace MediaStack_Test_App
{
    class Program
    {
        static async Task Main(string[] args)
        {
            new MediaStackContext();
            EnvParser.ParseEnvFromFile($@"{Environment.CurrentDirectory}{Path.DirectorySeparatorChar}.env");
            MediaImporter i = new MediaImporter();
            await i.Start();
        }
    }
}
