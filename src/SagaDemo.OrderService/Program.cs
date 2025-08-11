using MassTransit;
using MongoDB.Driver;
using SagaDemo.OrderService.Consumers;
using SagaDemo.OrderService.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var mongoConnectionString = builder.Configuration.GetConnectionString("MongoDB")
    ?? "mongodb://localhost:27017";
builder.Services.AddSingleton<IMongoClient>(new MongoClient(mongoConnectionString));
builder.Services.AddSingleton<IMongoDatabase>(provider =>
{
    var client = provider.GetService<IMongoClient>()!;
    return client.GetDatabase("orderservice");
});

builder.Services.AddScoped<IOrderRepository, OrderRepository>();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<OrderApprovedConsumer>();
    x.AddConsumer<OrderRejectedConsumer>();
    x.AddConsumer<OrderCompletedConsumer>();
    x.AddConsumer<OrderCancelledConsumer>();

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

app.UseAuthorization();
app.MapControllers();

app.Run();