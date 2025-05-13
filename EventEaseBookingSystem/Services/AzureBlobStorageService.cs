using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;

namespace EventEaseBookingSystem.Services
{
    public class AzureBlobStorageService
    {
        private readonly string _connectionString;

        public AzureBlobStorageService(IConfiguration config)
        {
            _connectionString = BuildConnectionString(config);
        }

        private string BuildConnectionString(IConfiguration config)
        {
            var accountName = config["AzureBlobStorage:AccountName"] ?? "eventeaseblobpt2";
            var accountKey = config["AzureBlobStorage:AccountKey"];

            return $"DefaultEndpointsProtocol=https;AccountName={accountName};AccountKey={accountKey};EndpointSuffix=core.windows.net";
        }

        // ✅ NEW RECOMMENDED METHOD: takes containerName AND fileName
        public async Task<string> UploadFileAsync(IFormFile file, string containerName, string fileName)
        {
            if (file == null || file.Length == 0)
                return string.Empty;

            var containerClient = new BlobContainerClient(_connectionString, containerName);
            await containerClient.CreateIfNotExistsAsync();

            var blobClient = containerClient.GetBlobClient(fileName);

            using var stream = file.OpenReadStream();
            await blobClient.UploadAsync(stream, true); // overwrite = true

            return blobClient.Uri.ToString();
        }

        // Original method (optional, if you want to keep it for backwards compatibility)
        public async Task<string> UploadFileAsync(IFormFile file, string containerName)
        {
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            return await UploadFileAsync(file, containerName, fileName);
        }

        public async Task DeleteFileAsync(string blobUrl)
        {
            if (string.IsNullOrWhiteSpace(blobUrl))
                return;

            // Extract file name from URL
            var fileName = Path.GetFileName(new Uri(blobUrl).LocalPath);
            var containerClient = new BlobContainerClient(_connectionString, _containerName);
            var blobClient = containerClient.GetBlobClient(fileName);

            await blobClient.DeleteIfExistsAsync();
        }

        // This field is removed from constructor to allow flexible container name usage
        private readonly string _containerName = "venues"; // Optional default if you need it
    }
}
