namespace SagaDemo.Contracts;

public record ReserveInventory(Guid OrderId, List<InventoryItem> Items);

public record InventoryReserved(Guid OrderId, List<InventoryItem> Items);

public record InventoryReservationFailed(Guid OrderId, string Reason);

public record ReleaseInventory(Guid OrderId, List<InventoryItem> Items);

public record InventoryReleased(Guid OrderId, List<InventoryItem> Items);

public record InventoryItem(Guid ProductId, int Quantity);