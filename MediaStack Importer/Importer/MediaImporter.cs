using System.Threading.Tasks;
using MediaStack_Importer.Controllers;
using MediaStack_Importer.Services.MonitorService;
using MediaStack_Importer.Services.ScannerService;
using MediaStackCore.Services.UnitOfWorkService;
using Microsoft.Extensions.Logging;

namespace MediaStack_Importer.Importer
{
    public class MediaImporter
    {
        #region Data members

        protected ILogger Logger;

        protected IUnitOfWorkService UnitOfWorkService;

        protected IMediaFileSystemHelper MediaFSHelper;

        protected MediaScanner scanner;

        protected MediaMonitor monitor;

        #endregion

        #region Constructors

        public MediaImporter(ILogger logger, IMediaFileSystemHelper fsHelper, IUnitOfWorkService unitOfWorkService)
        {
            this.Logger = logger;
            this.MediaFSHelper = fsHelper;
            this.UnitOfWorkService = unitOfWorkService;
            this.scanner = new MediaScanner(this.Logger, this.UnitOfWorkService, this.MediaFSHelper);
            this.monitor = new MediaMonitor(this.Logger, this.UnitOfWorkService, this.MediaFSHelper);
        }

        #endregion

        #region Methods

        public async Task Start()
        {
            this.scanner.Start();
            //await this.monitor.Start();
        }

        #endregion
    }
}
