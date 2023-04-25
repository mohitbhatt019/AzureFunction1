using Azure;
using Azure.Data.Tables;
using Azure.Storage.Blobs;
using AzureCrudViaMinimalApi.Hub;
using AzureCrudViaMinimalApi.Repository.IRepository;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Azure.Documents;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.ComponentModel;

namespace AzureCrudViaMinimalApi.Repository
{
    public class TableStorageRepository : ITableStorageRepository
    {
        private const string TableName = "mytable";
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;
        private readonly string _container;
        private IHubContext<MessageHub, IMessageHubClient> messageHub;

        public TableStorageRepository(IHubContext<MessageHub, IMessageHubClient> _messageHub, IConfiguration configuration)


        {
            messageHub = _messageHub;
            _configuration = configuration;
            _connectionString = configuration.GetValue<string>("StorageConnectionString");
            _container = configuration.GetValue<string>("ContainerName");

        }

        // This method returns a TableClient object to perform CRUD operations on the Azure Storage Table.
        // It creates the table if it doesn't already exist.
        private async Task<TableClient> GetTableClient()
        {
            // Creating a TableServiceClient object by passing the connection string.
            var serviceClient = new TableServiceClient(_configuration["StorageConnectionString"]);

            // Retrieving the TableClient object by passing the table name and checking if the table exists or not.
            var tableClient = serviceClient.GetTableClient(TableName);
            await tableClient.CreateIfNotExistsAsync();
            return tableClient;
        }

        // This method deletes an entity from Azure Storage Table and its corresponding blob from the blob storage.
        public async Task<dynamic> DeleteEntityAsync(string name, string extension, string partitionKey, string rowKey)
        {
            // Retrieving the TableClient object to perform delete operation.
            var tableClient = await GetTableClient();
            var userDetails = await GetEntityAsync(name, partitionKey);
            if (userDetails == null) return false;
            // Deleting the entity from the Azure Storage Table and storing the result in removeData.
            var removeData = tableClient.DeleteEntity(name, partitionKey);

            // Checking if the deletion was successful or not.
            if (removeData.Status == 204)
            {
                // Retrieving the container from blob storage and checking if it exists.
                var container = BlobExtensions.GetContainer(_connectionString, _container);
                if (!await container.ExistsAsync())
                {
                    return false;
                }
                
                // Retrieving the blobClient to delete the blob from blob storage.
                var blobClient = container.GetBlobClient(name + "." + extension);

                // Checking if the blob exists or not.
                if (await blobClient.ExistsAsync())
                {
                    await blobClient.DeleteIfExistsAsync();
                    var getData = await GetAllEntityForSpecificUser(userDetails.UserId);
                    
                    if (getData.Count == 0)
                    {
                        return getData;
                    }
                    //SignalR Implemented
                    var objNotifHub = new MessageHub();
                    await objNotifHub.SendUpdatedDataViaSignalR(getData, messageHub);
                    return true;
                }
                else
                {
                    return false;
                }
            }

            return false;
        }

        // A utility class that provides an extension method to get BlobContainerClient using the connection string and container name.
        public static class BlobExtensions
        {
            public static BlobContainerClient GetContainer(string connectionString, string containerName)
            {
                BlobServiceClient blobServiceClient = new(connectionString);
                return blobServiceClient.GetBlobContainerClient(containerName);
            }
        }

        // A method that returns the entity data with the specified filename and id.
        public async Task<FileDataInfo> GetEntityAsync(string fileName, string id)
        {
            // get the table client using the connection string and table name.
            var tableClient = await GetTableClient();

            // get the entity with the specified filename and id.
            var data =  tableClient.GetEntity<FileDataInfo>(fileName, id);
            return data;
        }

        // A method that returns all entities in the table.
        public async Task<ICollection<FileDataInfo>> GetAllEntityAsync()
        {
            ICollection<FileDataInfo> getAllData = new List<FileDataInfo>();

            // get the table client using the connection string and table name.
            var tableClient = await GetTableClient();

            // execute a query to get all entities in the table.
            var celebs = tableClient.QueryAsync<FileDataInfo>(filter: "");

            // iterate through the results and add them to the collection.
            await foreach (var fileDatas in celebs)
            {
                getAllData.Add(fileDatas);
            }
            return getAllData;
        }


        //public async Task<FileDataInfo> UpsertEntityAsync(FileDataInfo entity)
        //{
        //    var tableClient = await GetTableClient();
        //    //await tableClient.UpsertEntityAsync(entity);
        //    //var getData = await GetAllEntityForSpecificUser(entity.UserId);
        //    //// SignalR
        //    //await messageHub.Clients.All.SendOffersToUser(getData);
        //    //return entity;
        //    try
        //    {
        //       var updatedData= await tableClient.UpsertEntityAsync(entity);
        //        var getData = await GetAllEntityForSpecificUser(entity.UserId);
        //        // SignalR
        //        var objNotifHub = new MessageHub();

        //        await objNotifHub.SendUpdatedDataViaSignalR(getData, messageHub);
        //        if (updatedData.Status == 204)
        //        {
        //            //Blob 
        //            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(_connectionString);
        //            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

        //            CloudBlobContainer container = blobClient.GetContainerReference(_container);
        //            CloudBlockBlob sourceBlob = container.GetBlockBlobReference(entity.PartitionKey + "." + entity.Extension);

        //            CloudBlockBlob newBlob = container.GetBlockBlobReference(entity.Name + "." + entity.Extension);
        //            await newBlob.StartCopyAsync(sourceBlob);

        //            while (newBlob.CopyState.Status == CopyStatus.Pending)
        //            {
        //                await Task.Delay(1000);
        //                await newBlob.FetchAttributesAsync();
        //            }
        //            await sourceBlob.DeleteIfExistsAsync();
        //            //***********************************

        //            return entity;
        //        }
        //        else return null;


        //    }
        //    catch (Exception ex)
        //    {

        //        throw ex;
        //    }
        //}


        // Upserts a FileDataInfo entity
        public async Task<FileDataInfo> UpsertEntityAsync(FileDataInfo entity)
        {
            // Returns a TableClient for the specified table
            var tableClient = await GetTableClient();
            try
            {              
                    // Copy the source blob to a new blob with a different name
                    BlobServiceClient blobClient = new BlobServiceClient(_connectionString);
                    BlobContainerClient container = blobClient.GetBlobContainerClient(_container);

                    BlobClient sourceBlob = container.GetBlobClient(entity.PartitionKey + "." + entity.Extension);
                    BlobClient newBlob = container.GetBlobClient(entity.Name + "." + entity.Extension);

                    await newBlob.StartCopyFromUriAsync(sourceBlob.Uri);
                    await sourceBlob.DeleteIfExistsAsync();
                // delete from table pervious file name
                await tableClient.DeleteEntityAsync(entity.PartitionKey, entity.RowKey);

                // update the partition key so that new file record created on table .
                entity.PartitionKey = entity.Name;

                var updatedData = await tableClient.UpsertEntityAsync(entity);
                    var getData = await GetAllEntityForSpecificUser(entity.UserId);
                    // Send a SignalR message to update the client-side UI with the updated data
                    var objNotifHub = new MessageHub();
                    await objNotifHub.SendUpdatedDataViaSignalR(getData, messageHub);

                    return entity;
            }
            catch (Exception ex)
            {
                throw ;
            }
        }

        // Retrieves all FileDataInfo entities for a specific user
        public async Task<ICollection<FileDataInfo>> GetAllEntityForSpecificUser(int id)
        {
            ICollection<FileDataInfo> getAllData = new List<FileDataInfo>();

            var tableClient = await GetTableClient();

            // Query Azure Table Storage for all FileDataInfo entities with the specified user ID
            var celebs = tableClient.QueryAsync<FileDataInfo>(filter: e => e.UserId == id);

            await foreach (var fileDatas in celebs)
            {
                getAllData.Add(fileDatas);
            }
            return getAllData;
        }

        // Creates a new FileDataInfo entity
        public async Task<FileDataInfo> CreateEntityAsync(FileDataInfo entity)
        {
            // Generate a new ID for the entity and set its partition and row keys
            entity.PartitionKey = entity.Name;
                string Id = Guid.NewGuid().ToString();
                entity.Id = Id;
                entity.RowKey = Id;
            var tableClient = await GetTableClient();
            //await tableClient.UpsertEntityAsync(entity);
            //var getData = await GetAllEntityForSpecificUser(entity.UserId);
            //// SignalR
            //await messageHub.Clients.All.SendOffersToUser(getData);
            //return entity;
            try
            {
                // Add the entity to Azure Table Storage
                await tableClient.AddEntityAsync(entity);

                // Retrieve all FileDataInfo entities for the same user
                var getData = await GetAllEntityForSpecificUser(entity.UserId);

                // Send a SignalR message to update the client-side UI with the updated data
                var objNotifHub = new MessageHub();

                await objNotifHub.SendUpdatedDataViaSignalR(getData, messageHub);
                return entity;

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
    }
}