using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

using Microsoft.Extensions.Configuration.Json;

namespace AzFunctions
{
    public class PostData
    {
        public string name { get; set; }
    }

    public class ItemJson
     {
        public int id { get; set; }
        public decimal quantity { get; set; }
    }

    public class OrderItemsReserver
    {
        [FunctionName("OrderItemsReserverFunction")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log, ExecutionContext context)
        {
            log.LogInformation($"C# Http trigger function executed at: {DateTime.Now}");
            CreateContainerIfNotExists(log, context);

            CloudStorageAccount storageAccount = GetCloudStorageAccount(context);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference("ddziadkou-sa");

            string str = req.Query["name"];

            string randomStr = $"Some_ID__{DateTime.UtcNow.ToString("dd_MM_yyyy__H_mm_ss_fff")}.json";
            CloudBlockBlob blob = container.GetBlockBlobReference(randomStr);

            //[{ "Id":1,"Quantity":12},{ "Id":2,"Quantity":6}]

            //string str = @"[{ ""Id"":1,""Quantity"":12},{ ""Id"":2,""Quantity"":6}]";

            //var serializeJesonObject = JsonConvert.SerializeObject(new {itemI4d = randomStr, Quantity = $"<html><body><h2> This is a Sample email content ! </h2></body></html>" });

            //ItemJson deptObj = JsonConvert.DeserializeObject<ItemJson>(str);
            var deptObj = JsonConvert.DeserializeObject<List<ItemJson>>(str);
            string json = JsonConvert.SerializeObject(deptObj, Formatting.Indented);

            var serializeJesonObject = JsonConvert.SerializeObject(new { str = str });
            blob.Properties.ContentType = "application/json";

            using (var ms = new MemoryStream())
            {
                LoadStreamWithJson(ms, json);
                await blob.UploadFromStreamAsync(ms);
            }
            log.LogInformation($"Blob {randomStr} is uploaded to container {container.Name}");
            await blob.SetPropertiesAsync();



            return new OkObjectResult("UploadBlobHttpTrigger function executed successfully!!");
        }

        private static void CreateContainerIfNotExists(ILogger logger, ExecutionContext executionContext)
        {
            CloudStorageAccount storageAccount = GetCloudStorageAccount(executionContext);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            string[] containers = new string[] { "ddziadkou-sa" };
            foreach (var item in containers)
            {
                CloudBlobContainer blobContainer = blobClient.GetContainerReference(item);
                blobContainer.CreateIfNotExistsAsync();
            }
        }

        private static CloudStorageAccount GetCloudStorageAccount(ExecutionContext executionContext)
        {
            var config = new ConfigurationBuilder()
                            .SetBasePath(executionContext.FunctionAppDirectory)
                            .AddJsonFile("local.settings.json", true, true)
                            .AddEnvironmentVariables().Build();
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(config["CloudStorageAccount"]);
            return storageAccount;
        }
        private static void LoadStreamWithJson(Stream ms, object obj)
        {
            StreamWriter writer = new StreamWriter(ms);
            writer.Write(obj);
            writer.Flush();
            ms.Position = 0;
        }
    }
}