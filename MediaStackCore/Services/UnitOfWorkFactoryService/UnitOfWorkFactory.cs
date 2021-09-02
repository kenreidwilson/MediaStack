using MediaStackCore.Data_Access_Layer;

namespace MediaStackCore.Services.UnitOfWorkFactoryService
{
    public class UnitOfWorkFactory : IUnitOfWorkFactory
    {
        #region Methods

        public IUnitOfWork Create()
        {
            return new UnitOfWork(new MediaStackContext());
        }

        #endregion
    }
}