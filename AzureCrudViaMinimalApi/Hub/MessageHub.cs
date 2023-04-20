using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;

namespace AzureCrudViaMinimalApi.Hub
{
    //MessageHub class is derived from the Hub class provided by the SignalR library. It defines several methods and properties that are
    //used to communicate with clients over a WebSocket connection.
    public class MessageHub : Hub<IMessageHubClient>
    {
        //The Users field is a thread-safe dictionary that is used to store the UserHubModels objects for each connected client.
        private static readonly ConcurrentDictionary<string, UserHubModels> Users = new ConcurrentDictionary<string, UserHubModels>
                                                                                         (StringComparer.InvariantCultureIgnoreCase);

        //The SignalR method is called by the server to send a message to specific users identified by the UserId property of the entity parameter.
        //It retrieves a list of connections that belong to the specified users, then sends the message to those connections using the SignalR method
        //provided by the IMessageHubClient interface.
        public async Task SendUpdatedDataViaSignalR(ICollection<FileDataInfo> data, IHubContext<MessageHub, IMessageHubClient> messageHub)
        {
            var getspecificUserDictionary = Users.Where(m => m.Value.id == data.FirstOrDefault().UserId.ToString());
            IReadOnlyList<string> connections;
            var list = new List<string>();

            foreach (var connection in getspecificUserDictionary)
            {

                list.Add(connection.Value.connectionId.ToString());
            }
            connections = list;
            await messageHub.Clients.Clients(connections).SendUpdatedDataViaSignalR(data);
        }


        //The OnConnectedAsync method is called when a client connects to the server.
        //It creates a new UserHubModels object and adds it to the Users dictionary.
        public async override Task OnConnectedAsync()
        {
            var userhubModels = new UserHubModels()
            {
                connectionId = Context.ConnectionId
            };

            Users.TryAdd(Context.ConnectionId.ToString(), userhubModels);
            await base.OnConnectedAsync();
        }

        //The GetUserId method is called by the client to inform the server of their user ID.
        //It updates the UserHubModels object for the current connection with the specified user ID.
        public void GetUserId(string id)
        {
            if (id == "") return;
            //now we will check int the users that owr conneciton id present or not
            Users.Remove(Context.ConnectionId, out var data);
            if (data != null)
            {
                data.id = id;
                Users.TryAdd(Context.ConnectionId.ToString(), data);
            }
        }


        //The OnDisconnectedAsync method is called when a client disconnects from the server.
        //It removes the UserHubModels object for the current connection from the Users dictionary.
        public async override Task OnDisconnectedAsync(Exception exception)
        {
            Users.Remove(Context.ConnectionId, out UserHubModels userhub);
            await base.OnDisconnectedAsync(exception);
        }

        //SendOffersToUser is used to send notification to all clients
        //public async Task SendOffersToUser(ICollection<FileDataInfo> entity)
        //{
        //    await Clients.All.SendOffersToUser(entity);
        //}
        /////////////////


    }

    //It is a simple model class that is used to store information about a user, including their connection ID and user ID.
    public class UserHubModels
    {
        public string id { get; set; } = default(string);
        public string connectionId { get; set; }
    }
}
