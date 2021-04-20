using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MediaStackCore.Models
{
    public class Tag
    {
        public int ID { get; set; }

        public string Name { get; set; }

        [JsonIgnore]
        public virtual ICollection<Media> Media { get; set; } = new HashSet<Media>();
    }
}
