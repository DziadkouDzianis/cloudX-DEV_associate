using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OrderProcessor;

using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using System.Configuration;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.Cosmos;
//using OrderProcessor.cosmosDBConnector;

namespace OrderProcessorTrigger
{
    //public class connectionString
    //{
    //    const string cosmos = @"AccountEndpoint=https://cosmosdb-ddziadkou.documents.azure.com:443/;AccountKey=rguA80a7SvDG78o0pEihiI1zMPVo8qkibkdyYVbaDq5Rj8Gpd15Kb64hr8zXyC7PPI4LdkW5ysgq8sajrKMtSA==;";
    //    const string cosmos_database = "msdocs-db-sql-cosmos";
    //    const string cosmos_collection = "container-ddziadkou";

    //    CosmosClient client = new CosmosClient(cosmos);
    //    var database = client.GetDatabase(cosmos_database);
    //    var collection = database.GetContainer(cosmos_collection);


    //    private static string DatabaseId = "msdocs-db-sql-cosmos";
    //    private static string CollectionId = "container-ddziadkou";
    //    private static string Endpoint = "https://cosmosdb-ddziadkou.documents.azure.com:443";
    //    private static string AuthKey = "rguA80a7SvDG78o0pEihiI1zMPVo8qkibkdyYVbaDq5Rj8Gpd15Kb64hr8zXyC7PPI4LdkW5ysgq8sajrKMtSA==";



    //}
    public static class InsertOrderItemTrigger
    {
        [FunctionName("InsertOrderItemTrigger")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            //[CosmosDB(
            //    databaseName: DatabaseId,
            //    collectionName: CollectionId,
            //    ConnectionStringSetting = cosmos)] IAsyncCollector<object> products,
            ILogger log
            //, ExecutionContext context
            )
        {
            try
            {
                string requestBody = req.Query["name"];
                //string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                cosmosDBOutput finalOutput = JsonConvert.DeserializeObject<cosmosDBOutput>(requestBody);

                cosmosDBConnector cosmosDBConnector = new cosmosDBConnector();
                await cosmosDBConnector.Initialize();
                await cosmosDBConnector.CreateItemAsync(finalOutput);

                return new OkObjectResult("create the cosmosDB item");
            }
            catch (Exception ex)
            {
                log.LogError($"Couldn't insert item. Exception thrown: {ex.Message}");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }
    }
}