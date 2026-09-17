using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Azure.Storage.Files.Shares;
using Azure.Storage.Queues;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using ABCRetail.Services;

namespace ABCRetail.Functions
{
    public class Program
    {
        public static void Main(string[] args)
        {
            IHostBuilder hostBuilder = new HostBuilder();

            hostBuilder.ConfigureFunctionsWorkerDefaults();

            hostBuilder.ConfigureAppConfiguration(AddEnvironmentVariablesToConfig);

            hostBuilder.ConfigureServices(RegisterServices);

            IHost host = hostBuilder.Build();

            host.Run();
        }

        private static void AddEnvironmentVariablesToConfig(IConfigurationBuilder config)
        {
            config.AddEnvironmentVariables();
        }

        private static void RegisterServices(HostBuilderContext context, IServiceCollection services)
        {
            string connectionString = context.Configuration["AzureStorage:ConnectionString"];

            TableServiceClient tableServiceClient = new TableServiceClient(connectionString);
            BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);
            QueueServiceClient queueServiceClient = new QueueServiceClient(connectionString);
            ShareServiceClient shareServiceClient = new ShareServiceClient(connectionString);

            services.AddSingleton(tableServiceClient);
            services.AddSingleton(blobServiceClient);
            services.AddSingleton(queueServiceClient);
            services.AddSingleton(shareServiceClient);

            services.AddSingleton<TableStorageService>();
            services.AddSingleton<BlobStorageService>();
            services.AddSingleton<QueueStorageService>();
            services.AddSingleton<FileStorageService>();
        }
    }
}