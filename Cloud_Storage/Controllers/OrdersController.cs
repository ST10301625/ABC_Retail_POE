using Cloud_Storage.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using Cloud_Storage.Services;

public class OrdersController : Controller
{
    private readonly TableStorageService _tableStorageService;
    private readonly QueueService _queueService;

    public OrdersController(TableStorageService tableStorageService, QueueService queueService)
    {
        _tableStorageService = tableStorageService;
        _queueService = queueService;
    }

    // Action to display all orders (optional first)
    public async Task<IActionResult> Index()
    {
        //var orders = await _tableStorageService.GetAllOrdersAsync();
        return View();
    }

    public async Task<IActionResult> Register()
    {
        var customers = await _tableStorageService.GetAllCustomersAsync();
        var products = await _tableStorageService.GetAllProductsAsync();

        // Check for null or empty lists
        if (customers == null || customers.Count == 0)
        {
            // Handle the case where no customers are found
            ModelState.AddModelError("", "No customers found. Please add customers first.");
            return View(); // Or redirect to another action
        }

        if (products == null || products.Count == 0)
        {
            // Handle the case where no products are found
            ModelState.AddModelError("", "No products found. Please add products first.");
            return View(); // Or redirect to another action
        }

        ViewData["Customers"] = customers;
        ViewData["Products"] = products;

        return View();
    }

    // Action to handle the form submission and register the order
    [HttpPost]
    public async Task<IActionResult> Register(Order order)
    {
        if (ModelState.IsValid)
        {
            // TableService
            order.Order_Date = DateTime.SpecifyKind(order.Order_Date, DateTimeKind.Utc);
            order.PartitionKey = "OrdersPartition";
            order.RowKey = Guid.NewGuid().ToString();
            await _tableStorageService.AddOrderAsync(order);

            // MessageQueue
            string message = $"New order by Customer {order.Customer_ID} of Product {order.Product_ID} at {order.Order_Location} on {order.Order_Date}";
            await _queueService.SendMessageAsync(message);

            return RedirectToAction("Index");
        }
        else
        {
            // Log model state errors
            foreach (var error in ModelState)
            {
                Console.WriteLine($"Key: {error.Key}, Errors: {string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage))}");
            }
        }

        // Reload customers and products lists if validation fails
        var customers = await _tableStorageService.GetAllCustomersAsync();
        var products = await _tableStorageService.GetAllProductsAsync();
        ViewData["Customers"] = customers;
        ViewData["Products"] = products;

        return View(order);
    }
}
