using MassTransit;
using SagaDemo.Contracts;
using SagaDemo.OrderService.Models;
using SagaDemo.OrderService.Repositories;

namespace SagaDemo.OrderService.Consumers;

public class OrderApprovedConsumer : IConsumer<OrderApproved>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<OrderApprovedConsumer> _logger;

    public OrderApprovedConsumer(IOrderRepository orderRepository, ILogger<OrderApprovedConsumer> logger)
    {
        _orderRepository = orderRepository;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderApproved> context)
    {
        _logger.LogInformation("Order {OrderId} approved", context.Message.OrderId);

        var order = await _orderRepository.GetByIdAsync(context.Message.OrderId);
        if (order != null)
        {
            order.Status = OrderStatus.Approved;
            await _orderRepository.UpdateAsync(order);
        }
    }
}

public class OrderRejectedConsumer : IConsumer<OrderRejected>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<OrderRejectedConsumer> _logger;

    public OrderRejectedConsumer(IOrderRepository orderRepository, ILogger<OrderRejectedConsumer> logger)
    {
        _orderRepository = orderRepository;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderRejected> context)
    {
        _logger.LogInformation("Order {OrderId} rejected: {Reason}", context.Message.OrderId, context.Message.Reason);

        var order = await _orderRepository.GetByIdAsync(context.Message.OrderId);
        if (order != null)
        {
            order.Status = OrderStatus.Rejected;
            await _orderRepository.UpdateAsync(order);
        }
    }
}

public class OrderCompletedConsumer : IConsumer<OrderCompleted>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<OrderCompletedConsumer> _logger;

    public OrderCompletedConsumer(IOrderRepository orderRepository, ILogger<OrderCompletedConsumer> logger)
    {
        _orderRepository = orderRepository;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderCompleted> context)
    {
        _logger.LogInformation("Order {OrderId} completed", context.Message.OrderId);

        var order = await _orderRepository.GetByIdAsync(context.Message.OrderId);
        if (order != null)
        {
            order.Status = OrderStatus.Completed;
            await _orderRepository.UpdateAsync(order);
        }
    }
}

public class OrderCancelledConsumer : IConsumer<OrderCancelled>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<OrderCancelledConsumer> _logger;

    public OrderCancelledConsumer(IOrderRepository orderRepository, ILogger<OrderCancelledConsumer> logger)
    {
        _orderRepository = orderRepository;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderCancelled> context)
    {
        _logger.LogInformation("Order {OrderId} cancelled: {Reason}", context.Message.OrderId, context.Message.Reason);

        var order = await _orderRepository.GetByIdAsync(context.Message.OrderId);
        if (order != null)
        {
            order.Status = OrderStatus.Cancelled;
            await _orderRepository.UpdateAsync(order);
        }
    }
}