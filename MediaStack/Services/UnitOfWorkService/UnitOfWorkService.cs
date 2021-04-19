using MediaStack_Library.Data_Access_Layer;

namespace MediaStack_Library.Services.UnitOfWorkService
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