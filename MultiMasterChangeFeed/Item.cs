using System;

namespace MultiMasterChangeFeed
{
    public class Item
    {
        public string Id { get; set; }
        public string Partition { get; set; }
        public DateTime _ts { get; set; }
    }
}
