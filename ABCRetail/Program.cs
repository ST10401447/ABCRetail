
using ABCRetail.Services;
using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Azure.Storage.Files.Shares;
using Azure.Storage.Queues;
using Microsoft.Extensions.DependencyInjection;

namespace ABCRetail
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            string connectionString = builder.Configuration["AzureStorage:ConnectionString"];

            builder.Services.AddSingleton(new TableServiceClient(connectionString));
            builder.Services.AddSingleton(new BlobServiceClient(connectionString));
            builder.Services.AddSingleton(new QueueServiceClient(connectionString));
            builder.Services.AddSingleton(new ShareServiceClient(connectionString));

            builder.Services.AddHttpClient<TableStorageService>();
            builder.Services.AddHttpClient<BlobStorageService>();
            builder.Services.AddHttpClient<QueueStorageService>();
            builder.Services.AddHttpClient<FileStorageService>();
            builder.Services.AddHttpClient<ProductTableService>();
            builder.Services.AddHttpClient<OrderTableService>();
            

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthorization();

            app.MapStaticAssets();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            app.Run();
        }
    }
}
