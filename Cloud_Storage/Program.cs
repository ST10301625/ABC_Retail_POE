using Azure.Storage.Files.Shares;
using Cloud_Storage.Services;
using Microsoft.EntityFrameworkCore;
using Azure.Data.Tables;  // Add this for Azure Table Storage
using Microsoft.AspNetCore.Authentication.Cookies; // Add this for cookie authentication
using Cloud_Storage.Services;  // Assuming you have TableStorageService in a Services folder

var builder = WebApplication.CreateBuilder(args);

// Access the configuration object
var configuration = builder.Configuration;

// Add services to the container.
builder.Services.AddControllersWithViews();

// Register HttpClient
builder.Services.AddHttpClient(); // Registering HttpClient

// Register BlobService with configuration
builder.Services.AddSingleton(new BlobService(configuration.GetConnectionString("AzureStorage")));

// Register TableStorageService with configuration
builder.Services.AddSingleton(new TableStorageService(configuration.GetConnectionString("AzureStorage")));

// Register QueueService with configuration
builder.Services.AddSingleton<QueueService>(sp =>
{
    var connectionString = configuration.GetConnectionString("AzureStorage");
    return new QueueService(connectionString, "orders");
});

// Register FileShareService with configuration
builder.Services.AddSingleton<AzureFileShareService>(sp =>
{
    var connectionString = configuration.GetConnectionString("AzureStorage");
    return new AzureFileShareService(connectionString, "fileshare");
});

// Register TableClient for Azure Table Storage (for user login and signup)
builder.Services.AddSingleton(sp =>
{
    var connectionString = configuration.GetConnectionString("AzureStorage");
    string tableName = "sign";  // Use the "sign" table for storing user information
    return new TableClient(connectionString, tableName);
});

// Add Cookie Authentication for login system
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Add authentication and authorization middleware
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
