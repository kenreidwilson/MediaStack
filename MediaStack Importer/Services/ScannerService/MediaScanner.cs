using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading;
using MediaStackCore.Controllers;
using MediaStackCore.Data_Access_Layer;
using MediaStackCore.Models;
using MediaStackCore.Services.UnitOfWorkService;

namespace MediaStack_Importer.Services.ScannerService
{
    /// <summary>
    ///     Ensures that the data persisted is
    ///     up-to-date with what is on disk.
    /// </summary>
    public class MediaScanner : BaseImporterService
    {
        #region Data members

        private readonly int batchSize = 500;

        private readonly ConcurrentDictionary<string, Category> batchedCategories = new();

        private readonly ConcurrentDictionary<string, Artist> batchedArtists = new();

        private readonly ConcurrentDictionary<string, Album> batchedAlbums = new();

        private readonly ConcurrentDictionary<string, Media> batchedMedia = new();

        private readonly ConcurrentDictionary<string, string> hashCache = new();

        protected IUnitOfWorkService UnitOfWorkService;

        #endregion

        #region Constructors

        public MediaScanner(IMediaFileSystemController fsController, IUnitOfWorkService unitOfWorkService) : base(
            fsController)
        {
            this.FSController = fsController;
            this.UnitOfWorkService = unitOfWorkService;
        }

        #endregion

        #region Methods

        public void Start()
        {
            Console.WriteLine("Creating Media References");
            this.CreateMediaReferences();
            Console.WriteLine("Searching for new Media...");
            this.SearchForNewMedia();
            Console.WriteLine("Verifing Media...");
            this.VerifyAllMedia();
            Console.WriteLine("Done");
        }

        protected void CreateMediaReferences()
        {
            using IUnitOfWork unitOfWork = this.UnitOfWorkService.Create();
            var categoryNames =
                Directory.GetDirectories(this.FSController.MediaDirectory, "*", SearchOption.TopDirectoryOnly);
            foreach (string categoryDirectory in categoryNames)
            {
                string categoryName = categoryDirectory.Split(Path.DirectorySeparatorChar).Last();
                Category category = unitOfWork.FindOrCreateCategory(categoryName);
                var artistNames = Directory.GetDirectories($"{categoryDirectory}", "*", SearchOption.TopDirectoryOnly);
                foreach (string artistDirectory in artistNames)
                {
                    string artistName = artistDirectory.Split(Path.DirectorySeparatorChar).Last();
                    Artist artist = unitOfWork.FindOrCreateArtist(artistName);
                    var albumNames =
                        Directory.GetDirectories($"{artistDirectory}");
                    foreach (string albumDirectory in albumNames)
                    {
                        string albumName = albumDirectory.Split(Path.DirectorySeparatorChar).Last();
                        unitOfWork.FindOrCreateAlbum(artist, albumName);
                    }
                }
            }
            unitOfWork.Save();
        }

        protected void SearchForNewMedia()
        {
            var filePaths = Directory.GetFiles(this.FSController.MediaDirectory, "*", SearchOption.AllDirectories);
            var unitOfWorkWriteLock = new object();
            int toProcess = filePaths.Length;
            using (ManualResetEvent resetEvent = new ManualResetEvent(false))
            {
                foreach (var filePath in filePaths)
                {
                    ThreadPool.QueueUserWorkItem(callBack =>
                    {
                        try
                        {
                            this.addMedia(this.CreateMediaFromFileIfNotExists(filePath));
                            lock (unitOfWorkWriteLock)
                            {
                                if (this.batchedMedia.Count >= this.batchSize)
                                {
                                    this.saveBatchedMedia();
                                    this.batchedMedia.Clear();
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }
                        finally
                        {
                            if (Interlocked.Decrement(ref toProcess) == 0)
                            {
                                resetEvent.Set();
                            }
                        }
                    });
                }
                resetEvent.WaitOne();
            }

            this.saveBatchedMedia();
            this.batchedMedia.Clear();
        }

        protected Media CreateMediaFromFileIfNotExists(string filePath)
        {
            using IUnitOfWork unitOfWork = this.UnitOfWorkService.Create();
            return unitOfWork.Media.Get().Any(m => string.Equals(m.Path, this.GetRelativePath(filePath))) 
                ? null 
                : this.CreateMediaFromFile(filePath, unitOfWork);
        }

        protected void VerifyAllMedia()
        {
            using (var resetEvent = new ManualResetEvent(false))
            {
                var medias = this.UnitOfWorkService.Create().Media.Get().Where(media => media.Path != null).ToList();
                var unitOfWorkWriteLock = new object();
                int toProcess = medias.Count;
                foreach (var media in medias)
                {
                    ThreadPool.QueueUserWorkItem(callBack =>
                    {
                        try
                        {
                            this.addMedia(this.VerifyMedia(media));

                            lock (unitOfWorkWriteLock)
                            {
                                if (this.batchedMedia.Count >= this.batchSize)
                                {
                                    this.saveBatchedMedia();
                                    this.batchedMedia.Clear();
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }
                        finally
                        {
                            if (Interlocked.Decrement(ref toProcess) == 0)
                            {
                                resetEvent.Set();
                            }
                        }
                    });
                }
                resetEvent.WaitOne();

                this.saveBatchedMedia();
                this.batchedMedia.Clear();
            }
        }

        protected Media VerifyMedia(Media media)
        {
            using IUnitOfWork unitOfWork = this.UnitOfWorkService.Create();
            if (!File.Exists(this.FSController.GetMediaFullPath(media)))
            {
                unitOfWork.DisableMedia(media);
                return media;
            }
            else
            {
                var newHash = this.getFileHash(this.FSController.GetMediaFullPath(media));
                if (newHash != media.Hash)
                {
                    return this.HandleMediaHashChange(media, unitOfWork, newHash);
                }
            }

            return null;
        }

        protected Media HandleMediaHashChange(Media media, IUnitOfWork unitOfWork, string newHash)
        {
            string path = this.FSController.GetMediaFullPath(media);
            unitOfWork.DisableMedia(media);
            this.addMedia(media);
            Media pMedia = unitOfWork.Media.Get().FirstOrDefault(m => m.Hash == newHash);
            if (pMedia == null)
            {
                return this.CreateMediaFromFile(path, unitOfWork);
            }
            else
            {
                return this.HandleMovedMedia(pMedia, path, unitOfWork);
            }
        }

        protected Media CreateMediaFromFile(string filePath, IUnitOfWork unitOfWork)
        {
            Media media = new Media
            {
                Path = this.GetRelativePath(filePath)
            };

            try
            {
                this.FindAndSetMediaTypeAndHash(unitOfWork, filePath, media);
            }
            catch (DuplicateMediaException)
            {
                string fileHash = this.getFileHash(this.FSController.GetMediaFullPath(media));
                media = unitOfWork.Media.Get().FirstOrDefault(m => m.Hash == fileHash);
                if (media != null)
                {
                    media.Path = this.GetRelativePath(filePath);
                    return media;
                }
            }
            catch (Exception)
            {
                return null;
            }

            this.FindAndSetMediaReferences(unitOfWork, filePath, media);

            return media;
        }

        protected void FindAndSetMediaTypeAndHash(IUnitOfWork unitOfWork, string filePath, Media media)
        {
            using (var stream = File.OpenRead(filePath))
            {
                media.Type = this.FSController.DetermineMediaType(stream);
                if (media.Type == null)
                {
                    throw new TypeNotRecognizedException();
                }

                stream.Position = 0;
                media.Hash = this.getFileHash(filePath, stream);
                if (unitOfWork.Media.Get().Any(m => m.Hash == media.Hash))
                {
                    throw new DuplicateMediaException();
                }
            }
        }

        protected void FindAndSetMediaReferences(IUnitOfWork unitOfWork, string filePath, Media media)
        {
            var mediaReferences = this.FSController.DeriveMediaReferences(filePath);

            if (mediaReferences.Category != null)
            {
                string categoryName = mediaReferences.Category;
                media.CategoryID = unitOfWork.Categories.Get().FirstOrDefault(c => c.Name == categoryName)?.ID;
                if (media.CategoryID != null && mediaReferences.Artist != null)
                {
                    string artistName = mediaReferences.Artist;
                    media.ArtistID = unitOfWork.Artists.Get().FirstOrDefault(a => a.Name == artistName)?.ID;
                    if (media.ArtistID != null && mediaReferences.Album != null)
                    {
                        string albumName = mediaReferences.Album;
                        media.AlbumID = unitOfWork.Albums.Get()
                                                  .FirstOrDefault(a => a.Name == albumName && a.ArtistID == media.ArtistID)?.ID;
                    }
                }
            }
        }

        protected Media HandleMovedMedia(Media media, string newPath, IUnitOfWork unitOfWork)
        {
            media.Path = this.GetRelativePath(newPath);
            return media;
        }

        private void addMedia(Media media)
        {
            if (media == null)
            {
                return;
            }

            if (!this.batchedMedia.ContainsKey(media.Hash))
            {
                this.batchedMedia[media.Hash] = media;
            }
        }

        private void saveBatchedMedia()
        {
            using (IUnitOfWork unitOfWork = this.UnitOfWorkService.Create())
            {
                unitOfWork.Media.BulkInsert(
                    this.batchedMedia.Values
                        .Where(media => media.ID == 0 && !unitOfWork.Media
                                                   .Get()
                                                   .Any(m => m.Hash == media.Hash))
                        .ToList());
                unitOfWork.Media.BulkUpdate(this.batchedMedia.Values.Where(m => m.ID != 0).ToList());
                unitOfWork.Save();
            }
        }

        private string getFileHash(string filePath, FileStream stream = null)
        {
            if (!this.hashCache.ContainsKey(this.GetRelativePath(filePath)))
            {
                using (stream ??= File.OpenRead(filePath))
                {
                    this.hashCache[this.GetRelativePath(filePath)] = this.FSController.CalculateHash(stream);
                }
            }
            return this.hashCache[this.GetRelativePath(filePath)];
        }

        public class DuplicateMediaException : Exception { }

        public class TypeNotRecognizedException : Exception { }

        #endregion
    }
}
