using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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


namespace OrderProcessor
{
    internal class cosmosDBConnector
    {

        private static readonly string DatabaseId = "msdocs-db-sql-cosmos";
        private static readonly string CollectionId = "container-ddziadkou";
        private static readonly string Endpoint = "https://cosmosdb-ddziadkou.documents.azure.com:443";
        private static readonly string AuthKey = "8lVDo1uIFQc9b9m4Lz3s7Wrj4RyAyNmgqIpzH4FqqxqmEOzNcJZF87dJYxqKXH2cAzSRqiNTCPuPmru4Jo5WyQ==";
        private static DocumentClient client;

        public static object ConfigurationManager { get; private set; }

        public static async Task Initialize()
        {
            client = new DocumentClient(new Uri(Endpoint), AuthKey);
            CreateDatabaseIfNotExistsAsync().Wait();
            CreateCollectionIfNotExistsAsync().Wait();
        }

        private static async Task CreateDatabaseIfNotExistsAsync()
        {
            try
            {
                await client.ReadDatabaseAsync(UriFactory.CreateDatabaseUri(DatabaseId));
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    await client.CreateDatabaseAsync(new Database { Id = DatabaseId });
                }
                else
                {
                    throw;
                }
            }
        }

        private static async Task CreateCollectionIfNotExistsAsync()
        {
            try
            {
                await client.ReadDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri(DatabaseId, CollectionId));
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    await client.CreateDocumentCollectionAsync(
                        UriFactory.CreateDatabaseUri(DatabaseId),
                        new DocumentCollection { Id = CollectionId },
                        new RequestOptions { OfferThroughput = 1000 });
                }
                else
                {
                    throw;
                }
            }
        }

        //public static async Task<IEnumerable<T>> GetItemsAsync()
        //{
        //    IDocumentQuery<T> query = client.CreateDocumentQuery<T>(
        //        UriFactory.CreateDocumentCollectionUri(DatabaseId, CollectionId))
        //        .AsDocumentQuery();

        //    List<T> results = new List<T>();
        //    while (query.HasMoreResults)
        //    {
        //        results.AddRange(await query.ExecuteNextAsync<T>());
        //    }

        //    return results;
        //}

        public static async Task<Document> CreateItemAsync(cosmosDBOutput item)
        {
            return await client.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(DatabaseId, CollectionId), item);
        }
    }
}
