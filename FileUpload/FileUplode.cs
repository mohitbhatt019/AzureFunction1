using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.Storage.Queue;
using Microsoft.Azure.Storage;
using Microsoft.Extensions.Azure;
using Azure.Storage.Queues;
using System.Text;
using Azure;

namespace FileUpload
{
    public static class FileUplode
    {
        // This function is triggered when a user submits a file to upload
        // It receives an HttpRequest object and an ILogger object as parameters
        [FunctionName("FileUplode")]
        public static async Task<IActionResult> Run(
           [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req, ILogger log)
        {
            // Get the user ID from the headers of the HttpRequest object
            req.Headers.TryGetValue("userId", out var userId);
            int id = Convert.ToInt32(userId);

            // Get the connection string and container name from the environment variables
            string Connection = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
            string containerName = Environment.GetEnvironmentVariable("ContainerName");

            // Create a BlobServiceClient object to interact with the Azure Blob Storage
            BlobServiceClient blobServiceClient = new BlobServiceClient(Connection);

            // Create a container if it doesn't already exist
            var containerNames = CreateSampleContainerAsync(blobServiceClient, containerName);
            if (containerNames == null)
            {
                return new OkObjectResult("No data");
            }

            // Create a Stream object to store the file data
            Stream myBlob = new MemoryStream();

            // Get the file from the HttpRequest object and open a Stream to read the file data
            var file = req.Form.Files[0];
            myBlob = file.OpenReadStream();

            // Create a BlobClient object to interact with a specific blob in the container
            var blobClient = new BlobContainerClient(Connection, containerName.ToString());
            var blob = blobClient.GetBlobClient(file.FileName);

            // Upload the file data to the blob
            try
            {
                var UploadStatus = await blob.UploadAsync(myBlob);
            }
            catch (Exception)
            {
                throw;
            }
            //Create Queue


            // Add information about the uploaded file to a message queue
            var SendToQueue = CreateQueue(id,file.FileName, Connection, Environment.GetEnvironmentVariable("queueName"));

            // Return an OkObjectResult indicating that the file was uploaded successfully
            return new OkObjectResult("file uploaded successfylly");
        }

        // Create a container
        //-------------------------------------------------
        private static async Task<dynamic> CreateSampleContainerAsync(BlobServiceClient blobServiceClient, string containerName)
        {
            try
            {
                // Create the container
                BlobContainerClient container = await blobServiceClient.CreateBlobContainerAsync(containerName);

                if (await container.ExistsAsync())
                {
                    
                    Console.WriteLine("Created container {0}", container.Name);
                    return container;
                }
            }
            catch (RequestFailedException e)
            {
                Console.WriteLine("HTTP error code {0}: {1}",
                                    e.Status, e.ErrorCode);
                Console.WriteLine(e.Message);
            }
            return null;
        }
        // Create the messagequeue
        private static async Task<bool> CreateQueue(int id, string fileDetail, string Connection, string queueName)

        {
            // here we will split the string in two parts.
            string[] strname = fileDetail.Split('.');

            // here we will create the information that will be store in the queue.
            var fileData = new FileInfo()
            {
                Extension = strname[1],
                Name = strname[0],
                Date = DateTime.Now.ToUniversalTime(),
                UserId = id
            };


            QueueServiceClient queueServiceClient = new QueueServiceClient(Connection);
            try
            {
                // here we will add a message to the queue.
                QueueClient queueClient = new QueueClient(Connection, queueName);
                queueClient.CreateIfNotExists();
                var bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(fileData).ToString());
                await queueClient.SendMessageAsync(Convert.ToBase64String(bytes));
                return true;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        //Download image
        // This code defines an Azure Function that downloads an image from an Azure Blob Storage container.
        // The function is triggered by an HTTP GET request to a specific URL route that includes the image file name.
        // The function returns a Stream of the downloaded image file.
        [FunctionName("DownloadImage")]
        public static async Task<Stream> Runs(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "DownloadImage/{fileName}")] HttpRequest req, string fileName,
            ILogger log)
        {
            // Retrieve connection string and container name from Azure Functions App settings.
            string connection = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
            string containerName = Environment.GetEnvironmentVariable("ContainerName");

            // Create a BlobContainerClient object to interact with the Blob Storage container.
            var container = new BlobContainerClient(connection, containerName);

            // Check if the container exists in the Blob Storage account.
            if (await container.ExistsAsync())
            {
                // Get a BlobClient object for the requested file.
                var blobClient = container.GetBlobClient(fileName);

                // Check if the requested file exists in the container.
                if (blobClient.Exists())
                {
                    // Download the file and return its content as a Stream.
                    var downloadFile = await blobClient.DownloadStreamingAsync();
                    return downloadFile.Value.Content;
                }
                else
                {
                    // If the requested file does not exist in the container, throw a FileNotFoundException.
                    throw new FileNotFoundException();
                }
            }
            else
            {
                // If the container does not exist in the Blob Storage account, throw a FileNotFoundException.
                throw new FileNotFoundException();
            }
        }


    }
}
