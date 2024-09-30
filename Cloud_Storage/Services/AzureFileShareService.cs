using Azure;
using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;
using Cloud_Storage.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

public class AzureFileShareService
{
    private readonly string _connectionString;
    private readonly string _fileShareName;

    public AzureFileShareService(string connectionString, string fileShareName)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        _fileShareName = fileShareName ?? throw new ArgumentNullException(nameof(fileShareName));
    }

    public async Task UploadFileAsync(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            throw new ArgumentException("No file provided for upload.");
        }

        try
        {
            using (var stream = file.OpenReadStream())
            {
                var content = new MultipartFormDataContent();
                content.Add(new StreamContent(stream), "file", file.FileName);

                string azureFunctionUrl = "http://localhost:7012/api/UploadFile"; // This should match your Azure Function's URL

                using (var httpClient = new HttpClient())
                {
                    var response = await httpClient.PostAsync(azureFunctionUrl, content);

                    if (!response.IsSuccessStatusCode)
                    {
                        throw new Exception($"File upload to Azure Function failed: {response.ReasonPhrase}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // Replace Console.WriteLine with logging
            throw new Exception($"Error uploading file: {ex.Message}", ex);
        }
    }

    public async Task<Stream> DownloadFileAsync(string directoryName, string fileName)
    {
        try
        {
            var serviceClient = new ShareServiceClient(_connectionString);
            var shareClient = serviceClient.GetShareClient(_fileShareName);
            var directoryClient = shareClient.GetDirectoryClient(directoryName);
            var fileClient = directoryClient.GetFileClient(fileName);

            var downloadInfo = await fileClient.DownloadAsync();
            return downloadInfo.Value.Content;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error downloading file: {ex.Message}", ex);
        }
    }

    public async Task<List<FileModel>> ListFilesAsync(string directoryName)
    {
        var fileModels = new List<FileModel>();

        try
        {
            var serviceClient = new ShareServiceClient(_connectionString);
            var shareClient = serviceClient.GetShareClient(_fileShareName);
            var directoryClient = shareClient.GetDirectoryClient(directoryName);

            await foreach (ShareFileItem item in directoryClient.GetFilesAndDirectoriesAsync())
            {
                if (!item.IsDirectory)
                {
                    var fileClient = directoryClient.GetFileClient(item.Name);
                    var properties = await fileClient.GetPropertiesAsync();

                    fileModels.Add(new FileModel
                    {
                        Name = item.Name,
                        Size = properties.Value.ContentLength,
                        LastModified = properties.Value.LastModified
                    });
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Error listing files: {ex.Message}", ex);
        }

        return fileModels;
    }
}
