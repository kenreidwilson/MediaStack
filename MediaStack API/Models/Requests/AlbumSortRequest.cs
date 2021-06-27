using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using MediaStackCore.Data_Access_Layer;
using MediaStackCore.Models;
using Microsoft.EntityFrameworkCore;

namespace MediaStack_API.Models.Requests
{
    public class AlbumSortRequest
    {
        [Required]
        public string Property { get; set; }

        [Required]
        public int AlbumID { get; set; }

        public async Task<Album> SortRequestedAlbum(IUnitOfWork unitOfWork)
        {
            List<Media> medias = await unitOfWork.Media.Get()
                                           .Where(m => m.AlbumID == this.AlbumID)
                                           .OrderBy(m => m.Path)
                                           .ToListAsync();

            foreach (Media media in medias)
            {
                media.AlbumOrder = medias.IndexOf(media);
                unitOfWork.Media.Update(media);
            }
            await unitOfWork.SaveAsync();
            return await unitOfWork.Albums.Get().FirstAsync(a => a.ID == this.AlbumID);
        }
    }
}
