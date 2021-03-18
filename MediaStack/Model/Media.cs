using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MediaStack_Library.Model
{
    public class Media
    {
        public int ID { get; set; }

        public int? CategoryID { get; set; }

        [JsonIgnore]
        public Category Category { get; set; }

        public int? ArtistID { get; set; }

        [JsonIgnore]
        public Artist Artist { get; set; }

        public int? AlbumID { get; set; }

        [JsonIgnore]
        public Album Album { get; set; }

        public ICollection<Tag> Tags { get; set; } = new HashSet<Tag>();

        [JsonIgnore]
        public string Hash { get; set; }

        [JsonIgnore]
        public string Path { get; set; }

        public MediaType? Type { get; set; }

        public DateTime Created { get; set; } = DateTime.Now;

        public int Score { get; set; } = 0;

        public string Source { get; set; }
    }
}
