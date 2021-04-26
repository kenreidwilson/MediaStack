using System.Threading.Tasks;
using MediaStack_Importer.Services.MonitorService;
using MediaStack_Importer.Services.ScannerService;
using MediaStackCore.Controllers;
using MediaStackCore.Services.UnitOfWorkService;

namespace MediaStack_Importer.Importer
{
    public class MediaImporter
    {
        #region Data members

        protected IUnitOfWorkService UnitOfWorkService;
        protected IMediaFileSystemController FSController;

        protected MediaScanner scanner;
        protected MediaMonitor monitor;

        #endregion

        #region Constructors

        public MediaImporter(IMediaFileSystemController fsController, IUnitOfWorkService unitOfWorkService)
        {
            this.FSController = fsController;
            this.UnitOfWorkService = unitOfWorkService;
            this.scanner = new MediaScanner(this.FSController, this.UnitOfWorkService);
            this.monitor = new MediaMonitor(this.FSController, this.UnitOfWorkService);
        }

        #endregion

        #region Methods

        public async Task Start()
        {
            this.scanner.Start();
            await this.monitor.Start();
        }

        #endregion
    }
}