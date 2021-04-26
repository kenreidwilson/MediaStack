using System.ComponentModel.DataAnnotations;

namespace MediaStack_API.Models.ViewModels
{
    public class AlbumViewModel
    {
        public int ID { get; set; }

        [Required]
        public int ArtistID { get; set; }

        [Required]
        public string Name { get; set; }
    }
}
