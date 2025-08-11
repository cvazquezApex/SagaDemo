using MassTransit;
using MongoDB.Driver;
using SagaDemo.Contracts;
using SagaDemo.SagaOrchestrator.Sagas;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var mongoConnectionString = builder.Configuration.GetConnectionString("MongoDB")
    ?? "mongodb://localhost:27017";

builder.Services.AddMassTransit(x =>
{
    x.AddSagaStateMachine<OrderSaga, OrderSagaState>()
        .MongoDbRepository(r =>
        {
            r.Connection = mongoConnectionString;
            r.DatabaseName = "sagaorchestrator";
        });

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