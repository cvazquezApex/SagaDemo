$orderRequest = @{
    customerId = "550e8400-e29b-41d4-a716-446655440000"
    items = @(
        @{
            productId = "11111111-1111-1111-1111-111111111111"
            productName = "Laptop"
            quantity = 1
            price = 999.99
        }
    )
} | ConvertTo-Json -Depth 3

try {
    $response = Invoke-RestMethod -Uri "http://localhost:5001/api/orders" -Method Post -Body $orderRequest -ContentType "application/json"
    Write-Host "Order created successfully: $($response.id)"
    
    # Wait a bit for saga processing
    Start-Sleep -Seconds 5
    
    # Check order status
    $orderStatus = Invoke-RestMethod -Uri "http://localhost:5001/api/orders/$($response.id)" -Method Get
    Write-Host "Order Status: $($orderStatus.status)"
} catch {
    Write-Host "Error: $($_.Exception.Message)"
}