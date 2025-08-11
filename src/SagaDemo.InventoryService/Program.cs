using MassTransit;
using MongoDB.Driver;
using SagaDemo.InventoryService.Consumers;
using SagaDemo.InventoryService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var mongoConnectionString = builder.Configuration.GetConnectionString("MongoDB")
    ?? "mongodb://localhost:27017";
builder.Services.AddSingleton<IMongoClient>(new MongoClient(mongoConnectionString));
builder.Services.AddSingleton<IMongoDatabase>(provider =>
{
    var client = provider.GetService<IMongoClient>()!;
    return client.GetDatabase("inventoryservice");
});

builder.Services.AddScoped<IInventoryManager, InventoryManager>();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<ReserveInventoryConsumer>();
    x.AddConsumer<ReleaseInventoryConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        var rabbitMqHost = builder.Configuration["RabbitMQ:Host"] ?? "localhost";
        var rabbitMqUsername = builder.Configuration["RabbitMQ:Username"] ?? "guest";
        var rabbitMqPassword = builder.Configuration["RabbitMQ:Password"] ?? "guest";

        cfg.Host(rabbitMqHost, "/", h =>
        {
            h.Username(rabbitMqUsername);
            h.Password(rabbitMqPassword);
        });

        cfg.ConfigureEndpoints(context);
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

using (var scope = app.Services.CreateScope())
{
    var inventoryManager = scope.ServiceProvider.GetRequiredService<IInventoryManager>();
    await inventoryManager.InitializeProductsAsync();
}

app.UseAuthorization();
app.MapControllers();

app.Run();