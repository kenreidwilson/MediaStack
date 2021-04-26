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

            /*
            EnvParser.ParseEnvFromFile($@"{Environment.CurrentDirectory}{Path.DirectorySeparatorChar}.env");
            MediaFSController controller = new MediaFSController();  
            var context = new MediaStackContext();
            using (StreamReader r = new StreamReader("output2.json"))
            {
                var vals = new Dictionary<string, Entity>();
                string json = r.ReadToEnd();
                List<Entity> items = JsonConvert.DeserializeObject<List<Entity>>(json);
                foreach (Entity e in items)
                {
                    vals[e.hash] = e;
                }
                using var md5 = MD5.Create();
                foreach (Media m in context.Media)
                {
                    using (var stream = File.OpenRead(MediaFSController.GetMediaFullPath(m)))
                    {
                        string hash = BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToLowerInvariant();
                        try
                        {
                            m.Source = vals[hash].source;
                            m.Score = vals[hash].score;
                            foreach (string tagName in vals[hash].tags)
                            {
                                var tag = context.Tags.FirstOrDefault(tag => tag.Name == tagName);
                                if (tag == null)
                                {
                                    tag = new Tag { Name = tagName };
                                }
                                m.Tags.Add(tag);
                            }
                        }
                        catch (KeyNotFoundException)
                        {
                            Console.WriteLine($"Could not find key for {hash}");
                        }
                        finally
                        {
                            context.SaveChanges();
                        }
                    }
                }
            }
            */
        }

        public class Entity
        {
            public string hash;
            public int score;
            public string source;
            public List<string> tags;
        }
    }
}
