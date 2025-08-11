using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SagaDemo.PaymentService.Models;

public class Payment
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; }

    [BsonRepresentation(BsonType.String)]
    public Guid OrderId { get; set; }

    [BsonRepresentation(BsonType.String)]
    public Guid CustomerId { get; set; }

    public decimal Amount { get; set; }

    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public enum PaymentStatus
{
    Pending,
    Processed,
    Failed,
    Refunded,
    RefundFailed
}