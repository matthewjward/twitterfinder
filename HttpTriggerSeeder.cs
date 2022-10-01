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
    public static class HttpTriggerSeeder
    {
        [FunctionName("HttpTriggerSeeder")]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            [CosmosDB(
                databaseName: "ToDoList",
                collectionName: "Items",
                ConnectionStringSetting = "CosmosDBConnection")] IAsyncCollector<dynamic> itemsOut,
            ILogger log)
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {Environment.GetEnvironmentVariable("TwitterToken")}");
            var response = client.GetAsync($"https://api.twitter.com/1.1/friends/ids.json?screen_name=mattyjward").Result.Content.ReadAsStringAsync().Result;
            var friends = JObject.Parse(response)["ids"];
            
            var seedItem = new {
                id = "mattyjward",
                friends = friends
            };

            var indexItem = new {
                id = "index",
                value = 0
            };

            itemsOut.AddAsync(seedItem);            
            itemsOut.AddAsync(indexItem);            

            string responseMessage = $"Retrieved friends for mattyjward. This HTTP triggered function executed successfully.";

            return new OkObjectResult(responseMessage);
        }
    }
}
