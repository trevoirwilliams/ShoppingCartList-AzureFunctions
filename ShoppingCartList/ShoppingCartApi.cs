using Azure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Newtonsoft.Json;
using ShoppingCartList.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;

namespace ShoppingCartList
{
    public class ShoppingCartApi
    {
        private readonly CosmosClient _cosmosClient;
        private Container documentContainer;
       
        public ShoppingCartApi(CosmosClient cosmosClient)
        {
            _cosmosClient = cosmosClient;
            documentContainer = _cosmosClient.GetContainer("ShoppingCartItems", "Items");
        }

        [FunctionName("GetShoppingCartItems")]
        public async Task<IActionResult> GetShoppingCartItems(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "shoppingcartitem")] HttpRequest req,
            ////[CosmosDB(
            ////    databaseName: "ShoppingCartItems",
            ////    collectionName: "Items",
            ////    ConnectionStringSetting = "CosmosDBConnection")] DocumentClient client,
            ILogger log)
        {
            log.LogInformation("Getting All Shopping Cart Items");

            ////Uri collectionUri = UriFactory.CreateDocumentCollectionUri("ShoppingCartItems", "Items");

            ////IDocumentQuery<ShoppingCartItem> query = client.CreateDocumentQuery<ShoppingCartItem>(collectionUri)
            ////    .AsDocumentQuery();

            ////while (query.HasMoreResults)
            ////{
            ////    foreach (ShoppingCartItem result in await query.ExecuteNextAsync())
            ////    {
            ////        log.LogInformation(result.ItemName);
            ////    }
            ////}

            ////List<ShoppingCartItem> shoppingCartItems = new();
            
            var items = documentContainer.GetItemQueryIterator<ShoppingCartItem>();
            return new OkObjectResult((await items.ReadNextAsync()).ToList());
        }

        [FunctionName("GetShoppingCartItemById")]
        public async Task<IActionResult> GetShoppingCartItemById(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "shoppingcartitem/{id}/{category}")]

            ////[CosmosDB(
            ////    databaseName: "ShoppingCartItems",
            ////    collectionName: "Items",
            ////    ConnectionStringSetting = "CosmosDBConnection",
            ////    Id = "{id}",
            ////    PartitionKey = "{category}")]ShoppingCartItem shoppingCartItem,

            HttpRequest req, ILogger log, string id, string category)
        {
            log.LogInformation($"Getting Shopping Cart Item with ID: {id}");

            try
            {
                var item = await documentContainer.ReadItemAsync<ShoppingCartItem>(id, new Microsoft.Azure.Cosmos.PartitionKey(category));
                return new OkObjectResult(item.Resource);
            }
            catch (CosmosException e) when (e.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return new NotFoundResult();
            }
        }

        [FunctionName("CreateShoppingCartItem")]
        public async Task<IActionResult> CreateShoppingCartItems(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "shoppingcartitem")] HttpRequest req,
            ////[CosmosDB(
            ////    databaseName: "ShoppingCartItems",
            ////    collectionName: "Items",
            ////    ConnectionStringSetting = "CosmosDBConnection")] IAsyncCollector<ShoppingCartItem> shoppingCartItemsOut,
            ILogger log)
        {
            log.LogInformation("Creating Shopping Cart Item");
            string requestData = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<CreateShoppingCartItem>(requestData);

            var item = new ShoppingCartItem
            {
                ItemName = data.ItemName,
                Category = data.Category,
            };

            await documentContainer.CreateItemAsync(item, new Microsoft.Azure.Cosmos.PartitionKey(item.Category));

            ////await shoppingCartItemsOut.AddAsync(item);

            return new OkObjectResult(item);
        }

        [FunctionName("PutShoppingCartItem")]
        public async Task<IActionResult> PutShoppingCartItem(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "shoppingcartitem/{id}/{category}")] HttpRequest req,
            ILogger log, string id, string category)
        {
            log.LogInformation($"Updating Shopping Cart Item with ID: {id}");

            string requestData = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<UpdateShoppingCartItem>(requestData);

            var item = await documentContainer.ReadItemAsync<ShoppingCartItem>(id, new Microsoft.Azure.Cosmos.PartitionKey(category));

            if (item.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return new NotFoundResult();
            }

            item.Resource.Collected = data.Collected;

            await documentContainer.UpsertItemAsync(item.Resource);

            return new OkObjectResult(item.Resource);
        }

        [FunctionName("DeleteShoppingCartItem")]
        public async Task<IActionResult> DeleteShoppingCartItem(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "shoppingcartitem/{id}/{category}")] HttpRequest req,
            ILogger log, string id, string category)
        {
            log.LogInformation($"Deleting Shopping Cart Item with ID: {id}");

            await documentContainer.DeleteItemAsync<ShoppingCartItem>(id, new Microsoft.Azure.Cosmos.PartitionKey(category));
            return new OkResult();
        }
    }
}
