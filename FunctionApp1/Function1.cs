using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace FunctionApp1
{
    public class Function1
    {
        [FunctionName("Function1")]
        public void Run([QueueTrigger("myqueue-items", Connection = "DefaultEndpointsProtocol=https;AccountName=bhattstorage;AccountKey=CXC/yu8EY42mFP8tR0Ko5RctoO73rwGL+p9cvcXwAAIJ8UiKkLa0WfyBC0I2cAJV/e9SNrhutY3a+AStooYuPg==;EndpointSuffix=core.windows.net")]string myQueueItem, ILogger log)
        {
            log.LogInformation($"C# Queue trigger function processed: {myQueueItem}");
        }
    }
}
