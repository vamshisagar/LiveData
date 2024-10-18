using LiveData.Cosmos;
using LiveData.Hubs;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins", builder =>
    {
        builder.WithOrigins("http://localhost:3000")
               .AllowAnyMethod()
               .AllowAnyHeader()
               .AllowCredentials();
    });
});

var cosmosDbSettings = builder.Configuration.GetSection("CosmosDb");
builder.Services.AddSingleton<CosmosClient>(new CosmosClient(
    cosmosDbSettings["EndpointUri"],
    cosmosDbSettings["PrimaryKey"]));
builder.Services.AddSignalR();
builder.Services.AddSingleton<ChangeFeedProcessorService>();


var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseCors("AllowAllOrigins");
app.UseRouting();
app.MapHub<DataHub>("/dataHub");
app.MapControllers();

var changeFeedProcessor = ((IApplicationBuilder)app).ApplicationServices.GetRequiredService<ChangeFeedProcessorService>();
changeFeedProcessor.StartChangeFeedProcessorAsync().GetAwaiter().GetResult();
app.Run();
