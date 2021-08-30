using MediaStackCore.Data_Access_Layer;

namespace MediaStackCore.Services.UnitOfWorkService
{
    public class UnitOfWorkService : IUnitOfWorkService
    {
        #region Methods

        public IUnitOfWork Create()
        {
            return new UnitOfWork(new MediaStackContext());
        }

        #endregion
    }
}