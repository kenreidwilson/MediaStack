using System;
using System.Collections.Generic;

namespace MediaStack_Library.Model
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

        public List<Tag> Tags { get; set; }

        public string Hash { get; set; }

        public string Path { get; set; }

        public MediaType? Type { get; set; }

        public DateTime Created { get; set; } = DateTime.Now;
    }
}
