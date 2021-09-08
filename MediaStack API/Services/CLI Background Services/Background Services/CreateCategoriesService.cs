using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediaStackCore.Models;
using MediaStackCore.Services.MediaFilesService;
using MediaStackCore.Services.UnitOfWorkFactoryService;
using Microsoft.Extensions.Logging;

namespace MediaStack_API.Services.CLI_Background_Services.Background_Services
{
    public class CreateCategoriesService : BatchedParallelService<Category>
    {
        #region Data members

        protected IUnitOfWorkFactory unitOfWorkFactory;

        protected IMediaFilesService FSController;

        #endregion

        #region Constructors

        public CreateCategoriesService(ILogger logger, IUnitOfWorkFactory unitOfWorkFactory,
            IMediaFilesService fsController) : base(logger)
        {
            this.FSController = fsController;
            this.unitOfWorkFactory = unitOfWorkFactory;
        }

        #endregion

        #region Methods

        public override Task Execute(CancellationToken cancellationToken)
        {
            Logger.LogInformation("Creating Categories");
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
            using var unitOfWork = this.unitOfWorkFactory.Create();
            unitOfWork.Categories.BulkInsert(BatchedEntities.Values.ToList());
            unitOfWork.Save();
            BatchedEntities.Clear();
        }

        protected override void OnFinish()
        {
            this.Save();
            this.Logger.LogInformation("Done Creating Categories");
        }

        private Category getCategoryIfNotExists(string categoryName)
        {
            using var unitOfWork = this.unitOfWorkFactory.Create();
            if (!unitOfWork.Categories.Get().Any(c => c.Name == categoryName))
            {
                return new Category {Name = categoryName};
            }

            return null;
        }

        #endregion
    }
}