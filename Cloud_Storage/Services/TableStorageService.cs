using Azure;
using Azure.Data.Tables;
using Cloud_Storage.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json; // For using Json extension methods
using System.Threading.Tasks;

public class TableStorageService
{
    private readonly TableClient _customerTableClient;
    private readonly TableClient _orderTableClient;
    private readonly TableClient _productTableClient; // Correctly initialized for Products

    // URL to the Azure Function
    private readonly string _functionUrl = "https://st10301625functionapp.azurewebsites.net/api/GetAllProducts?code=TFpRYZl_ThfDk9RnnYPTeOst2zUSho3ExdedTKh2-w9FAzFu2fGJPw%3D%3D";

    public TableStorageService(string connectionString)
    {
        _customerTableClient = new TableClient(connectionString, "Customers");
        _orderTableClient = new TableClient(connectionString, "Orders");
        _productTableClient = new TableClient(connectionString, "Products"); // Initialize product table client
    }

    // Updated method to call the Azure Function
    public async Task<List<Product>> GetAllProductsAsync()
    {
        var products = new List<Product>();

        using (var httpClient = new HttpClient())
        {
            try
            {
                // Make a GET request to the Azure Function
                var response = await httpClient.GetAsync(_functionUrl);

                if (response.IsSuccessStatusCode)
                {
                    // Deserialize the JSON response to a list of Product
                    products = await response.Content.ReadFromJsonAsync<List<Product>>() ?? new List<Product>();
                }
                else
                {
                    throw new Exception($"Failed to fetch products: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions (e.g., log them)
                throw new Exception($"An error occurred while retrieving products: {ex.Message}", ex);
            }
        }

        return products;
    }

    public async Task AddProductAsync(Product product)
    {
        // Ensure PartitionKey and RowKey are set
        if (string.IsNullOrEmpty(product.PartitionKey) || string.IsNullOrEmpty(product.RowKey))
        {
            throw new ArgumentException("PartitionKey and RowKey must be set.");
        }

        try
        {
            // Add product directly to Table Storage
            await _productTableClient.AddEntityAsync(product);
        }
        catch (RequestFailedException ex)
        {
            // Handle exception as necessary, for example log it or rethrow
            throw new InvalidOperationException("Error adding product to Table Storage", ex);
        }
    }

    public async Task DeleteProductAsync(string partitionKey, string rowKey)
    {
        // Delete product from Table Storage
        await _productTableClient.DeleteEntityAsync(partitionKey, rowKey);
    }

    public async Task<Product?> GetProductAsync(string partitionKey, string rowKey)
    {
        try
        {
            var response = await _productTableClient.GetEntityAsync<Product>(partitionKey, rowKey);
            return response.Value;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            // Handle not found
            return null;
        }
    }

    public async Task<List<Customer>> GetAllCustomersAsync()
    {
        var customers = new List<Customer>();

        await foreach (var customer in _customerTableClient.QueryAsync<Customer>())
        {
            customers.Add(customer);
        }

        return customers;
    }

    public async Task AddCustomerAsync(Customer customer)
    {
        if (string.IsNullOrEmpty(customer.PartitionKey) || string.IsNullOrEmpty(customer.RowKey))
        {
            throw new ArgumentException("PartitionKey and RowKey must be set.");
        }

        try
        {
            await _customerTableClient.AddEntityAsync(customer);
        }
        catch (RequestFailedException ex)
        {
            throw new InvalidOperationException("Error adding customer to Table Storage", ex);
        }
    }

    public async Task DeleteCustomerAsync(string partitionKey, string rowKey)
    {
        await _customerTableClient.DeleteEntityAsync(partitionKey, rowKey);
    }

    public async Task<Customer?> GetCustomerAsync(string partitionKey, string rowKey)
    {
        try
        {
            var response = await _customerTableClient.GetEntityAsync<Customer>(partitionKey, rowKey);
            return response.Value;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return null;
        }
    }

    public async Task AddOrderAsync(Order order)
    {
        if (string.IsNullOrEmpty(order.PartitionKey) || string.IsNullOrEmpty(order.RowKey))
        {
            throw new ArgumentException("PartitionKey and RowKey must be set.");
        }

        try
        {
            await _orderTableClient.AddEntityAsync(order);
        }
        catch (RequestFailedException ex)
        {
            throw new InvalidOperationException("Error adding order to Table Storage", ex);
        }
    }

    public async Task<List<Order>> GetAllOrdersAsync()
    {
        var orders = new List<Order>();

        await foreach (var order in _orderTableClient.QueryAsync<Order>())
        {
            orders.Add(order);
        }

        return orders;
    }

    // Placeholder methods, implement as necessary
    internal async Task<string?> GetCustomerByIdAsync(int customer_ID)
    {
        throw new NotImplementedException();
    }

    internal async Task<string?> GetProductByIdAsync(int product_ID)
    {
        throw new NotImplementedException();
    }
}
