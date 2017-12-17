using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GmitNavUWP
{
    public class Room
    {
        [JsonProperty("name")]
        public String name { get; set; }
        [JsonProperty("aliases")]
        public List<String> aliases { get; set; } = null;
        [JsonProperty("lat")]
        public Double lat { get; set; }
        [JsonProperty("lng")]
        public Double lng { get; set; }
        [JsonProperty("level")]
        public int level { get; set; }

    }
}
