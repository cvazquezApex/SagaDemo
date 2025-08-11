using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SagaDemo.InventoryService.Models;

public class Product
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public int AvailableQuantity { get; set; }

    public int ReservedQuantity { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class Reservation
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; }

    [BsonRepresentation(BsonType.String)]
    public Guid OrderId { get; set; }

    [BsonRepresentation(BsonType.String)]
    public Guid ProductId { get; set; }

    public int Quantity { get; set; }

    public ReservationStatus Status { get; set; } = ReservationStatus.Reserved;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public enum ReservationStatus
{
    Reserved,
    Released,
    Failed
}