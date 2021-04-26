using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using MediaStackCore.Data_Access_Layer;
using MediaStackCore.Models;

namespace MediaStack_API.Models.Requests
{
    public class AlbumSortRequest
    {
        [Required]
        public string Property { get; set; }

        [Required]
        public int AlbumID { get; set; }

        public Album SortRequestedAlbum(IUnitOfWork unitOfWork)
        {
            List<Media> medias = unitOfWork.Media.Get()
                                           .Where(m => m.AlbumID == this.AlbumID)
                                           .OrderBy(m => m.Path)
                                           .ToList();

            foreach (Media media in medias)
            {
                media.AlbumOrder = medias.IndexOf(media);
                unitOfWork.Media.Update(media);
            }
            unitOfWork.Save();
            return unitOfWork.Albums.Get().First(a => a.ID == this.AlbumID);
        }
    }
}
