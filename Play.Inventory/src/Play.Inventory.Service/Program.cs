using System;
using Play.Common.MongoDb;
using Play.Inventory.Service.Clients;
using Play.Inventory.Service.Entities;
using Polly;
using Polly.Timeout;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMongo().AddMongoRepository<InventoryItem>("inventoryitems");


//variable randon para que los reintentos sean aleatorios, y no al mismo tiempo
Random jitterrer = new Random();

//service providerso we can use it in the function bellow
var serviceProvider = builder.Services.BuildServiceProvider();

builder.Services.AddHttpClient<CatalogClient>(client =>
{
  client.BaseAddress = new Uri("https://localhost:7030");
})// Add a policy to retry 5 times, on failure. EL ORDEN IMPORTANTE DE LAS POLICIES
//el or permite que se ejecuten todas las policies, se pone el error que se quiere controlar con lasegunda y si es este se lo deja  ala otra policy
.AddTransientHttpErrorPolicy(builder => builder.Or<TimeoutRejectedException>().WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt) + TimeSpan.FromMilliseconds(jitterrer.Next(0, 1000)).TotalSeconds)))
// add policy to stop all trafic if the service is really down, after x errors, we will make an error for x seconds until we make on retry
.AddTransientHttpErrorPolicy(builder => builder.Or<TimeoutRejectedException>().CircuitBreakerAsync(3, TimeSpan.FromSeconds(15)))
// Add a policy to consider a failure if we wait more than x seconds in each try
.AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(1));


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
