using System.IO;
using System.Linq;
using MediaStackCore.Controllers;
using MediaStackCore.Models;
using MediaStackCore.Services.UnitOfWorkService;
using Microsoft.Extensions.Logging;

namespace MediaStack_Importer.Services.ScannerService.ScannerJobs
{
    public class CreateCategoriesJob : BatchScannerJob<Category>
    {
        #region Data members

        protected IUnitOfWorkService UnitOfWorkService;

        protected IMediaFileSystemController FSController;

        #endregion

        #region Constructors

        public CreateCategoriesJob(ILogger logger, IMediaFileSystemController fsController, IUnitOfWorkService unitOfWorkService) : base(logger)
        {
            this.FSController = fsController;
            this.UnitOfWorkService = unitOfWorkService;
        }

        #endregion

        #region Methods

        public void CreateCategories()
        {
            this.Logger.LogDebug("Creating Categories");
            Execute(Directory.GetDirectories(this.FSController.MediaDirectory, "*", SearchOption.TopDirectoryOnly));
        }

        protected override void ProcessData(object data)
        {
            if (data is string categoryPath)
            {
                var categoryName = categoryPath.Split(Path.DirectorySeparatorChar).Last();
                this.Logger.LogDebug($"Processing Category: {categoryName}");
                Category potentialCategory = this.getCategoryIfNotExists(categoryName);
                if (potentialCategory != null)
                {
                    BatchedEntities[categoryName] = potentialCategory;
                }
            }
        }

        protected override void Save()
        {
            this.Logger.LogDebug("Saving Categories");
            using var unitOfWork = this.UnitOfWorkService.Create();
            unitOfWork.Categories.BulkInsert(BatchedEntities.Values.ToList());
            unitOfWork.Save();
            BatchedEntities.Clear();
        }

        private Category getCategoryIfNotExists(string categoryName)
        {
            using var unitOfWork = this.UnitOfWorkService.Create();
            if (!unitOfWork.Categories.Get().Any(c => c.Name == categoryName))
            {
                return new Category {Name = categoryName};
            }
            return null;
        }

        #endregion
    }
}
