using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MediaStack_Library.Model
{
    public class Album
    {
        public int ID { get; set; }

        public string Name { get; set; }

        public int ArtistID { get; set; }

        [JsonIgnore]
        public Artist Artist { get; set; }

        [JsonIgnore]
        public ICollection<Media> Media { get; set; } = new HashSet<Media>();
    }
}
