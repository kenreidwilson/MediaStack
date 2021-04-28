using System;
using System.Collections.Generic;
using MediaStackCore.Models;

namespace MediaStack_API.Models.ViewModels
{
    public class MediaViewModel
    {
        #region Properties

        public int ID { get; set; }

        public int? CategoryID { get; set; }

        public int? ArtistID { get; set; }

        public int? AlbumID { get; set; }

        public ICollection<TagViewModel> Tags { get; set; } = new List<TagViewModel>();

        public string Hash { get; set; }

        public MediaType? Type { get; set; }

        public DateTime Created { get; set; } = DateTime.Now;

        public int AlbumOrder { get; set; }

        public int Score { get; set; } = 0;

        public string Source { get; set; }

        #endregion
    }
}
