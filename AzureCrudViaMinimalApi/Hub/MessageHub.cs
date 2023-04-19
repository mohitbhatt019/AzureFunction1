using Microsoft.AspNetCore.SignalR;
using System.Collections.ObjectModel;

namespace AzureCrudViaMinimalApi.Hub
{
    public class MessageHub : Hub<IMessageHubClient>
    {
        public async Task SendOffersToUser(ICollection<FileDataInfo> entity)
        {
            await Clients.All.SendOffersToUser(entity);
        }

       
    }
}
