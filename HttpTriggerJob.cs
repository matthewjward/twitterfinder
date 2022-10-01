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
    public static class HttpTriggerJob
    {
        [FunctionName("HttpTriggerJob")]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            [CosmosDB(
                databaseName: "%COSMOS_DATABASE%",
                collectionName: "%COSMOS_CONTAINER%",
                ConnectionStringSetting = "COSMOS_DB_CONNECTION",
                Id = "mattyjward")] UserItem seedItem,
            [CosmosDB(
                databaseName: "%COSMOS_DATABASE%",
                collectionName: "%COSMOS_CONTAINER%",
                ConnectionStringSetting = "COSMOS_DB_CONNECTION",
                Id = "index")] IndexItem indexItem,
            [CosmosDB(
                databaseName: "%COSMOS_DATABASE%",
                collectionName: "%COSMOS_CONTAINER%",
                ConnectionStringSetting = "COSMOS_DB_CONNECTION")] IAsyncCollector<dynamic> itemsOut,
            ILogger log)
        {
            const int itemsToProcess = 1;
            var totalFriends = seedItem.Friends.Count(); //TODO - handle null
            var index = indexItem.Value; //TODO -mhandle missing
            
            HttpClient client = new HttpClient(); //TODO - move to settings 
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {Environment.GetEnvironmentVariable("TwitterToken")}");

            var itemsProcessed = 0;
            while(itemsProcessed < itemsToProcess){
                if (index >= totalFriends)
                {
                    var response = client.GetAsync($"https://api.twitter.com/1.1/friends/ids.json?screen_name=mattyjward").Result.Content.ReadAsStringAsync().Result;
                    var friends = JObject.Parse(response)["ids"];
            
                    var newSeedItem = new {
                        id = "mattyjward",
                        friends = friends
                    };

                    itemsOut.AddAsync(newSeedItem);                        
                    index = 0;
                }
                else 
                {
                    var friendId =  seedItem.Friends[index];
                    var response = client.GetAsync($"https://api.twitter.com/1.1/friends/ids.json?user_id={friendId}").Result.Content.ReadAsStringAsync().Result;
                    var friends = JObject.Parse(response)["ids"];
               
                    var friendItem = new {
                        id = friendId,
                        friends = friends
                    };

                    itemsOut.AddAsync(friendItem);
                    index++;
                }

                itemsProcessed++;               
            }

            var newIndexItem = new {
                id = "index",
                value = index
            };

            itemsOut.AddAsync(newIndexItem);             
        
            string responseMessage = $"Next index is {index}. This HTTP triggered function executed successfully.";

            return new OkObjectResult(responseMessage);
        }
    }
}
