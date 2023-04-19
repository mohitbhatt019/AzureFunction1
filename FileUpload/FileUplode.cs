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
        [FunctionName("FileUplode")]
        public static async Task<IActionResult> Run(
           [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req, ILogger log)
        {
            req.Headers.TryGetValue("userId", out var userId);
            int id = Convert.ToInt32(userId);


            string Connection = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
            string containerName = Environment.GetEnvironmentVariable("ContainerName");
            BlobServiceClient blobServiceClient = new BlobServiceClient(Connection);
            var containerNames = CreateSampleContainerAsync(blobServiceClient, containerName);
            if (containerNames == null)
            {
                return new OkObjectResult("No data");
            }
            Stream myBlob = new MemoryStream();
            var file = req.Form.Files[0];
            myBlob = file.OpenReadStream();
            var blobClient = new BlobContainerClient(Connection, containerName.ToString());
            var blob = blobClient.GetBlobClient(file.FileName);
            try
            {
                var UploadStatus = await blob.UploadAsync(myBlob);
            }
            catch (Exception)
            {
                throw;
            }
            //Create Queue

            var SendToQueue = CreateQueue(id,file.FileName, Connection, Environment.GetEnvironmentVariable("queueName"));


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
        [FunctionName("DownloadImage")]
        public static async Task<Stream> Runs(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "DownloadImage/{fileName}")] HttpRequest req, string fileName,
            ILogger log)
        {
            string connection = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
            string containerName = Environment.GetEnvironmentVariable("ContainerName");
            var container = new BlobContainerClient(connection, containerName);
            if (await container.ExistsAsync())
            {
                var blobClient = container.GetBlobClient(fileName);
                if (blobClient.Exists())
                {


                    var downloadFile = await blobClient.DownloadStreamingAsync();
                    return downloadFile.Value.Content;
                }
                else
                {
                    throw new FileNotFoundException();
                }
            }
            else
            {
                throw new FileNotFoundException();
            }
        }


    }
}
