namespace Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate;

public class OrderItem : BaseEntity
{
    public CatalogItemOrdered ItemOrdered { get; private set; }

    public decimal UnitPrice { get; private set; }

    public int Units { get; private set; }

    public decimal Cost => UnitPrice * Units;

    private OrderItem()
    {
        // required by EF
    }

    public OrderItem(CatalogItemOrdered itemOrdered, decimal unitPrice, int units)
    {
        ItemOrdered = itemOrdered;
        UnitPrice = unitPrice;
        Units = units;
    }
}

//public class SpecificOrderItem : BaseEntity
//{
//    public CatalogItemOrdered ItemOrdered { get; private set; }

//    public SpecificOrderItem(CatalogItemOrdered itemOrdered, decimal multiply)
//    {
//        ItemOrdered = itemOrdered;
//        Multiply = multiply;
//    }

//    public decimal Multiply { get; private set; }
//}
