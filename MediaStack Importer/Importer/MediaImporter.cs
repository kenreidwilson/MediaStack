using MediaStack_Library.Controllers;
using MediaStack_Library.Data_Access_Layer;
using System.Threading.Tasks;

namespace MediaStack_Importer.Importer
{
    public class MediaImporter
    {
        protected IUnitOfWork unitOfWork;
        protected MediaFSController controller;

        protected MediaScanner scanner;
        protected MediaMonitor monitor;

        public MediaImporter()
        {
            this.controller = new MediaFSController();
            this.scanner = new MediaScanner(this.controller);
            this.monitor = new MediaMonitor(this.controller);
        }

        public async Task Start()
        {
            this.scanner.Start();
            await this.monitor.Start();
        }
    }
}
