namespace SagaDemo.PaymentService.Services;

public interface IPaymentProcessor
{
    Task<bool> ProcessPaymentAsync(Guid customerId, decimal amount);
    Task<bool> RefundPaymentAsync(Guid paymentId, decimal amount);
}

public class PaymentProcessor : IPaymentProcessor
{
    private readonly ILogger<PaymentProcessor> _logger;

    public PaymentProcessor(ILogger<PaymentProcessor> logger)
    {
        _logger = logger;
    }

    public async Task<bool> ProcessPaymentAsync(Guid customerId, decimal amount)
    {
        _logger.LogInformation("Processing payment for customer {CustomerId}, amount {Amount}", customerId, amount);
        
        await Task.Delay(1000);
        
        var random = new Random();
        var success = random.NextDouble() > 0.2;
        
        if (success)
        {
            _logger.LogInformation("Payment processed successfully for customer {CustomerId}", customerId);
        }
        else
        {
            _logger.LogWarning("Payment failed for customer {CustomerId}", customerId);
        }

        return success;
    }

    public async Task<bool> RefundPaymentAsync(Guid paymentId, decimal amount)
    {
        _logger.LogInformation("Processing refund for payment {PaymentId}, amount {Amount}", paymentId, amount);
        
        await Task.Delay(500);
        
        var random = new Random();
        var success = random.NextDouble() > 0.1;
        
        if (success)
        {
            _logger.LogInformation("Refund processed successfully for payment {PaymentId}", paymentId);
        }
        else
        {
            _logger.LogWarning("Refund failed for payment {PaymentId}", paymentId);
        }

        return success;
    }
}