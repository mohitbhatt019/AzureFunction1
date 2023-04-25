using Azure.Data.Tables;
using AzureCrudViaMinimalApi;
using AzureCrudViaMinimalApi.Repository.IRepository;
using Microsoft.Azure.Documents;

namespace crudWithAzure.Data
{
    public class AuthenticateRepository : IAuthenticateRepository
    {
        private const string TableName = "Login";
        private readonly IConfiguration _configuration;
        public AuthenticateRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // This method retrieves a TableClient for the specified table name using the StorageConnectionString
        // defined in the application's configuration. If the table does not exist, it creates it.
        private async Task<TableClient> GetTableClient()
        {
            var serviceClient = new TableServiceClient(_configuration["StorageConnectionString"]);
            var tableClient = serviceClient.GetTableClient(TableName);
            await tableClient.CreateIfNotExistsAsync();
            return tableClient;
        }

        // This method authenticates a user by querying the user credentials table for the specified username and password.
        // If a matching user is found, it returns the UserCredentials object for that user, otherwise it returns null.
        public UserCredentials? authenticateUser(string userName, string password)
        {
            try
            {
                var getServiceClient = new TableServiceClient(_configuration["StorageConnectionString"]);
                var tableClient = getServiceClient.GetTableClient(TableName);

                // Query the user credentials table for the specified username and password
                // check enter details correct or not
                var checkUser = tableClient.Query<UserCredentials>(m => m.Username == userName && m.Password == password);
                if (checkUser.FirstOrDefault() != null)
                {
                    // Return the UserCredentials object for the matching user
                    return checkUser.FirstOrDefault();
                }
                else
                {
                    // No matching user was found
                    return null;
                }
            }
            catch (Exception ex)
            {
                // An error occurred while authenticating the user
                return null;
            }

        }

        // This method authenticates a user by querying the user credentials table for the specified username and password.
        // If a matching user is found, it returns the RowKey for that user, otherwise it returns null.
        public async Task<string> AuthenticateUser(string username, string password)
        {
            var tableClient = await GetTableClient();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                // If the username or password is null or empty, return a NotFound result
                Results.NotFound();
            }
            //var data =  await tableClient.GetEntityIfExistsAsync<UserCredentials>(username, password);
            //if (data.Value.Username == username && data.Value.Password == password) return true;

            //else return false;

            var rowKey = await GetRowKeyByUsername(username);
            //var chk = await GetUserswithIdByRowKey(rowKey);

            var data = await GetAllUsers();


            foreach (var user in data)
            {
                if ((user.Username == username && user.Password == password))
                {
                    // If a matching user is found, return their RowKey
                    return rowKey;
                }
                else continue;

            }
            // No matching user was found
            return rowKey;
        }

        // This method retrieves the RowKey for the specified username by querying the user credentials table.
        public async Task<string> GetRowKeyByUsername(string username)
        {
            var tableClient = await GetTableClient();
            var queryResult = tableClient.QueryAsync<UserCredentials>(filter: e => e.Username == username);

            await foreach (var entity in queryResult)
            {
                // Return the RowKey for the first matching user
                return entity.RowKey;
            }
            // No matching user was found
            return null; // or throw an exception, depending on your requirements
        }

        // This method retrieves all users from the user credentials table.
        public async Task<ICollection<UserCredentials>> GetAllUsers()
        {
            ICollection<UserCredentials> GetAllData = new List<UserCredentials>();
            var tableClient = await GetTableClient();

            // Query the user credentials table for all users
            var celebs = tableClient.QueryAsync<UserCredentials>(filter: "");


            await foreach (var fileDatas in celebs)
            {
                // Add each user to the GetAllData list
                GetAllData.Add(fileDatas);
            }
            // Return the list of all users
            return GetAllData;
        }

       

        //public async Task<ICollection<User>> GetAllEntityAsync()
        //{
        //    ICollection<User> getAllData = new List<User>();

        //    var tableClient = await GetTableClient();

        //    var celebs = tableClient.QueryAsync<User>(filter: "");


        //    await foreach (var fileDatas in celebs)
        //    {
        //        getAllData.Add(fileDatas);
        //    }
        //    return getAllData;
        //}

        //public async Task<bool> AuthenticateUser(string username, string password)
        //{
        //    var tableClient = await GetTableClient();

        //    if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        //    {
        //        Results.NotFound();
        //    }
        //    //var data =  await tableClient.GetEntityIfExistsAsync<UserCredentials>(username, password);
        //    //if (data.Value.Username == username && data.Value.Password == password) return true;

        //    //else return false;
        //    var data = await GetAllUsers();
        //    foreach (var user in data)
        //    {
        //        if ((user.UserName == username && user.Password == password))
        //        {
        //            return true;
        //        }
        //        else continue;

        //    }
        //    return false;


        //}

        //public async Task<ICollection<User>> GetAllUsers()
        //{
        //    ICollection<User> getAllData = new List<User>();

        //    var tableClient = await GetTableClient();

        //    var celebs = tableClient.QueryAsync<User>(filter: "");


        //    await foreach (var fileDatas in celebs)
        //    {
        //        getAllData.Add(fileDatas);
        //    }
        //    return getAllData;
        //}
    }
}


       
        




      

   