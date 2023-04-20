using Microsoft.AspNetCore.Mvc;
using Microsoft.WindowsAzure.Storage.Blob;

namespace AzureCrudViaMinimalApi.Repository.IRepository
{
    public interface ITableStorageRepository
    {       
        Task<FileDataInfo> GetEntityAsync(string fileName, string id);
        Task<FileDataInfo> UpsertEntityAsync(FileDataInfo entity);
        Task<FileDataInfo> CreateEntityAsync(FileDataInfo entity);
        Task<bool> DeleteEntityAsync(string name, string extension, string partitionKey, string rowKey);
        public Task<ICollection<FileDataInfo>> GetAllEntityAsync();
        public Task<ICollection<FileDataInfo>> GetAllEntityForSpecificUser(int id);
        //public Task RenameAsync(CloudBlobContainer container, string oldName, string newName);




    }

}

