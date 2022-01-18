using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;

namespace ShoppingCartList.Models
{
    public class ShoppingCartItem : TableEntity
    {
        [JsonProperty("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public DateTime Created { get; set; } = DateTime.Now;
        public string ItemName { get; set; }
        public bool Collected { get; set; }
        [JsonProperty("category")]
        public string Category { get; set; }
    }

    public class CreateShoppingCartItem
    {
        public string ItemName { get; set; }
        public string Category { get; set; }
    }

    public class UpdateShoppingCartItem
    {
        public bool Collected { get; set; }
    }

}
