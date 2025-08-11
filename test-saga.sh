#!/bin/bash

# Bash script to test the Saga Demo application
# Run this after starting the services with: docker compose up --build

echo "Testing Saga Orchestration Pattern Demo"
echo "======================================="

# Wait for services to start
echo "Waiting for services to start..."
sleep 30

# Test 1: Create a successful order
echo ""
echo "Test 1: Creating a successful order with 1 Laptop"

ORDER1_RESPONSE=$(curl -s -X POST "http://localhost:5001/api/orders" \
  -H "Content-Type: application/json" \
  -d '{
    "customerId": "550e8400-e29b-41d4-a716-446655440000",
    "items": [
      {
        "productId": "11111111-1111-1111-1111-111111111111",
        "productName": "Laptop",
        "quantity": 1,
        "price": 999.99
      }
    ]
  }')

if [ $? -eq 0 ]; then
    ORDER1_ID=$(echo $ORDER1_RESPONSE | jq -r '.id')
    echo "✓ Order created successfully. Order ID: $ORDER1_ID"
else
    echo "✗ Failed to create order"
fi

# Test 2: Create an order that might fail due to payment
echo ""
echo "Test 2: Creating an order with 2 Mice (might fail due to payment)"

ORDER2_RESPONSE=$(curl -s -X POST "http://localhost:5001/api/orders" \
  -H "Content-Type: application/json" \
  -d '{
    "customerId": "550e8400-e29b-41d4-a716-446655440001",
    "items": [
      {
        "productId": "22222222-2222-2222-2222-222222222222",
        "productName": "Mouse",
        "quantity": 2,
        "price": 29.99
      }
    ]
  }')

if [ $? -eq 0 ]; then
    ORDER2_ID=$(echo $ORDER2_RESPONSE | jq -r '.id')
    echo "✓ Order created successfully. Order ID: $ORDER2_ID"
else
    echo "✗ Failed to create order"
fi

# Test 3: Create an order that will fail due to insufficient inventory
echo ""
echo "Test 3: Creating an order with 100 Laptops (will fail due to insufficient inventory)"

ORDER3_RESPONSE=$(curl -s -X POST "http://localhost:5001/api/orders" \
  -H "Content-Type: application/json" \
  -d '{
    "customerId": "550e8400-e29b-41d4-a716-446655440002",
    "items": [
      {
        "productId": "11111111-1111-1111-1111-111111111111",
        "productName": "Laptop",
        "quantity": 100,
        "price": 999.99
      }
    ]
  }')

if [ $? -eq 0 ]; then
    ORDER3_ID=$(echo $ORDER3_RESPONSE | jq -r '.id')
    echo "✓ Order created successfully. Order ID: $ORDER3_ID"
else
    echo "✗ Failed to create order"
fi

# Wait for saga processing
echo ""
echo "Waiting for saga processing to complete..."
sleep 10

# Check order statuses
echo ""
echo "Checking order statuses:"

if [ ! -z "$ORDER1_ID" ]; then
    ORDER1_STATUS=$(curl -s "http://localhost:5001/api/orders/$ORDER1_ID" | jq -r '.status')
    echo "Order 1 Status: $ORDER1_STATUS"
fi

if [ ! -z "$ORDER2_ID" ]; then
    ORDER2_STATUS=$(curl -s "http://localhost:5001/api/orders/$ORDER2_ID" | jq -r '.status')
    echo "Order 2 Status: $ORDER2_STATUS"
fi

if [ ! -z "$ORDER3_ID" ]; then
    ORDER3_STATUS=$(curl -s "http://localhost:5001/api/orders/$ORDER3_ID" | jq -r '.status')
    echo "Order 3 Status: $ORDER3_STATUS (Expected: Cancelled)"
fi

# Display all orders
echo ""
echo "All Orders:"
curl -s "http://localhost:5001/api/orders" | jq -r '.[] | "Order \(.id): Status = \(.status), Amount = \(.amount), Customer = \(.customerId)"'

echo ""
echo "Saga testing completed!"
echo "Check the service logs for detailed saga execution flow."