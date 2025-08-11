using MassTransit;
using Microsoft.Extensions.Logging;
using SagaDemo.Contracts;

namespace SagaDemo.SagaOrchestrator.Sagas;

public class OrderSaga : MassTransitStateMachine<OrderSagaState>
{
    private readonly ILogger<OrderSaga> _logger;

    public State OrderCreated { get; private set; } = null!;
    public State PaymentProcessing { get; private set; } = null!;
    public State InventoryReserving { get; private set; } = null!;
    public State OrderApproved { get; private set; } = null!;
    public State OrderCompleted { get; private set; } = null!;
    public State PaymentFailed { get; private set; } = null!;
    public State InventoryFailed { get; private set; } = null!;
    public State OrderCancelled { get; private set; } = null!;

    public Event<Contracts.OrderCreated> OrderCreatedEvent { get; private set; } = null!;
    public Event<PaymentProcessed> PaymentProcessedEvent { get; private set; } = null!;
    public Event<PaymentFailed> PaymentFailedEvent { get; private set; } = null!;
    public Event<InventoryReserved> InventoryReservedEvent { get; private set; } = null!;
    public Event<InventoryReservationFailed> InventoryReservationFailedEvent { get; private set; } = null!;
    public Event<PaymentRefunded> PaymentRefundedEvent { get; private set; } = null!;
    public Event<InventoryReleased> InventoryReleasedEvent { get; private set; } = null!;

    public OrderSaga(ILogger<OrderSaga> logger)
    {
        _logger = logger;
        InstanceState(x => x.CurrentState);

        Event(() => OrderCreatedEvent, x => x.CorrelateById(m => m.Message.OrderId));
        Event(() => PaymentProcessedEvent, x => x.CorrelateBy<Guid>(s => s.OrderId, m => m.Message.OrderId));
        Event(() => PaymentFailedEvent, x => x.CorrelateBy<Guid>(s => s.OrderId, m => m.Message.OrderId));
        Event(() => InventoryReservedEvent, x => x.CorrelateBy<Guid>(s => s.OrderId, m => m.Message.OrderId));
        Event(() => InventoryReservationFailedEvent, x => x.CorrelateBy<Guid>(s => s.OrderId, m => m.Message.OrderId));
        Event(() => PaymentRefundedEvent, x => x.CorrelateBy<Guid>(s => s.OrderId, m => m.Message.OrderId));
        Event(() => InventoryReleasedEvent, x => x.CorrelateBy<Guid>(s => s.OrderId, m => m.Message.OrderId));

        Initially(
            When(OrderCreatedEvent)
                .Then(context =>
                {
                    var message = context.Message;
                    _logger.LogInformation("🚀 SAGA STARTED: Order {OrderId} created by customer {CustomerId} for amount ${Amount:F2}", 
                        message.OrderId, message.CustomerId, message.Amount);
                    
                    context.Saga.CorrelationId = message.OrderId;
                    context.Saga.OrderId = message.OrderId;
                    context.Saga.CustomerId = message.CustomerId;
                    context.Saga.Amount = message.Amount;
                    context.Saga.Items = message.Items;
                    context.Saga.CreatedAt = DateTime.UtcNow;
                    context.Saga.UpdatedAt = DateTime.UtcNow;
                    
                    _logger.LogInformation("📦 SAGA ORDER: {ItemCount} items - {Items}", 
                        message.Items.Count, 
                        string.Join(", ", message.Items.Select(i => $"{i.ProductName} x{i.Quantity}")));
                })
                .Publish(context => 
                {
                    _logger.LogInformation("💳 SAGA PAYMENT: Requesting payment processing for order {OrderId} amount ${Amount:F2}", 
                        context.Saga.OrderId, context.Saga.Amount);
                    return new ProcessPayment(
                        context.Saga.OrderId,
                        context.Saga.CustomerId,
                        context.Saga.Amount);
                })
                .TransitionTo(PaymentProcessing)
                .Then(context => 
                {
                    _logger.LogInformation("🔄 SAGA STATE: Order {OrderId} transitioned to PaymentProcessing", 
                        context.Saga.OrderId);
                }));

        During(PaymentProcessing,
            When(PaymentProcessedEvent)
                .Then(context =>
                {
                    _logger.LogInformation("✅ SAGA PAYMENT SUCCESS: Order {OrderId} payment processed with PaymentId {PaymentId}", 
                        context.Saga.OrderId, context.Message.PaymentId);
                    
                    context.Saga.PaymentId = context.Message.PaymentId;
                    context.Saga.UpdatedAt = DateTime.UtcNow;
                })
                .Publish(context => 
                {
                    _logger.LogInformation("📦 SAGA INVENTORY: Requesting inventory reservation for order {OrderId} - {ItemCount} items", 
                        context.Saga.OrderId, context.Saga.Items.Count);
                    return new ReserveInventory(
                        context.Saga.OrderId,
                        context.Saga.Items.Select(i => new InventoryItem(i.ProductId, i.Quantity)).ToList());
                })
                .TransitionTo(InventoryReserving)
                .Then(context => 
                {
                    _logger.LogInformation("🔄 SAGA STATE: Order {OrderId} transitioned to InventoryReserving", 
                        context.Saga.OrderId);
                }),

            When(PaymentFailedEvent)
                .Then(context => 
                {
                    _logger.LogWarning("❌ SAGA PAYMENT FAILED: Order {OrderId} payment failed - {Reason}", 
                        context.Saga.OrderId, context.Message.Reason);
                    context.Saga.UpdatedAt = DateTime.UtcNow;
                })
                .Publish(context => 
                {
                    _logger.LogInformation("🚫 SAGA REJECT: Publishing order rejection for {OrderId}", 
                        context.Saga.OrderId);
                    return new Contracts.OrderRejected(
                        context.Saga.OrderId,
                        "Payment failed");
                })
                .TransitionTo(PaymentFailed)
                .Then(context => 
                {
                    _logger.LogInformation("🏁 SAGA COMPLETE: Order {OrderId} saga finalized due to payment failure", 
                        context.Saga.OrderId);
                })
                .Finalize());

        During(InventoryReserving,
            When(InventoryReservedEvent)
                .Then(context => 
                {
                    _logger.LogInformation("✅ SAGA INVENTORY SUCCESS: Order {OrderId} inventory successfully reserved", 
                        context.Saga.OrderId);
                    context.Saga.UpdatedAt = DateTime.UtcNow;
                })
                .Publish(context => 
                {
                    _logger.LogInformation("✅ SAGA APPROVED: Publishing order approval for {OrderId}", 
                        context.Saga.OrderId);
                    return new Contracts.OrderApproved(context.Saga.OrderId);
                })
                .Publish(context => 
                {
                    _logger.LogInformation("🎉 SAGA SUCCESS: Publishing order completion for {OrderId}", 
                        context.Saga.OrderId);
                    return new Contracts.OrderCompleted(context.Saga.OrderId);
                })
                .TransitionTo(OrderCompleted)
                .Then(context => 
                {
                    _logger.LogInformation("🏁 SAGA COMPLETE: Order {OrderId} saga successfully completed", 
                        context.Saga.OrderId);
                })
                .Finalize(),

            When(InventoryReservationFailedEvent)
                .Then(context => 
                {
                    _logger.LogWarning("❌ SAGA INVENTORY FAILED: Order {OrderId} inventory reservation failed - {Reason}", 
                        context.Saga.OrderId, context.Message.Reason);
                    context.Saga.UpdatedAt = DateTime.UtcNow;
                })
                .IfElse(context => context.Saga.PaymentId.HasValue,
                    binder => binder
                        .Then(context => 
                        {
                            _logger.LogInformation("💰 SAGA REFUND: Requesting payment refund for order {OrderId} PaymentId {PaymentId}", 
                                context.Saga.OrderId, context.Saga.PaymentId);
                        })
                        .Publish(context => new RefundPayment(
                            context.Saga.OrderId,
                            context.Saga.PaymentId!.Value,
                            context.Saga.Amount))
                        .TransitionTo(InventoryFailed)
                        .Then(context => 
                        {
                            _logger.LogInformation("🔄 SAGA STATE: Order {OrderId} transitioned to InventoryFailed, waiting for refund", 
                                context.Saga.OrderId);
                        }),
                    binder => binder
                        .Then(context => 
                        {
                            _logger.LogInformation("🚫 SAGA REJECT: No payment to refund, rejecting order {OrderId}", 
                                context.Saga.OrderId);
                        })
                        .Publish(context => new Contracts.OrderRejected(
                            context.Saga.OrderId,
                            "Inventory reservation failed"))
                        .Then(context => 
                        {
                            _logger.LogInformation("🏁 SAGA COMPLETE: Order {OrderId} saga finalized due to inventory failure", 
                                context.Saga.OrderId);
                        })
                        .Finalize()));

        During(InventoryFailed,
            When(PaymentRefundedEvent)
                .Then(context => 
                {
                    _logger.LogInformation("💰 SAGA REFUND SUCCESS: Order {OrderId} payment refunded successfully", 
                        context.Saga.OrderId);
                    context.Saga.UpdatedAt = DateTime.UtcNow;
                })
                .Publish(context => 
                {
                    _logger.LogInformation("🚫 SAGA CANCELLED: Publishing order cancellation for {OrderId}", 
                        context.Saga.OrderId);
                    return new Contracts.OrderCancelled(
                        context.Saga.OrderId,
                        "Inventory reservation failed, payment refunded");
                })
                .TransitionTo(OrderCancelled)
                .Then(context => 
                {
                    _logger.LogInformation("🏁 SAGA COMPLETE: Order {OrderId} saga finalized with cancellation and refund", 
                        context.Saga.OrderId);
                })
                .Finalize());

        SetCompletedWhenFinalized();
    }
}