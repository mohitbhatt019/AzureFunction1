using System;
using System.Net.Http;
using System.Text;
using System.Text.Unicode;
using Microsoft.Azure.Documents.SystemFunctions;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace QueueTrigger
{
    public class Trigger
    {
        // This function is triggered by a message in the "myqueue-items" queue, and it deserializes the message to a FileDataInfo object
        [FunctionName("Trigger")]
        public static void Run([QueueTrigger("myqueue-items", Connection = "AzureWebJobsStorage")] string myQueueItem, ILogger log)
        {
            var data = myQueueItem;
            var finalData = JsonConvert.DeserializeObject<FileDataInfo>(myQueueItem);

            // This function is called with the deserialized FileDataInfo object, and it sends the data to a web API to be saved in a table
            sendDataToTable(finalData);
        }

        // This function sends data to a web API to be saved in a table
        private static bool sendDataToTable(FileDataInfo queueData)
        {
            // Create an HTTP client to make a POST request to the web API
            HttpClient httpClient = new HttpClient();

            // Create the request body as a JSON string
            using var content = new StringContent(JsonConvert.SerializeObject(queueData), Encoding.UTF8, "application/json");

            // Send the POST request to the web API and get the response
            HttpResponseMessage response = httpClient.PostAsync(Environment.GetEnvironmentVariable("WebApiLink"), content).Result;

            // Get the status of the request (this is not currently used)
            var dataStatus = response.RequestMessage;

            // Return true to indicate that the data was sent successfully
            return true;
        }


    }
}
