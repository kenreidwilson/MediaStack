using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediaStackCore.Services.MediaFilesService;
using MediaStackCore.Services.UnitOfWorkFactoryService;

namespace MediaStackCore.Services.MediaScannerService
{
    public class MediaScanner : IMediaScanner
    {
        #region Data members

        protected IMediaFilesService mediaFilesService;

        protected IUnitOfWorkFactory unitOfWorkFactory;

        #endregion

        #region Constructors

        public MediaScanner(IMediaFilesService mediaFilesService, IUnitOfWorkFactory unitOfWorkFactory)
        {
            this.mediaFilesService = mediaFilesService;
            this.unitOfWorkFactory = unitOfWorkFactory;
        }

        #endregion

        #region Methods

        public event IMediaScanner.NewMediaFileDelegate OnNewMediaFileFound;

        public event IMediaScanner.MissingMediaDelegate OnMissingMediaFound;

        public Task FindNewMedia()
        {
            var tasks = new List<Task>();

            foreach (var mediaData in this.mediaFilesService.GetAllMediaData())
            {
                tasks.Add(Task.Run(() =>
                {
                    using var unitOfWork = this.unitOfWorkFactory.Create();
                    if (!unitOfWork.Media.Get().Any(m => m.Path == mediaData.RelativePath))
                    {
                        this.OnNewMediaFileFound?.Invoke(mediaData);
                    }
                }));
            }

            return Task.WhenAll(tasks);
        }

        public Task FindMissingMedia()
        {
            var tasks = new List<Task>();

            foreach (var media in this.unitOfWorkFactory.Create().Media.Get(m => m.Path != null))
            {
                tasks.Add(Task.Run(() =>
                {
                    using var unitOfWork = this.unitOfWorkFactory.Create();
                    if (!this.mediaFilesService.DoesMediaFileExist(media))
                    {
                        this.OnMissingMediaFound?.Invoke(media);
                    }
                }));
            }

            return Task.WhenAll(tasks);
        }

        public Task FindChangedMediaFiles()
        {
            var tasks = new List<Task>();

            foreach (var media in this.unitOfWorkFactory.Create().Media.Get(m => m.Path != null))
            {
                tasks.Add(Task.Run(async () =>
                {
                    var mediaData = this.mediaFilesService.GetMediaData(media);

                    if (media == null)
                    {
                        return;
                    }

                    await using var mediaDataStream = mediaData.GetDataStream();
                    if (await this.mediaFilesService.Hasher.CalculateHashAsync(mediaDataStream,
                        mediaData.FullPath) != media.Hash)
                    {
                        this.OnMissingMediaFound?.Invoke(media);
                        this.OnNewMediaFileFound?.Invoke(mediaData);
                    }
                }));
            }

            return Task.WhenAll(tasks);
        }

        #endregion
    }
}