using SagaDemo.OrderService.Models;

namespace SagaDemo.OrderService.Repositories;

public interface IOrderRepository
{
    Task<Order> CreateAsync(Order order);
    Task<Order?> GetByIdAsync(Guid id);
    Task<List<Order>> GetAllAsync();
    Task UpdateAsync(Order order);
    Task DeleteAsync(Guid id);
}