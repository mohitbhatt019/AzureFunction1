
﻿using Microsoft.Azure.Documents;

namespace AzureCrudViaMinimalApi.Repository.IRepository

{

    public interface IAuthenticateRepository
    {

        public UserCredentials? authenticateUser(string userName, string password);
        Task<string> AuthenticateUser(string username, string password);
        Task<ICollection<UserCredentials>> GetAllUsers();
        Task<UserCredentials> GetLoginEntityAsync(string username,string password);

    }
}
