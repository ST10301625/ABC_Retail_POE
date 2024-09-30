using Cloud_Storage.Models;
using Cloud_Storage.Services;
using Microsoft.AspNetCore.Mvc;

public class ProductsController : Controller
{
    private readonly BlobService _blobService;
    private readonly TableStorageService _tableStorageService;

    public ProductsController(BlobService blobService, TableStorageService tableStorageService)
    {
        _blobService = blobService;
        _tableStorageService = tableStorageService;
    }

    [HttpGet]
    public IActionResult AddProduct()
    {
        return View();
    }

    [HttpPost]


    [HttpPost]
    public async Task<IActionResult> AddProduct(Product product, IFormFile file)
    {
        if (file != null)
        {
            // Prepare the request to your Azure Function
            using var formContent = new MultipartFormDataContent();
            using var stream = file.OpenReadStream();

            // Add the file to the form content
            formContent.Add(new StreamContent(stream), "file", file.FileName);

            // Send the request to the Azure Function
            using var httpClient = new HttpClient();
            var response = await httpClient.PostAsync("http://localhost:7012/api/UploadImage", formContent);

            if (response.IsSuccessStatusCode)
            {
                // Get the image URL from the response
                var imageUrl = await response.Content.ReadAsStringAsync();
                product.ImageUrl = imageUrl;
            }
            else
            {
                ModelState.AddModelError("", "Image upload failed.");
                return View(product);
            }
        }

        if (ModelState.IsValid)
        {
            product.PartitionKey = "ProductsPartition";
            product.RowKey = Guid.NewGuid().ToString();
            await _tableStorageService.AddProductAsync(product);
            return RedirectToAction("Index");
        }

        return View(product);
    }

    [HttpPost]
    public async Task<IActionResult> DeleteProduct(string partitionKey, string rowKey, Product product)
    {
        if (product != null && !string.IsNullOrEmpty(product.ImageUrl))
        {
            // Delete the associated blob image
            await _blobService.DeleteBlobAsync(product.ImageUrl);
        }
        // Delete Table entity
        await _tableStorageService.DeleteProductAsync(partitionKey, rowKey);

        return RedirectToAction("Index");
    }

    public async Task<IActionResult> Index()
    {
        var products = await _tableStorageService.GetAllProductsAsync();
        return View(products);
    }
}
