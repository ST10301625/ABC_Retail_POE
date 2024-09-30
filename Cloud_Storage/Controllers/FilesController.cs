using Cloud_Storage.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

public class FilesController : Controller
{
    private readonly AzureFileShareService _fileShareService;
    private readonly HttpClient _httpClient; // Add HttpClient field

    public FilesController(AzureFileShareService fileShareService, HttpClient httpClient) // Inject HttpClient
    {
        _fileShareService = fileShareService;
        _httpClient = httpClient; // Assign the injected HttpClient
    }

    public async Task<IActionResult> Index()
    {
        List<FileModel> files;
        try
        {
            files = await _fileShareService.ListFilesAsync("uploads");
        }
        catch (Exception ex)
        {
            ViewBag.Message = $"Failed to load files: {ex.Message}";
            files = new List<FileModel>();
        }

        return View(files);
    }

    // Upload file to azure function
    [HttpPost]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            ModelState.AddModelError("file", "Please select a file to upload.");
            return await Index();
        }

        try
        {
            using (var stream = file.OpenReadStream())
            {
                // Prepare the content to send to the Azure Function
                var content = new MultipartFormDataContent();
                content.Add(new StreamContent(stream), "file", file.FileName);

                // Call the Azure Function - replace with your actual function URL
                string azureFunctionUrl = "http://localhost:7012/api/UploadFile"; // Consider moving this to appsettings

                var response = await _httpClient.PostAsync(azureFunctionUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Message"] = $"File '{file.FileName}' uploaded successfully!";
                }
                else
                {
                    TempData["Message"] = $"File upload failed: {response.ReasonPhrase}";
                }
            }
        }
        catch (Exception ex)
        {
            TempData["Message"] = $"File upload failed: {ex.Message}";
        }

        return RedirectToAction("Index");
    }

    // Handle file download
    [HttpGet]
    public async Task<IActionResult> DownloadFile(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            return BadRequest("File name cannot be null or empty.");
        }

        try
        {
            var fileStream = await _fileShareService.DownloadFileAsync("uploads", fileName);

            if (fileStream == null)
            {
                return NotFound($"File '{fileName}' not found.");
            }

            return File(fileStream, "application/octet-stream", fileName);
        }
        catch (Exception ex)
        {
            return BadRequest($"Error downloading file: {ex.Message}");
        }
    }
}


