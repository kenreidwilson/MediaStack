using MediaStack_Library.Data_Access_Layer;

namespace MediaStack_Library.Services.UnitOfWorkService
{
    public interface IUnitOfWorkService
    {
        #region Methods

        IUnitOfWork Create();

        #endregion
    }
}