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
    public static class HttpTriggerFinder
    {
        [FunctionName("HttpTriggerFinder")]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            [CosmosDB("%COSMOS_DATABASE%", "%COSMOS_CONTAINER%",
                ConnectionStringSetting = "COSMOS_DB_CONNECTION",
                SqlQuery = "select * from r where r.id != 'index' AND IS_NULL(r.friends) = false")]
                IEnumerable<UserItem> items,
            ILogger log)
        {
            var me = items.Single(i => i.Id == "mattyjward");
            var others = items.Where(i => i.Id != "mattyjward");
            var list = others.SelectMany(i => i.Friends)
                                .Where(i => !me.Friends.Contains(i))
                                .GroupBy(i => i)
                                .Select(i => new {Key = i.Key, Count = i.Count()})
                                .OrderByDescending(i => i.Count);
            
            /*
            db.collection.aggregate({
            $unwind: "$friends"
            },
            {
            "$group": {
                "_id": "$friends",
                "count": {
                "$sum": 1
                }
            }
            })
            */
            
            return new OkObjectResult(list);
        }
    }
}
