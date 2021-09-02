using System.Linq;
using MediaStackCore.Data_Access_Layer;
using MediaStackCore.Models;

namespace MediaStackCore.Extensions
{
    public static class UnitOfWorkExtensions
    {
        #region Methods

        public static Media DisableMedia(this IUnitOfWork unitOfWork, Media media)
        {
            media.Path = null;
            return media;
        }

        public static bool IsMediaDisabled(this IUnitOfWork unitOfWor, Media media)
        {
            return media.Path == null;
        }

        public static Media FindMediaFromMediaData(this IUnitOfWork unitOfWork, MediaData mediaData)
        {
            return unitOfWork.Media.Get().FirstOrDefault(m => m.Path == mediaData.RelativePath);
        }

        #endregion
    }
}
