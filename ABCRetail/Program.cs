
using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Azure.Storage.Files.Shares;
using Azure.Storage.Queues;
using ABCRetail.Services;

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

            builder.Services.AddSingleton<TableStorageService>();
            builder.Services.AddSingleton<BlobStorageService>();
            builder.Services.AddSingleton<QueueStorageService>();
            builder.Services.AddSingleton<FileStorageService>();
            builder.Services.AddSingleton<ProductTableService>();
            builder.Services.AddSingleton<OrderTableService>();

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
