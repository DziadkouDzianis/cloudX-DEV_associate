using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate;

namespace Microsoft.eShopWeb.Web.Pages.Basket;

internal class cosmosDBOutput
{
    public List<OrderItem> OrderDetails { get;set;}

    public Address Address { get; set; }

    public decimal GrandTotal { get; set; }

public cosmosDBOutput(List<OrderItem> orderDetails, Address address, decimal grandTotal)
    {
        OrderDetails = orderDetails;
        Address = address;
        GrandTotal = grandTotal;
    }
}
