//using System;
using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Ardalis.GuardClauses;
//using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate;




namespace OrderProcessor
{
    // This can easily be modified to be BaseEntity<T> and public T Id to support different key types.
    // Using non-generic integer types for simplicity and to ease caching logic
    public abstract class BaseEntity
    {
        public virtual int Id { get; protected set; }
    }


    /// <summary>
    /// Represents a snapshot of the item that was ordered. If catalog item details change, details of
    /// the item that was part of a completed order should not change.
    /// </summary>
    public class CatalogItemOrdered // ValueObject
    {
        public CatalogItemOrdered(int catalogItemId, string productName, string pictureUri)
        {
            CatalogItemId = catalogItemId;
            ProductName = productName;
            PictureUri = pictureUri;
        }

        private CatalogItemOrdered()
        {
            // required by EF
        }

        public int CatalogItemId { get; set; }
        public string ProductName { get; set; }
        public string PictureUri { get; set; }
        public object Guard { get; }
    }



    public class Address // ValueObject
    {
        public string Street { get; private set; }

        public string City { get; private set; }

        public string State { get; private set; }

        public string Country { get; private set; }

        public string ZipCode { get; private set; }

        private Address() { }

        public Address(string street, string city, string state, string country, string zipcode)
        {
            Street = street;
            City = city;
            State = state;
            Country = country;
            ZipCode = zipcode;
        }
    }


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

    public class cosmosDBOutput
    {
        public List<OrderItem> OrderDetails { get; set; }

        public Address Address { get; set; }

        public decimal GrandTotal { get; set; }

        public cosmosDBOutput(List<OrderItem> orderDetails, Address address, decimal grandTotal)
        {
            OrderDetails = orderDetails;
            Address = address;
            GrandTotal = grandTotal;
        }
    }
}
