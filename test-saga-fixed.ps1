# PowerShell script to test the Saga Demo application
# Run this after starting the services with: docker compose up --build

Write-Host "Testing Saga Orchestration Pattern Demo" -ForegroundColor Green
Write-Host "=======================================" -ForegroundColor Green

# Wait for services to start
Write-Host "Waiting for services to start..." -ForegroundColor Yellow
Start-Sleep -Seconds 30

# Test 1: Create a successful order
Write-Host "`nTest 1: Creating a successful order with 1 Laptop" -ForegroundColor Cyan

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
    $response1 = Invoke-RestMethod -Uri "http://localhost:5001/api/orders" -Method Post -Body $orderRequest -ContentType "application/json"
    Write-Host "Order created successfully. Order ID: $($response1.id)" -ForegroundColor Green
    $orderId1 = $response1.id
} catch {
    Write-Host "Failed to create order: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 2: Create an order that might fail due to payment (20% failure rate)
Write-Host "`nTest 2: Creating an order with 2 Mice (might fail due to payment)" -ForegroundColor Cyan

$orderRequest2 = @{
    customerId = "550e8400-e29b-41d4-a716-446655440001"
    items = @(
        @{
            productId = "22222222-2222-2222-2222-222222222222"
            productName = "Mouse"
            quantity = 2
            price = 29.99
        }
    )
} | ConvertTo-Json -Depth 3

try {
    $response2 = Invoke-RestMethod -Uri "http://localhost:5001/api/orders" -Method Post -Body $orderRequest2 -ContentType "application/json"
    Write-Host "Order created successfully. Order ID: $($response2.id)" -ForegroundColor Green
    $orderId2 = $response2.id
} catch {
    Write-Host "Failed to create order: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 3: Create an order that will fail due to insufficient inventory
Write-Host "`nTest 3: Creating an order with 100 Laptops (will fail due to insufficient inventory)" -ForegroundColor Cyan

$orderRequest3 = @{
    customerId = "550e8400-e29b-41d4-a716-446655440002"
    items = @(
        @{
            productId = "11111111-1111-1111-1111-111111111111"
            productName = "Laptop"
            quantity = 100
            price = 999.99
        }
    )
} | ConvertTo-Json -Depth 3

try {
    $response3 = Invoke-RestMethod -Uri "http://localhost:5001/api/orders" -Method Post -Body $orderRequest3 -ContentType "application/json"
    Write-Host "Order created successfully. Order ID: $($response3.id)" -ForegroundColor Green
    $orderId3 = $response3.id
} catch {
    Write-Host "Failed to create order: $($_.Exception.Message)" -ForegroundColor Red
}

# Wait for saga processing
Write-Host "`nWaiting for saga processing to complete..." -ForegroundColor Yellow
Start-Sleep -Seconds 10

# Check order statuses
Write-Host "`nChecking order statuses:" -ForegroundColor Cyan

if ($orderId1) {
    try {
        $order1Status = Invoke-RestMethod -Uri "http://localhost:5001/api/orders/$orderId1" -Method Get
        Write-Host "Order 1 Status: $($order1Status.status)" -ForegroundColor $(if ($order1Status.status -eq "Completed") { "Green" } else { "Yellow" })
    } catch {
        Write-Host "Failed to get order 1 status: $($_.Exception.Message)" -ForegroundColor Red
    }
}

if ($orderId2) {
    try {
        $order2Status = Invoke-RestMethod -Uri "http://localhost:5001/api/orders/$orderId2" -Method Get
        Write-Host "Order 2 Status: $($order2Status.status)" -ForegroundColor $(
            switch ($order2Status.status) {
                "Completed" { "Green" }
                "Rejected" { "Red" }
                "Cancelled" { "Red" }
                default { "Yellow" }
            }
        )
    } catch {
        Write-Host "Failed to get order 2 status: $($_.Exception.Message)" -ForegroundColor Red
    }
}

if ($orderId3) {
    try {
        $order3Status = Invoke-RestMethod -Uri "http://localhost:5001/api/orders/$orderId3" -Method Get
        Write-Host "Order 3 Status: $($order3Status.status) (Expected: Cancelled)" -ForegroundColor $(if ($order3Status.status -eq "Cancelled") { "Green" } else { "Red" })
    } catch {
        Write-Host "Failed to get order 3 status: $($_.Exception.Message)" -ForegroundColor Red
    }
}

# Display all orders
Write-Host "`nAll Orders:" -ForegroundColor Cyan
try {
    $allOrders = Invoke-RestMethod -Uri "http://localhost:5001/api/orders" -Method Get
    $allOrders | ForEach-Object {
        Write-Host "Order $($_.id): Status = $($_.status), Amount = $($_.amount), Customer = $($_.customerId)"
    }
} catch {
    Write-Host "Failed to get all orders: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`nSaga testing completed!" -ForegroundColor Green
Write-Host "Check the service logs for detailed saga execution flow." -ForegroundColor Yellow