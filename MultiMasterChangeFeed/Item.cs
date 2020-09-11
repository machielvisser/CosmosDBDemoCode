using Newtonsoft.Json;
using System;

namespace MultiMasterChangeFeed
{
    public class Item
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("partition")]
        public string Partition { get; set; }

        [JsonProperty("_ts")]
        public string InsertionTimestamp { get; set; }
        
        [JsonProperty("region")]
        public string Region { get; set; }
    }
}
