using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SagaDemo.OrderService.Models;

public class Order
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; }

    [BsonRepresentation(BsonType.String)]
    public Guid CustomerId { get; set; }

    public decimal Amount { get; set; }

    public List<OrderItem> Items { get; set; } = new();

    public OrderStatus Status { get; set; } = OrderStatus.Created;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class OrderItem
{
    [BsonRepresentation(BsonType.String)]
    public Guid ProductId { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public decimal Price { get; set; }
}

public enum OrderStatus
{
    Created,
    PaymentProcessing,
    InventoryReserving,
    Approved,
    Completed,
    Cancelled,
    Rejected
}