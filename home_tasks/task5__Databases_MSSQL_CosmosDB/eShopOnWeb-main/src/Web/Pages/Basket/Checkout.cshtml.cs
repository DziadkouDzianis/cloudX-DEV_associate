using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate;
using Microsoft.eShopWeb.ApplicationCore.Exceptions;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.Infrastructure.Identity;
using Microsoft.eShopWeb.Web.Interfaces;
using AzFunctions;
using System.Web;

using Newtonsoft.Json;
using System.Text.Json;


//service bus
using System;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;


namespace Microsoft.eShopWeb.Web.Pages.Basket;

[Authorize]
public class CheckoutModel : PageModel
{
    private readonly IBasketService _basketService;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IOrderService _orderService;
    private string _username = null;
    private object itemId;
    private readonly IBasketViewModelService _basketViewModelService;
    private readonly IAppLogger<CheckoutModel> _logger;

    public CheckoutModel(IBasketService basketService,
        IBasketViewModelService basketViewModelService,
        SignInManager<ApplicationUser> signInManager,
        IOrderService orderService,
        IAppLogger<CheckoutModel> logger)
    {
        _basketService = basketService;
        _signInManager = signInManager;
        _orderService = orderService;
        _basketViewModelService = basketViewModelService;
        _logger = logger;
    }

    public BasketViewModel BasketModel { get; set; } = new BasketViewModel();


    //public class cosmosDBOutput
    //{
    //    public  List<OrderItem> _IOrderService;
    //    public  Address _shippingAddress;

    //    public cosmosDBOutput(List<OrderItem> IOrderServiceC,
    //                          Address shippingAddress)
    //    {
    //        _IOrderService = IOrderServiceC;
    //        _shippingAddress = shippingAddress;
    //    }
    //}

    public async Task OnGet()
    {
        await SetBasketModelAsync();
    }

    public async Task<IActionResult> OnPost(IEnumerable<BasketItemViewModel> items, object itemId)
    {
        try
        {
            await SetBasketModelAsync();

            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var updateModel = items.ToDictionary(b => b.Id.ToString(), b => b.Quantity);
            var updateModel2 = items.Select(b => {
                return new { Id = b.Id, Quantity = b.Quantity };
            });

            Address address = new Address("123 Main St.", "Kent", "OH", "United States", "44240");

            await _basketService.SetQuantities(BasketModel.Id, updateModel);
            //var tmp = await _orderService.CreateOrderAsync(BasketModel.Id, new Address("123 Main St.", "Kent", "OH", "United States", "44240"));
            var tmp = await _orderService.CreateOrderAsync(BasketModel.Id, address);
            await _basketService.DeleteBasketAsync(BasketModel.Id);
            //  LINQ
            //tmp.Select(x => x.Multiply).ToList();
            //var vaarsd =tmp.Select(x => new {ItemOrdered = x.ItemOrdered, Multiply = x.UnitPrice * x.Units }).ToList();

            //Calculating grand total 
            var grandTotal = 0m;
            foreach (var item in tmp)
            {
                grandTotal += item.UnitPrice * item.Units;
            };

            var serializeJsonObjectOIR = JsonConvert.SerializeObject(updateModel2);

            cosmosDBOutput cosmosDB = new cosmosDBOutput(tmp, address, grandTotal);

            //var serializeJsonObject = JsonConvert.SerializeObject(cosmosDB, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true})););
            //var serializeJsonObjectTMP = JsonSerializer.Serialize(vaarsd);

            string urlOIR = "https://orderitemsreserver-vsp.azurewebsites.net/api/OrderItemsReserverFunction?name=" + HttpUtility.UrlEncode(serializeJsonObjectOIR);
            //v1. Do a http call of some url
            using var client = new HttpClient();
            var contentOIR = await client.GetStringAsync(urlOIR);

//Azure function OrderProcessor
            //string url = "https://orderprocessor-ddziadkou.azurewebsites.net/api/InsertOrderItemTrigger?name=" + HttpUtility.UrlEncode(serializeJsonObject); // HttpUtility.UrlEncode(serializeJsonObject);
            var serializeJsonObject = System.Text.Json.JsonSerializer.Serialize(cosmosDB);
            string url = "https://orderprocessor-ddziadkou.azurewebsites.net/api/InsertOrderItemTrigger?name=" + System.Uri.EscapeDataString(serializeJsonObject); // HttpUtility.UrlEncode(serializeJsonObject);
            var content = await client.GetStringAsync(url);	//op
            ///https://orderprocessor-ddziadkou.azurewebsites.net/api/InsertOrderItemTrigger?name=%7B%22OrderDetails%22%3A%5B%7B%22ItemOrdered%22%3A%7B%22CatalogItemId%22%3A1%2C%22ProductName%22%3A%22.NET%20Bot%20Black%20Sweatshirt%22%2C%22PictureUri%22%3A%22%2Fimages%2Fproducts%2F1.png%22%7D%2C%22UnitPrice%22%3A19.50%2C%22Units%22%3A1%2C%22Cost%22%3A19.50%2C%22Id%22%3A9%7D%5D%2C%22Address%22%3A%7B%22Street%22%3A%22123%20Main%20St.%22%2C%22City%22%3A%22Kent%22%2C%22State%22%3A%22OH%22%2C%22Country%22%3A%22United%20States%22%2C%22ZipCode%22%3A%2244240%22%7D%2C%22GrandTotal%22%3A19.50%7D




//Azure Service Bus procedure
            // connection string to your Service Bus namespace
            string connectionString = "Endpoint=sb://ddziadkousb.servicebus.windows.net/;SharedAccessKeyName=ddziadkou_queue_send;SharedAccessKey=sADpSg1NQQ24cR3ubxbUNHwNz6UT0JXnynfCZEEQaQg=;EntityPath=ddziadkou_queue";

            // name of your Service Bus queue
            string queueName = "ddziadkou_queue";

            // the client that owns the connection and can be used to create senders and receivers
            ServiceBusClient clientSB;

            // the sender used to publish messages to the queue
            ServiceBusSender sender;

            // number of messages to be sent to the queue

            // Create the clients that we'll use for sending and processing messages.
            clientSB = new ServiceBusClient(connectionString);
            sender = clientSB.CreateSender(queueName);

            // create a batch 
            using ServiceBusMessageBatch messageBatch = await sender.CreateMessageBatchAsync();

            messageBatch.TryAddMessage(new ServiceBusMessage($"Message Azure Service Bus to the queue: {serializeJsonObjectOIR}"));

            try
            {
                // Use the producer client to send the batch of messages to the Service Bus queue
                await sender.SendMessagesAsync(messageBatch);
            }
            finally
            {
                // Calling DisposeAsync on client types is required to ensure that network
                // resources and other unmanaged objects are properly cleaned up.
                await sender.DisposeAsync();
                await clientSB.DisposeAsync();
            }




    //var content = await client.GetStringAsync(url, new StringContent(HttpUtility.UrlEncodeUnicode(serializeJsonObject)));

    ////v2 Call the Azure Function
    //ActionContext c = new ActionContext();
    //await OrderItemsReserver.Run(serializeJsonObject, "it works", c));


}
        catch (EmptyBasketOnCheckoutException emptyBasketOnCheckoutException)
        {
            //Redirect to Empty Basket page
            _logger.LogWarning(emptyBasketOnCheckoutException.Message);
            return RedirectToPage("/Basket/Index");
        }

        return RedirectToPage("Success");
    }

    private async Task SetBasketModelAsync()
    {
        if (_signInManager.IsSignedIn(HttpContext.User))
        {
            BasketModel = await _basketViewModelService.GetOrCreateBasketForUser(User.Identity.Name);
        }
        else
        {
            GetOrSetBasketCookieAndUserName();
            BasketModel = await _basketViewModelService.GetOrCreateBasketForUser(_username);
        }
    }

    private void GetOrSetBasketCookieAndUserName()
    {
        if (Request.Cookies.ContainsKey(Constants.BASKET_COOKIENAME))
        {
            _username = Request.Cookies[Constants.BASKET_COOKIENAME];
        }
        if (_username != null) return;

        _username = Guid.NewGuid().ToString();
        var cookieOptions = new CookieOptions();
        cookieOptions.Expires = DateTime.Today.AddYears(10);
        Response.Cookies.Append(Constants.BASKET_COOKIENAME, _username, cookieOptions);
    }
}
