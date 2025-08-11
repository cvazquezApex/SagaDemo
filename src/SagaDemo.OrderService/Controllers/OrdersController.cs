using MassTransit;
using Microsoft.AspNetCore.Mvc;
using SagaDemo.Contracts;
using SagaDemo.OrderService.Models;
using SagaDemo.OrderService.Repositories;

namespace SagaDemo.OrderService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderRepository _orderRepository;
    private readonly IPublishEndpoint _publishEndpoint;

    public OrdersController(IOrderRepository orderRepository, IPublishEndpoint publishEndpoint)
    {
        _orderRepository = orderRepository;
        _publishEndpoint = publishEndpoint;
    }

    [HttpPost]
    public async Task<ActionResult<Order>> CreateOrder(CreateOrderRequest request)
    {
        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = request.CustomerId,
            Amount = request.Items.Sum(i => i.Price * i.Quantity),
            Items = request.Items.Select(i => new Models.OrderItem
            {
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                Quantity = i.Quantity,
                Price = i.Price
            }).ToList()
        };

        await _orderRepository.CreateAsync(order);

        var orderCreated = new OrderCreated(
            order.Id,
            order.CustomerId,
            order.Amount,
            order.Items.Select(i => new Contracts.OrderItem(i.ProductId, i.ProductName, i.Quantity, i.Price)).ToList()
        );

        await _publishEndpoint.Publish(orderCreated);

        return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Order>> GetOrder(Guid id)
    {
        var order = await _orderRepository.GetByIdAsync(id);
        if (order == null)
            return NotFound();

        return order;
    }

    [HttpGet]
    public async Task<ActionResult<List<Order>>> GetOrders()
    {
        var orders = await _orderRepository.GetAllAsync();
        return orders;
    }
}

public record CreateOrderRequest(Guid CustomerId, List<CreateOrderItemRequest> Items);
public record CreateOrderItemRequest(Guid ProductId, string ProductName, int Quantity, decimal Price);