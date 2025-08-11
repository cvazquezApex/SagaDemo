using MongoDB.Driver;
using SagaDemo.InventoryService.Models;

namespace SagaDemo.InventoryService.Services;

public interface IInventoryManager
{
    Task<bool> ReserveInventoryAsync(Guid orderId, List<(Guid ProductId, int Quantity)> items);
    Task<bool> ReleaseInventoryAsync(Guid orderId);
    Task InitializeProductsAsync();
}

public class InventoryManager : IInventoryManager
{
    private readonly IMongoCollection<Product> _products;
    private readonly IMongoCollection<Reservation> _reservations;
    private readonly ILogger<InventoryManager> _logger;

    public InventoryManager(IMongoDatabase database, ILogger<InventoryManager> logger)
    {
        _products = database.GetCollection<Product>("products");
        _reservations = database.GetCollection<Reservation>("reservations");
        _logger = logger;
    }

    public async Task<bool> ReserveInventoryAsync(Guid orderId, List<(Guid ProductId, int Quantity)> items)
    {
        try
        {
            // First, check all items for availability
            foreach (var (productId, quantity) in items)
            {
                var product = await _products.Find(p => p.Id == productId).FirstOrDefaultAsync();
                if (product == null)
                {
                    _logger.LogWarning("Product {ProductId} not found", productId);
                    return false;
                }

                if (product.AvailableQuantity < quantity)
                {
                    _logger.LogWarning("Insufficient inventory for product {ProductId}. Available: {Available}, Requested: {Requested}",
                        productId, product.AvailableQuantity, quantity);
                    return false;
                }
            }

            // Reserve each item
            foreach (var (productId, quantity) in items)
            {
                var product = await _products.Find(p => p.Id == productId).FirstOrDefaultAsync();
                
                product.AvailableQuantity -= quantity;
                product.ReservedQuantity += quantity;
                product.UpdatedAt = DateTime.UtcNow;

                await _products.ReplaceOneAsync(p => p.Id == productId, product);

                var reservation = new Reservation
                {
                    Id = Guid.NewGuid(),
                    OrderId = orderId,
                    ProductId = productId,
                    Quantity = quantity,
                    Status = ReservationStatus.Reserved,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _reservations.InsertOneAsync(reservation);
            }

            _logger.LogInformation("Successfully reserved inventory for order {OrderId}", orderId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to reserve inventory for order {OrderId}", orderId);
            return false;
        }
    }

    public async Task<bool> ReleaseInventoryAsync(Guid orderId)
    {
        try
        {
            var reservations = await _reservations.Find(r => r.OrderId == orderId && r.Status == ReservationStatus.Reserved)
                .ToListAsync();

            foreach (var reservation in reservations)
            {
                var product = await _products.Find(p => p.Id == reservation.ProductId).FirstOrDefaultAsync();
                if (product != null)
                {
                    product.AvailableQuantity += reservation.Quantity;
                    product.ReservedQuantity -= reservation.Quantity;
                    product.UpdatedAt = DateTime.UtcNow;

                    await _products.ReplaceOneAsync(p => p.Id == reservation.ProductId, product);
                }

                reservation.Status = ReservationStatus.Released;
                reservation.UpdatedAt = DateTime.UtcNow;

                await _reservations.ReplaceOneAsync(r => r.Id == reservation.Id, reservation);
            }

            _logger.LogInformation("Successfully released inventory for order {OrderId}", orderId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to release inventory for order {OrderId}", orderId);
            return false;
        }
    }

    public async Task InitializeProductsAsync()
    {
        var existingCount = await _products.CountDocumentsAsync(FilterDefinition<Product>.Empty);
        if (existingCount > 0)
        {
            _logger.LogInformation("Products already exist in database");
            return;
        }

        var products = new List<Product>
        {
            new() { Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), Name = "Laptop", AvailableQuantity = 10 },
            new() { Id = Guid.Parse("22222222-2222-2222-2222-222222222222"), Name = "Mouse", AvailableQuantity = 50 },
            new() { Id = Guid.Parse("33333333-3333-3333-3333-333333333333"), Name = "Keyboard", AvailableQuantity = 30 },
            new() { Id = Guid.Parse("44444444-4444-4444-4444-444444444444"), Name = "Monitor", AvailableQuantity = 15 }
        };

        await _products.InsertManyAsync(products);
        _logger.LogInformation("Initialized {Count} products", products.Count);
    }
}