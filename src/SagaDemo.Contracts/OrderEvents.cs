namespace SagaDemo.Contracts;

public record OrderCreated(Guid OrderId, Guid CustomerId, decimal Amount, List<OrderItem> Items);

public record OrderApproved(Guid OrderId);

public record OrderRejected(Guid OrderId, string Reason);

public record OrderCompleted(Guid OrderId);

public record OrderCancelled(Guid OrderId, string Reason);

public record OrderItem(Guid ProductId, string ProductName, int Quantity, decimal Price);