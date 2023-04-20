namespace AzureCrudViaMinimalApi.Hub
{
    public interface IMessageHubClient
    {
        //Task SendOffersToUser(ICollection<FileDataInfo> entity);

        Task SendUpdatedDataViaSignalR(ICollection<FileDataInfo> entity);

    }
}
