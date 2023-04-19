using Azure;
using Azure.Data.Tables;

namespace AzureCrudViaMinimalApi
{
    public class UserCredentials: ITableEntity
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string PartitionKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
        public string? UserId { get; set; }         
        public string RowKey { get; set; }
    }
}
