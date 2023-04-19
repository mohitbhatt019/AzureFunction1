using Azure;

namespace AzureCrudViaMinimalApi
{
    public class EditClass
    {
        public string Name { get; set; }
        public string Extension { get; set; }
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public ETag ETag { get; set; }


    }
}
