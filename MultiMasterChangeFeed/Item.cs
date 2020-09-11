using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace MultiMasterChangeFeed
{
    public class Item
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("partition")]
        public string Partition { get; set; }

        [JsonConverter(typeof(UnixDateTimeConverter))]
        [JsonProperty("_ts")]
        public DateTime InsertionTimestamp { get; set; } = DateTime.UnixEpoch;
        
        [JsonProperty("region")]
        public string Region { get; set; }
    }
}
