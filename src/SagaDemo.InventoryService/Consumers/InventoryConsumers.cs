using MassTransit;
using SagaDemo.Contracts;
using SagaDemo.InventoryService.Services;

namespace SagaDemo.InventoryService.Consumers;

public class ReserveInventoryConsumer : IConsumer<ReserveInventory>
{
    private readonly IInventoryManager _inventoryManager;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<ReserveInventoryConsumer> _logger;

    public ReserveInventoryConsumer(
        IInventoryManager inventoryManager,
        IPublishEndpoint publishEndpoint,
        ILogger<ReserveInventoryConsumer> logger)
    {
        _inventoryManager = inventoryManager;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ReserveInventory> context)
    {
        var message = context.Message;
        _logger.LogInformation("Reserving inventory for order {OrderId}", message.OrderId);

        var items = message.Items.Select(i => (i.ProductId, i.Quantity)).ToList();
        var success = await _inventoryManager.ReserveInventoryAsync(message.OrderId, items);

        if (success)
        {
            await _publishEndpoint.Publish(new InventoryReserved(
                message.OrderId,
                message.Items));
        }
        else
        {
            await _publishEndpoint.Publish(new InventoryReservationFailed(
                message.OrderId,
                "Insufficient inventory or product not found"));
        }
    }
}

public class ReleaseInventoryConsumer : IConsumer<ReleaseInventory>
{
    private readonly IInventoryManager _inventoryManager;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<ReleaseInventoryConsumer> _logger;

    public ReleaseInventoryConsumer(
        IInventoryManager inventoryManager,
        IPublishEndpoint publishEndpoint,
        ILogger<ReleaseInventoryConsumer> logger)
    {
        _inventoryManager = inventoryManager;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ReleaseInventory> context)
    {
        var message = context.Message;
        _logger.LogInformation("Releasing inventory for order {OrderId}", message.OrderId);

        var success = await _inventoryManager.ReleaseInventoryAsync(message.OrderId);

        if (success)
        {
            await _publishEndpoint.Publish(new InventoryReleased(
                message.OrderId,
                message.Items));
        }
        else
        {
            _logger.LogWarning("Failed to release inventory for order {OrderId}", message.OrderId);
        }
    }
}