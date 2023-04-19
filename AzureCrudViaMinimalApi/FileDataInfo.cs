using Azure.Data.Tables;
using Azure;
namespace AzureCrudViaMinimalApi
{
    public class FileDataInfo : ITableEntity
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public DateTime? Date { get; set; }
        public string Extension { get; set; }
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set ; }
        public int UserId { get; set ; }
        public ETag ETag { get ; set; }
    }
}