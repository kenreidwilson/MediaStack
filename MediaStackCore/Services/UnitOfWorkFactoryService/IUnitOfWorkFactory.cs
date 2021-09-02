using MediaStackCore.Data_Access_Layer;

namespace MediaStackCore.Services.UnitOfWorkFactoryService
{
    public interface IUnitOfWorkFactory
    {
        #region Methods

        IUnitOfWork Create();

        #endregion
    }
}