using AzureCrudViaMinimalApi;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.OData.Edm;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Json;
using System.Text;

namespace AzureTest
{
    public class AzureTest
    {
        //This method is  to test HTTPGET endpoints and will get All data From mytable

        [Fact]
        public async void GetAll_StatusOk()
        {
            // Arrange
            await using var application = new WebApplicationFactory<Program>();
            using var client = application.CreateClient();

            // Act
            var response = await client.GetAsync("/GetAll");
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }


        //This method is  to test HTTPGET endpoints and will get data accoring to userId
        [Fact]
        public async void GetSpecificUserData_StatusOk()
        {
            // Arrange
            await using var application=new WebApplicationFactory<Program>();
            using var client = application.CreateClient();
            var id = 1;

            // Act
            var response = await client.GetAsync($"/GetAllEntityForSpecificUser/{id}");
            var data = await response.Content.ReadAsStringAsync();

            //Asert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }


        //This method is  to test HTTPGET endpoints and will know that user credentials are correct or not
        [Fact]
        public async void GetLogin_statusOk()
        {
            // Arrange
            await using var application = new WebApplicationFactory<Program>();
            using var client = application.CreateClient();
            string userName = "admin";
            string password = "Admin@123";

            // Act
            var response = await client.GetAsync($"/login/{userName}/{password}");
            var data = await response.Content.ReadAsStringAsync();

            //Asert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        //This method is  to test HTTPGET endpoints and will know that the file exist or not in the azure table
        [Fact]
        public async void FileExistOrNot_statusOk()
        {
            // Arrange
            await using var application = new WebApplicationFactory<Program>();
            using var client = application.CreateClient();
            string id = "d49bf72c-6748-4e75-bcfe-682a247b05bd";
            string filname = "pic";
            // Act
            var response = await client.GetAsync($"/getentityasync/{filname}/{id}");
            var data = await response.Content.ReadAsStringAsync();

            //Asert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
        

        //This method is  to test HTTPGET endpoints and will know that user credentials are correct or not
        [Fact]
        public async void GetFileFromTable_statusOk()
        {
            // Arrange
            await using var application = new WebApplicationFactory<Program>();
            using var client = application.CreateClient();

            // Act
            var response = await client.GetAsync($"/GetAllUsersFromLogin");
            var data = await response.Content.ReadAsStringAsync();

            //Asert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
        //This method is  to test HTTPGET endpoints We will create a new record in azure table
        [Fact]
        public async void CreateFile_statusOk()
        {
            await using var application = new WebApplicationFactory<Program>();
            using var client = application.CreateClient();
            var entity = new
            {
                id = "1",
                Name = "txt",
                Extension = "txt",
                Date = DateTime.UtcNow.ToString("o"),
                userId = 1
            };
            var content = new StringContent(JsonConvert.SerializeObject(entity), Encoding.UTF8, "application/json");

            var response = await client.PostAsync("/upsertentityasync", content);
            var data = response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        //This method is  to test HTTPDelete endpoint and from here, will delete record from table and container
        [Fact]
        public async void DeleteFile_statusOk()
        {
            await using var application = new WebApplicationFactory<Program>();
            using var client = application.CreateClient();
            var name = "1";
            var extension = "jpg";
            var partitionKey = "1";
            var rowKey = "ecfcde8a-af75-40c4-8a31-e4d6a66b243c\t\t\r\n";
            var response = await client.DeleteAsync($"/Delete/{name}/{extension}/{partitionKey}/{rowKey}");
            var data = response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        //This method is  to test HTTPPUT endpoint and from here we will update record from the table
        [Fact]
        public async void UpdateFile_statusOk()
        {
            await using var application = new WebApplicationFactory<Program>();
            using var client = application.CreateClient();
            
            var response = await client.PutAsJsonAsync("/updateentityasync", new FileDataInfo()
            {
                Id = "11ed5907-f87b-4a2a-9c30-8639b68e929a",
                Name = "txt",
                Extension = "txt",
                Date = DateTime.Today.ToUniversalTime(),
                UserId = 1,
                PartitionKey = "txt",
                RowKey = "11ed5907-f87b-4a2a-9c30-8639b68e929a"
            });
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

    }
}