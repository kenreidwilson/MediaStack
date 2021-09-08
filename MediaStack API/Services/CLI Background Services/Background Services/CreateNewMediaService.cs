using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediaStackCore.Models;
using MediaStackCore.Services.MediaScannerService;
using MediaStackCore.Services.MediaService;
using MediaStackCore.Services.UnitOfWorkFactoryService;
using Microsoft.Extensions.Logging;

namespace MediaStack_API.Services.CLI_Background_Services.Background_Services
{
    public class CreateNewMediaService : BatchedParallelService<Media>
    {
        #region Data members

        protected IUnitOfWorkFactory unitOfWorkFactory;

        protected IMediaService mediaService;

        protected IMediaScanner mediaScanner;

        #endregion

        #region Constructors

        public CreateNewMediaService(ILogger logger, IUnitOfWorkFactory unitOfWorkFactory,
            IMediaService mediaService, IMediaScanner mediaScanner) : base(logger)
        {
            this.unitOfWorkFactory = unitOfWorkFactory;
            this.mediaService = mediaService;
            this.mediaScanner = mediaScanner;
        }

        #endregion

        #region Methods

        public override async Task Execute(CancellationToken cancellationToken)
        {
            Logger.LogInformation("Creating New Media");
            await ExecuteWithData(await this.getNewMediaFiles(), cancellationToken);
        }

        protected override async Task ProcessData(object data)
        {
            if (data is IFileInfo mediaFile)
            {
                Logger.LogDebug($"Processing Media: {mediaFile.Name}");

                using var unitOfWork = this.unitOfWorkFactory.Create();

                try
                {
                    var media = await this.mediaService.CreateNewMediaOrFixMediaPathAsync(mediaFile);
                    if (media != null)
                    {
                        BatchedEntities[media.Hash] = media;
                    }
                }
                catch (ArgumentException e)
                {
                    Logger.LogDebug($"Error processing {mediaFile.Name}: {e.Message}");
                }
                catch (TaskCanceledException)
                {
                }
            }
        }

        protected override void Save()
        {
            using (var unitOfWork = this.unitOfWorkFactory.Create())
            {
                Logger.LogDebug("Saving New Media");
                unitOfWork.Media.BulkInsert(
                    BatchedEntities.Values
                                   .Where(media => media.ID == 0 && !unitOfWork.Media
                                                                               .Get()
                                                                               .Any(m => m.Hash == media.Hash))
                                   .ToList());

                unitOfWork.Media.BulkUpdate(BatchedEntities.Values.Where(m => m.ID != 0).ToList());
                unitOfWork.Save();
            }

            BatchedEntities.Clear();
        }

        protected override void OnFinish()
        {
            this.Save();
            this.Logger.LogInformation("Done Creating New Media.");
        }

        private async Task<IEnumerable<IFileInfo>> getNewMediaFiles()
        {
            var newMediaFiles = new List<IFileInfo>();
            this.mediaScanner.OnNewMediaFileFound += newMediaFiles.Add;
            await this.mediaScanner.FindNewMedia();
            Logger.LogInformation($"Found {newMediaFiles.Count} new Media");
            return newMediaFiles;
        }

        #endregion
    }
}