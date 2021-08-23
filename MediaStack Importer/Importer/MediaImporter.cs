using System.Threading.Tasks;
using MediaStack_Importer.Services.MonitorService;
using MediaStack_Importer.Services.ScannerService;
using MediaStackCore.Controllers;
using MediaStackCore.Services.UnitOfWorkService;
using Microsoft.Extensions.Logging;

namespace MediaStack_Importer.Importer
{
    public class MediaImporter
    {
        #region Data members

        protected ILogger Logger;

        protected IUnitOfWorkService UnitOfWorkService;

        protected IFileSystemController fileSystemFSHelper;

        protected MediaScanner scanner;

        protected MediaMonitor monitor;

        #endregion

        #region Constructors

        public MediaImporter(ILogger logger, IFileSystemController fsHelper, IUnitOfWorkService unitOfWorkService)
        {
            this.Logger = logger;
            this.fileSystemFSHelper = fsHelper;
            this.UnitOfWorkService = unitOfWorkService;
            this.scanner = new MediaScanner(this.Logger, this.UnitOfWorkService, this.fileSystemFSHelper);
            this.monitor = new MediaMonitor(this.Logger, this.UnitOfWorkService, this.fileSystemFSHelper);
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
