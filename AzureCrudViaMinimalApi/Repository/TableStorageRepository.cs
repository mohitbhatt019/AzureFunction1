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

        private async Task<TableClient> GetTableClient()
        {
            var serviceClient = new TableServiceClient(_configuration["StorageConnectionString"]);
            var tableClient = serviceClient.GetTableClient(TableName);
            await tableClient.CreateIfNotExistsAsync();
            return tableClient;
        }

        public async Task<bool> DeleteEntityAsync(string name, string extension, string partitionKey, string rowKey)
        {
            var tableClient = await GetTableClient();
            var removeData = tableClient.DeleteEntity(rowKey, partitionKey);
            if (removeData.Status == 204)
            {
                var container = BlobExtensions.GetContainer(_connectionString, _container);
                if (!await container.ExistsAsync())
                {
                    return false;
                }

                var blobClient = container.GetBlobClient(name + "." + extension);

                if (await blobClient.ExistsAsync())
                {
                    await blobClient.DeleteIfExistsAsync();
                    return true;
                }
                else
                {
                    return false;
                }
            }

            return false;
        }
        public static class BlobExtensions
        {
            public static BlobContainerClient GetContainer(string connectionString, string containerName)
            {
                BlobServiceClient blobServiceClient = new(connectionString);
                return blobServiceClient.GetBlobContainerClient(containerName);
            }
        }
        public async Task<FileDataInfo> GetEntityAsync(string fileName, string id)
        {
            var tableClient = await GetTableClient();
            var data = await tableClient.GetEntityAsync<FileDataInfo>(fileName, id);
            return data;
        }
        public async Task<ICollection<FileDataInfo>> GetAllEntityAsync()
        {
            ICollection<FileDataInfo> getAllData = new List<FileDataInfo>();

            var tableClient = await GetTableClient();

            var celebs = tableClient.QueryAsync<FileDataInfo>(filter: "");


            await foreach (var fileDatas in celebs)
            {
                getAllData.Add(fileDatas);
            }
            return getAllData;
        }


        public async Task<FileDataInfo> UpsertEntityAsync(FileDataInfo entity)
        {
            var tableClient = await GetTableClient();
            //await tableClient.UpsertEntityAsync(entity);
            //var getData = await GetAllEntityForSpecificUser(entity.UserId);
            //// SignalR
            //await messageHub.Clients.All.SendOffersToUser(getData);
            //return entity;
            try
            {
               var updatedData= await tableClient.UpsertEntityAsync(entity);
                var getData = await GetAllEntityForSpecificUser(entity.UserId);
                // SignalR
                var objNotifHub = new MessageHub();

                await objNotifHub.SendUpdatedDataViaSignalR(getData, messageHub);
                if (updatedData.Status == 204)
                {
                    //Blob 
                    CloudStorageAccount storageAccount = CloudStorageAccount.Parse(_connectionString);
                    CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

                    CloudBlobContainer container = blobClient.GetContainerReference(_container);
                    CloudBlockBlob sourceBlob = container.GetBlockBlobReference(entity.PartitionKey + "." + entity.Extension);

                    CloudBlockBlob newBlob = container.GetBlockBlobReference(entity.Name + "." + entity.Extension);
                    await newBlob.StartCopyAsync(sourceBlob);

                    while (newBlob.CopyState.Status == CopyStatus.Pending)
                    {
                        await Task.Delay(1000);
                        await newBlob.FetchAttributesAsync();
                    }
                    await sourceBlob.DeleteIfExistsAsync();
                    //***********************************
                   
                    return entity;
                }
                else return null;
               

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

       
        //public async Task<FileDataInfo> UpsertEntityAsync(FileDataInfo entity)
        //{
        //    var tableClient = await GetTableClient();
        //    var updateData =  await tableClient.UpsertEntityAsync(entity);
        //    var updatedData = new FileDataInfo
        //    {
        //        Id = entity.Id,
        //        PartitionKey = entity.PartitionKey,
        //        RowKey = entity.RowKey,
        //        Name = entity.Name,
        //        Extension = entity.Extension,
        //        // add any other updated properties
        //    };
        //    // await messageHub.Clients.All.SendOffersToUser(entity);

        //    await messageHub.Clients.All.SendUpdatedDataToUser(updatedData);
        //    return entity;
        //}





        public async Task<ICollection<FileDataInfo>> GetAllEntityForSpecificUser(int id)
        {
            ICollection<FileDataInfo> getAllData = new List<FileDataInfo>();

            var tableClient = await GetTableClient();


            var celebs = tableClient.QueryAsync<FileDataInfo>(filter: e => e.UserId == id);

            await foreach (var fileDatas in celebs)
            {
                getAllData.Add(fileDatas);
            }
            return getAllData;
        }

        public async Task<FileDataInfo> CreateEntityAsync(FileDataInfo entity)
        {
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
                await tableClient.AddEntityAsync(entity);

                var getData = await GetAllEntityForSpecificUser(entity.UserId);
                // SignalR
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