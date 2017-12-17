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
        private String name { get; set; }
        [JsonProperty("aliases")]
        private List<String> aliases { get; set; } = null;
        [JsonProperty("lat")]
        private Double lat { get; set; }
        [JsonProperty("lng")]
        private Double lng { get; set; }
        [JsonProperty("level")]
        private int level { get; set; }

    }
}
