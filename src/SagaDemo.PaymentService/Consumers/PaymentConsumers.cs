using MassTransit;
using MongoDB.Driver;
using SagaDemo.Contracts;
using SagaDemo.PaymentService.Models;
using SagaDemo.PaymentService.Services;

namespace SagaDemo.PaymentService.Consumers;

public class ProcessPaymentConsumer : IConsumer<ProcessPayment>
{
    private readonly IMongoCollection<Payment> _payments;
    private readonly IPaymentProcessor _paymentProcessor;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<ProcessPaymentConsumer> _logger;

    public ProcessPaymentConsumer(
        IMongoDatabase database,
        IPaymentProcessor paymentProcessor,
        IPublishEndpoint publishEndpoint,
        ILogger<ProcessPaymentConsumer> logger)
    {
        _payments = database.GetCollection<Payment>("payments");
        _paymentProcessor = paymentProcessor;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ProcessPayment> context)
    {
        var message = context.Message;
        _logger.LogInformation("Processing payment for order {OrderId}", message.OrderId);

        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            OrderId = message.OrderId,
            CustomerId = message.CustomerId,
            Amount = message.Amount,
            Status = PaymentStatus.Pending
        };

        await _payments.InsertOneAsync(payment);

        var success = await _paymentProcessor.ProcessPaymentAsync(message.CustomerId, message.Amount);

        if (success)
        {
            payment.Status = PaymentStatus.Processed;
            await _payments.ReplaceOneAsync(p => p.Id == payment.Id, payment);

            await _publishEndpoint.Publish(new PaymentProcessed(
                message.OrderId,
                payment.Id,
                message.Amount));
        }
        else
        {
            payment.Status = PaymentStatus.Failed;
            await _payments.ReplaceOneAsync(p => p.Id == payment.Id, payment);

            await _publishEndpoint.Publish(new PaymentFailed(
                message.OrderId,
                "Payment processing failed"));
        }
    }
}

public class RefundPaymentConsumer : IConsumer<RefundPayment>
{
    private readonly IMongoCollection<Payment> _payments;
    private readonly IPaymentProcessor _paymentProcessor;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<RefundPaymentConsumer> _logger;

    public RefundPaymentConsumer(
        IMongoDatabase database,
        IPaymentProcessor paymentProcessor,
        IPublishEndpoint publishEndpoint,
        ILogger<RefundPaymentConsumer> logger)
    {
        _payments = database.GetCollection<Payment>("payments");
        _paymentProcessor = paymentProcessor;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<RefundPayment> context)
    {
        var message = context.Message;
        _logger.LogInformation("Refunding payment {PaymentId} for order {OrderId}", message.PaymentId, message.OrderId);

        var payment = await _payments.Find(p => p.Id == message.PaymentId).FirstOrDefaultAsync();
        if (payment == null)
        {
            _logger.LogWarning("Payment {PaymentId} not found", message.PaymentId);
            await _publishEndpoint.Publish(new RefundFailed(
                message.OrderId,
                message.PaymentId,
                "Payment not found"));
            return;
        }

        var success = await _paymentProcessor.RefundPaymentAsync(message.PaymentId, message.Amount);

        if (success)
        {
            payment.Status = PaymentStatus.Refunded;
            payment.UpdatedAt = DateTime.UtcNow;
            await _payments.ReplaceOneAsync(p => p.Id == payment.Id, payment);

            await _publishEndpoint.Publish(new PaymentRefunded(
                message.OrderId,
                message.PaymentId,
                message.Amount));
        }
        else
        {
            payment.Status = PaymentStatus.RefundFailed;
            payment.UpdatedAt = DateTime.UtcNow;
            await _payments.ReplaceOneAsync(p => p.Id == payment.Id, payment);

            await _publishEndpoint.Publish(new RefundFailed(
                message.OrderId,
                message.PaymentId,
                "Refund processing failed"));
        }
    }
}