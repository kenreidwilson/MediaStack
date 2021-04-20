using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MediaStackCore.Models
{
    public class Category
    {
        public int ID { get; set; }
        public string Name { get; set; }

        [JsonIgnore]
        public ICollection<Media> Media { get; set; } = new HashSet<Media>();
    }
}
