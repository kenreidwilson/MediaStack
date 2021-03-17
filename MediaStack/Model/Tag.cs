using System.Collections;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MediaStack_Library.Model
{
    public class Tag
    {
        public int ID { get; set; }

        public string Name { get; set; }

        [JsonIgnore]
        public virtual ICollection<Media> Media { get; set; } = new HashSet<Media>();
    }
}
