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
        [FunctionName("Trigger")]
        public static void Run([QueueTrigger("myqueue-items", Connection = "AzureWebJobsStorage")] string myQueueItem, ILogger log)
        {
            var data = myQueueItem;
            var finalData = JsonConvert.DeserializeObject<FileDataInfo>(myQueueItem);
            sendDataToTable(finalData);
        }

        private static bool sendDataToTable(FileDataInfo queueData)
        {
            // now we will call owr web api to send the data to the table.
            HttpClient httpClient = new HttpClient();
            using var content = new StringContent(JsonConvert.SerializeObject(queueData), Encoding.UTF8, "application/json");

           
            HttpResponseMessage response = httpClient.PostAsync(Environment.GetEnvironmentVariable("WebApiLink"), content).Result;

          
            var dataStatus = response.RequestMessage;


            return true;
        }


    }
}
