namespace SagaDemo.Contracts;

public record ProcessPayment(Guid OrderId, Guid CustomerId, decimal Amount);

public record PaymentProcessed(Guid OrderId, Guid PaymentId, decimal Amount);

public record PaymentFailed(Guid OrderId, string Reason);

public record RefundPayment(Guid OrderId, Guid PaymentId, decimal Amount);

public record PaymentRefunded(Guid OrderId, Guid PaymentId, decimal Amount);

public record RefundFailed(Guid OrderId, Guid PaymentId, string Reason);