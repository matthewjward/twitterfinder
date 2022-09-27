using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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

    public static class TimerJob
    {
        [FunctionName("TimerJob")]
        public static void Run(
            [TimerTrigger("0 */20 * * * *")]TimerInfo myTimer,
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
            const int itemsToProcess = 15;
            var totalFriends = seedItem?.Friends.Count() ?? 0; 
            var index = indexItem?.Value ?? 0; 
            
            HttpClient client = new HttpClient();  
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {Environment.GetEnvironmentVariable("TwitterToken")}");

            var itemsProcessed = 0;
            while(itemsProcessed < itemsToProcess){
                if (index >= totalFriends)
                {
                    log.LogInformation($"Processing mattyjward");
                    var response = client.GetAsync($"https://api.twitter.com/1.1/friends/ids.json?screen_name=mattyjward").Result;
                    if (response.StatusCode == HttpStatusCode.OK) {
                        var content = response.Content.ReadAsStringAsync().Result;
                        var friends = JObject.Parse(content)["ids"];
                
                        var newSeedItem = new {
                            id = "mattyjward",
                            friends = friends
                        };

                        itemsOut.AddAsync(newSeedItem);                        
                        index = 0;
                    }             
                    else {
                        log.LogWarning($"Received status code {response.StatusCode} processing mattyjward");
                        itemsProcessed = itemsToProcess;
                    }       
                }
                else 
                {
                    var friendId =  seedItem.Friends[index];
                    log.LogInformation($"Processing friend {friendId}");
                    var response = client.GetAsync($"https://api.twitter.com/1.1/friends/ids.json?user_id={friendId}").Result;
                    if (response.StatusCode == HttpStatusCode.OK) {                      
                        var content = response.Content.ReadAsStringAsync().Result;
                        var friends = JObject.Parse(content)["ids"];
                
                        var friendItem = new {
                            id = friendId,
                            friends = friends
                        };

                        itemsOut.AddAsync(friendItem);
                        index++;
                    }
                    else if (response.StatusCode == HttpStatusCode.Unauthorized) {
                        log.LogWarning($"Received status code {response.StatusCode} processing {friendId}");
                        index++;
                    }
                    else {
                        log.LogWarning($"Received status code {response.StatusCode} processing {friendId}");
                        itemsProcessed = itemsToProcess;
                    } 
                }

                itemsProcessed++;               
            }

            var newIndexItem = new {
                id = "index",
                value = index
            };

            itemsOut.AddAsync(newIndexItem);             
        }
    }
}
