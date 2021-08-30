using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediaStackCore.Controllers;
using MediaStackCore.Models;
using MediaStackCore.Services.UnitOfWorkService;
using Microsoft.Extensions.Logging;

namespace MediaStack_API.Services.CLI_Background_Services.Background_Services
{
    public class CreateCategoriesService : BatchedParallelService<Category>
    {
        #region Data members

        protected IUnitOfWorkService UnitOfWorkService;

        protected IFileSystemController FSController;

        #endregion

        #region Constructors

        public CreateCategoriesService(ILogger logger, IUnitOfWorkService unitOfWorkService,
            IFileSystemController fsController) : base(logger)
        {
            this.FSController = fsController;
            this.UnitOfWorkService = unitOfWorkService;
        }

        #endregion

        #region Methods

        public override Task Execute(CancellationToken cancellationToken)
        {
            Logger.LogDebug("Creating Categories");
            return ExecuteWithData(this.FSController.GetCategoryNames(), cancellationToken);
        }

        protected override Task ProcessData(object data)
        {
            if (data is string categoryPath)
            {
                var categoryName = categoryPath.Split(Path.DirectorySeparatorChar).Last();
                Logger.LogDebug($"Processing Category: {categoryName}");
                var potentialCategory = this.getCategoryIfNotExists(categoryName);
                if (potentialCategory != null)
                {
                    BatchedEntities[categoryName] = potentialCategory;
                }
            }

            return Task.CompletedTask;
        }

        protected override void Save()
        {
            Logger.LogDebug("Saving Categories");
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