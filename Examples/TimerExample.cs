using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace MattWard.Functions
{
    public static class TimerExample
    {
        [FunctionName("TimerExample")]
        public static void Run([TimerTrigger("0 */2 * * * *")]TimerInfo myTimer, 
        [CosmosDB(
                databaseName: "ToDoList",
                collectionName: "Items",
                ConnectionStringSetting = "CosmosDBConnection")]out dynamic document,
        ILogger log)
        {
            string hello = "hello";
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
            document = new {
                hello
            };
        }
    }
}
