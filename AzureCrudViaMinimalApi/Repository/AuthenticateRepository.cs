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

        // Create table here
        private async Task<TableClient> GetTableClient()
        {
            var serviceClient = new TableServiceClient(_configuration["StorageConnectionString"]);
            var tableClient = serviceClient.GetTableClient(TableName);
            await tableClient.CreateIfNotExistsAsync();
            return tableClient;
        }

        // login user code
        public UserCredentials? authenticateUser(string userName, string password)
        {
            try
            {
                var getServiceClient = new TableServiceClient(_configuration["StorageConnectionString"]);
                var tableClient = getServiceClient.GetTableClient(TableName);
                // check enter details correct or not
                var checkUser = tableClient.Query<UserCredentials>(m => m.Username == userName && m.Password == password);
                if (checkUser.FirstOrDefault() != null)
                {
                    return checkUser.FirstOrDefault();
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                return null;
            }

        }

        public async Task<string> AuthenticateUser(string username, string password)
        {
            var tableClient = await GetTableClient();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
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
                    return rowKey;
                }
                else continue;

            }
            return rowKey;
        }
        public async Task<string> GetRowKeyByUsername(string username)
        {
            var tableClient = await GetTableClient();
            var queryResult = tableClient.QueryAsync<UserCredentials>(filter: e => e.Username == username);

            await foreach (var entity in queryResult)
            {
                return entity.RowKey;
            }

            return null; // or throw an exception, depending on your requirements
        }

        public async Task<ICollection<UserCredentials>> GetAllUsers()
        {
            ICollection<UserCredentials> GetAllData = new List<UserCredentials>();
            var tableClient = await GetTableClient();

            var celebs = tableClient.QueryAsync<UserCredentials>(filter: "");


            await foreach (var fileDatas in celebs)
            {
                GetAllData.Add(fileDatas);
            }
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


       
        




      

   