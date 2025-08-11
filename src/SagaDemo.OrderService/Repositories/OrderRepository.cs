using MongoDB.Driver;
using SagaDemo.OrderService.Models;

namespace SagaDemo.OrderService.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly IMongoCollection<Order> _orders;

    public OrderRepository(IMongoDatabase database)
    {
        _orders = database.GetCollection<Order>("orders");
    }

    public async Task<Order> CreateAsync(Order order)
    {
        await _orders.InsertOneAsync(order);
        return order;
    }

    public async Task<Order?> GetByIdAsync(Guid id)
    {
        return await _orders.Find(x => x.Id == id).FirstOrDefaultAsync();
    }

    public async Task<List<Order>> GetAllAsync()
    {
        return await _orders.Find(_ => true).ToListAsync();
    }

    public async Task UpdateAsync(Order order)
    {
        order.UpdatedAt = DateTime.UtcNow;
        await _orders.ReplaceOneAsync(x => x.Id == order.Id, order);
    }

    public async Task DeleteAsync(Guid id)
    {
        await _orders.DeleteOneAsync(x => x.Id == id);
    }
}