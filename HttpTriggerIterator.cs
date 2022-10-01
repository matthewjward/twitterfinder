using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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

    public static class HttpTriggerIterator
    {
        [FunctionName("HttpTriggerIterator")]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            [CosmosDB(
                databaseName: "ToDoList",
                collectionName: "Items",
                ConnectionStringSetting = "CosmosDBConnection",
                Id = "mattyjward")] UserItem seedItem,
            [CosmosDB(
                databaseName: "ToDoList",
                collectionName: "Items",
                ConnectionStringSetting = "CosmosDBConnection",
                Id = "index")] IndexItem indexItem,
            [CosmosDB(
                databaseName: "ToDoList",
                collectionName: "Items",
                ConnectionStringSetting = "CosmosDBConnection")] IAsyncCollector<dynamic> itemsOut,
            ILogger log)
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {Environment.GetEnvironmentVariable("TwitterToken")}");

            seedItem.Friends.Skip(indexItem.Value).Take(10).ToList().ForEach(i => 
            {
                var response = client.GetAsync($"https://api.twitter.com/1.1/friends/ids.json?user_id={i}").Result.Content.ReadAsStringAsync().Result;
                var friends = JObject.Parse(response)["ids"];
               
                var friendItem = new {
                    id = i,
                    friends = friends
                };
                
                itemsOut.AddAsync(friendItem);  
            });

            var newIndexItem = new {
                id = "index",
                value = indexItem.Value+10
            };

            itemsOut.AddAsync(newIndexItem); 
            
            
            string responseMessage = $"Retrieved friends for -. This HTTP triggered function executed successfully.";

            return new OkObjectResult(responseMessage);
        }
    }
}
