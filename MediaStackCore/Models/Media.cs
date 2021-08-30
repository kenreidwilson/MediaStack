using System;
using System.Collections.Generic;

namespace MediaStackCore.Models
{
    public class Media
    {
        public int ID { get; set; }

        public int? CategoryID { get; set; }

        public Category Category { get; set; }

        public int? ArtistID { get; set; }

        public Artist Artist { get; set; }

        public int? AlbumID { get; set; }

        public Album Album { get; set; }

        public ICollection<Tag> Tags { get; set; } = new HashSet<Tag>();

        public string Hash { get; set; }

        public string Path { get; set; }

        public MediaType? Type { get; set; }

        public int Score { get; set; } = 0;

        public string Source { get; set; }

        public DateTime Created { get; set; } = DateTime.Now;

        public int AlbumOrder { get; set; } = -1;
    }
}
