
using Azure.Storage.Blobs;
using AzureCrudViaMinimalApi;
using AzureCrudViaMinimalApi.Hub;
using AzureCrudViaMinimalApi.Repository;
using AzureCrudViaMinimalApi.Repository.IRepository;
using crudWithAzure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.OData.Edm;
using Microsoft.WindowsAzure.Storage.Queue.Protocol;

using System.Data.SqlTypes;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

//builder.Services.AddSignalR();
builder.Services.AddScoped<ITableStorageRepository, TableStorageRepository>();
builder.Services.AddScoped<IAuthenticateRepository, AuthenticateRepository>();
builder.Services.AddSignalR(options => { options.KeepAliveInterval = TimeSpan.FromSeconds(5); });

//builder.Services.AddSignalR(options => { options.KeepAliveInterval = TimeSpan.FromSeconds(5); });


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "MyPolicy",
      builder =>
      {
          builder.WithOrigins("http://localhost:3000")
                                    .AllowAnyOrigin()
                                    .AllowAnyHeader()
                                    .AllowAnyMethod();
      });

});

var app = builder.Build();
app.UseRouting();
app.UseAuthorization();



// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
// Map a GET request to retrieve a single entity based on file name and ID
app.MapGet("/getentityasync", async (string fileName, string id, ITableStorageRepository service) =>
{
    return Results.Ok(await service.GetEntityAsync(fileName, id));
})
    .WithName("GetData");

// Map a GET request to retrieve all entities

app.MapGet("/GetAllEntityForSpecificUser/{id}", async (int id ,ITableStorageRepository service) =>
{
    var data = await service.GetAllEntityForSpecificUser(id);

    return Results.Ok(await service.GetAllEntityForSpecificUser(id));

});

app.MapGet("GetAll", async (ITableStorageRepository service) =>
{
    var data = await service.GetAllEntityAsync();
    return Results.Ok(data);
});



//app.MapPut("/UpdateData", async (FileDataInfo data, ITableStorageRepository tableStorageRepository) =>
//{
//    var getMessage = await tableStorageRepository.UpsertEntityAsync(data);
//    if (getMessage!=null) return Results.Ok(new { Staus = 1, Message = "Updated Successfully" });
//    return Results.BadRequest(new { Staus = 0, Message = "Somehting went wrong" });

//});
// Map a POST request to upsert a new entity
app.MapPost("/upsertentityasync", async (FileDataInfo entity, ITableStorageRepository service) =>
{
    entity.PartitionKey = entity.Name;
    string Id = Guid.NewGuid().ToString();
    entity.Id = Id;
    entity.RowKey = Id;

    
    var createdEntity = await service.UpsertEntityAsync(entity);
    return createdEntity;
})
    .WithName("PostData");


//// Map a PUT request to update an existing entity
app.MapPut("/updateentityasync", async (FileDataInfo entity, ITableStorageRepository service) =>
{
    entity.PartitionKey = entity.PartitionKey;
    entity.RowKey = entity.Id;

    entity.UserId = entity.UserId;

    await service.UpsertEntityAsync(entity);
    return Results.Ok();
})
    .WithName("UpdateData");


// Map a DELETE request to delete an entity based on name, extension, partition key, and row key
app.MapDelete("/Delete", async (string name,string extension, string partitionKey, string rowKey, ITableStorageRepository tableStorageRepository) =>
{
    var getMessage = await tableStorageRepository.DeleteEntityAsync(name, extension, rowKey, partitionKey);
    if (getMessage) return Results.Ok(new { Staus = 1, Message = "Deleted Successfully" });
    return Results.BadRequest(new { Staus = 0, Message = "Somehting went wrong" });

});

//Authenticate user
//app.MapPost("/AuthenticateUser", async (UserCredentials userCredentials, IAuthenticateRepository service) =>
//{
//    var rowKey=await service.AuthenticateUser(userCredentials.Username, userCredentials.Password);

//    if (rowKey != null) return Results.Ok(new {username=userCredentials.Username, status = 1,message="User Logged In",rowKey= rowKey }) ;
//    return Results.BadRequest(new { Staus = 0, Message = "Wrong Username/Password" });

//});

// login user here 
app.MapGet("/login/{userName}/{password}", (string userName, string password, IAuthenticateRepository iautenticateRepository) =>
{
    var data = iautenticateRepository.authenticateUser(userName, password);
    if (data != null) return Results.Ok(new { Status = 1, Message = "login successfully", data = new { userName = data.Username, Id = data.RowKey } });
    return Results.BadRequest(new { Status = 0, Message = "login unsuccessfully" });
});


//GetAll Users from login Table
//app.MapGet("/GetAllUsersFromLogin", async (IAuthenticateRepository service) =>
//{
//    var data = await service.GetAllUsers();

//    return Results.Ok(await service.GetAllUsers());
//});

//Here i will get the userDetails who logged in by its id
//app.MapGet("/GetAllEntityForSpecificUser", async (int id, ITableStorageRepository service) =>
//{
//    var getAllUserInfo=await service.GetAllEntityForSpecificUser(id);
//    if (getAllUserInfo != null) return Results.Ok(getAllUserInfo);
//    else return Results.BadRequest();
//});

app.UseHttpsRedirection();


//Find record in Login Table
app.MapGet("/getentityFromLogin", async (string username,string password, IAuthenticateRepository service) =>
{
    var userCredentials = await service.GetLoginEntityAsync(username, password);
    return Results.Ok();
});
    

//GetAll Users from login Table
app.MapGet("/GetAllUsersFromLogin", async (IAuthenticateRepository service) =>
{
    var data = await service.GetAllUsers();

    return Results.Ok(await service.GetAllUsers());
});

//Here i will get the userDetails who logged in by its id
app.MapGet("/GetAllEntityForSpecificUser", async (int id, ITableStorageRepository service) =>
{
    var getAllUserInfo=await service.GetAllEntityForSpecificUser(id);
    if (getAllUserInfo != null) return Results.Ok(getAllUserInfo);
    else return Results.BadRequest();
});
//app.UseHttpsRedirection();

app.MapHub<MessageHub>("/getDataSignalR");
//app.MapHub<MessageHub>("/updateentityasync");

app.UseCors("MyPolicy");




app.Run();

public partial class Program { }

