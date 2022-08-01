using Newtonsoft.Json;

namespace CosmosDbFunc;

public class OrderItemViewModel
{
    [JsonProperty("ProductId")] 
    public int ProductId { get; set; }
    [JsonProperty("ProductName")]
    public string ProductName { get; set; }
    [JsonProperty("UnitPrice")]
    public decimal UnitPrice { get; set; }
    [JsonProperty("Discount")]
    public decimal Discount => 0;
    [JsonProperty("Units")]
    public int Units { get; set; }
    [JsonProperty("PictureUrl")]
    public string PictureUrl { get; set; }
}
