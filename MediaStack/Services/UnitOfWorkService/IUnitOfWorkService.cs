using MediaStackCore.Data_Access_Layer;

namespace MediaStackCore.Services.UnitOfWorkService
{
    public interface IUnitOfWorkService
    {
        #region Methods

        IUnitOfWork Create();

        #endregion
    }
}