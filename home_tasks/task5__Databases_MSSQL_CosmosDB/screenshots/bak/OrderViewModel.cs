using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace CosmosDbFunc;

public class OrderViewModel
{
    [JsonProperty("OrderNumber")]
    public int OrderNumber { get; set; }
    [JsonProperty("OrderDate")]
    public DateTimeOffset OrderDate { get; set; }
    [JsonProperty("Total")]
    public decimal Total { get; set; }
    [JsonProperty("ShippingAddress")]
    public Address ShippingAddress { get; set; }
    [JsonProperty("OrderItems")]
    public List<OrderItemViewModel> OrderItems { get; set; } = new List<OrderItemViewModel>();

    public override string ToString()
    {
        return JsonConvert.SerializeObject(this);
    }
}
