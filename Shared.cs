using System.Collections.Generic;
using Newtonsoft.Json;

namespace My.Funtions
{
    public class UserItem
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        
        [JsonProperty("friends")]
        public List<string> Friends { get; set; }
    }

    public class IndexItem
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        
        [JsonProperty("value")]
        public int Value { get; set; }
    }
}