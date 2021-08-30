using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediaStackCore.Models;
using MediaStackCore.Services.UnitOfWorkService;

namespace MediaStackCore.Controllers
{
    public class MediaFilesController
    {
        #region Types and Delegates

        public delegate void MediaFileChangedDelegate(Media media, MediaData newMediaData);

        public delegate void MissingMediaDelegate(Media media);

        public delegate void NewMediaFileDelegate(MediaData mediaData);

        #endregion

        #region Data members

        protected IFileSystemController FileSystemController;

        protected IUnitOfWorkService UnitOfWorkService;

        #endregion

        #region Constructors

        public MediaFilesController(IFileSystemController fileSystemController, IUnitOfWorkService unitOfWorkService)
        {
            this.FileSystemController = fileSystemController;
            this.UnitOfWorkService = unitOfWorkService;
        }

        #endregion

        #region Methods

        public event NewMediaFileDelegate OnNewMediaFileFound;

        public event MissingMediaDelegate OnMissingMediaFound;

        public event MediaFileChangedDelegate OnMediaFileChanged;

        public Task FindNewMedia()
        {
            var tasks = new List<Task>();

            foreach (var mediaData in this.FileSystemController.GetAllMediaData())
            {
                tasks.Add(Task.Run(() =>
                {
                    using (var unitOfWork = this.UnitOfWorkService.Create())
                    {
                        if (unitOfWork.Media.Get().FirstOrDefault(m => m.Path == mediaData.RelativePath) == null)
                        {
                            this.OnNewMediaFileFound?.Invoke(mediaData);
                        }
                    }
                }));
            }

            return Task.WhenAll(tasks);
        }

        public Task FindMissingMedia()
        {
            var tasks = new List<Task>();

            foreach (var media in this.UnitOfWorkService.Create().Media.Get(m => m.Path != null))
            {
                tasks.Add(Task.Run(() =>
                {
                    using var unitOfWork = this.UnitOfWorkService.Create();
                    if (!this.FileSystemController.DoesMediaFileExist(media))
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

            foreach (var media in this.UnitOfWorkService.Create().Media.Get(m => m.Path != null))
            {
                tasks.Add(Task.Run(async () =>
                {
                    var mediaData = this.FileSystemController.GetMediaData(media);

                    if (media == null)
                    {
                        return;
                    }

                    await using var mediaDataStream = mediaData.GetDataStream();
                    if (await this.FileSystemController.Hasher.CalculateHashAsync(mediaDataStream,
                        mediaData.FullPath) != media.Hash)
                    {
                        this.OnMediaFileChanged?.Invoke(media, mediaData);
                    }
                }));
            }

            return Task.WhenAll(tasks);
        }

        #endregion
    }
}