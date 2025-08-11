# 🚀 Saga Demo - Distributed Transaction Management

A comprehensive demonstration of the **Saga Orchestration Pattern** using .NET 8, MassTransit, MongoDB, and RabbitMQ. This project showcases how to manage distributed transactions across multiple microservices with proper compensation mechanisms and comprehensive monitoring.

## 📋 Table of Contents

- [Overview](#overview)
- [Architecture](#architecture)
- [Features](#features)
- [Prerequisites](#prerequisites)
- [Quick Start](#quick-start)
- [Services](#services)
- [Local Development](#local-development)
- [Testing](#testing)
- [Configuration](#configuration)
- [Monitoring & Logging](#monitoring--logging)
- [Troubleshooting](#troubleshooting)

## 🎯 Overview

The Saga Demo implements a distributed e-commerce order processing system that demonstrates:

- **Saga Orchestration Pattern** for managing distributed transactions
- **Event-driven communication** between microservices  
- **Compensation mechanisms** for handling failures
- **State management** across service boundaries
- **Comprehensive logging** with workflow visualization
- **Docker containerization** for easy deployment

## 🏗️ Architecture

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Order Service │    │ Payment Service │    │Inventory Service│
│     Port 5001   │    │     Port 5002   │    │     Port 5003   │
│                 │    │                 │    │                 │
│ - Create Orders │    │ - Process Pay   │    │ - Reserve Items │
│ - Update Status │    │ - Handle Refunds│    │ - Release Items │
└─────────────────┘    └─────────────────┘    └─────────────────┘
         │                        │                        │
         └────────────────────────┼────────────────────────┘
                                  │
                    ┌─────────────────────────┐
                    │   Saga Orchestrator     │
                    │      Port 5004          │
                    │                         │
                    │ - Coordinate Workflow   │
                    │ - Handle Compensation   │
                    │ - Detailed Logging      │
                    └─────────────────────────┘
                                  │
         ┌────────────────────────┼────────────────────────┐
         │                        │                        │
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│    MongoDB      │    │    RabbitMQ     │    │   Docker        │
│   Port 27018    │    │  Port 5672      │    │                 │
│                 │    │  UI: 15672      │    │ - Containerization│
│ - Data Storage  │    │ - Message Queue │    │ - Orchestration │
│ - Saga State    │    │ - Event Bus     │    │ - Development   │
└─────────────────┘    └─────────────────┘    └─────────────────┘
```

## ✨ Features

- **🎭 Saga Orchestration**: Central coordinator manages complex workflows
- **🔄 Event-Driven**: Asynchronous communication via RabbitMQ
- **🛡️ Fault Tolerance**: Automatic compensation for failed transactions
- **📊 State Management**: Persistent saga state in MongoDB (no transactions required)
- **🔍 Comprehensive Logging**: Detailed workflow tracking with emojis for easy monitoring
- **🐳 Docker Support**: Complete containerized deployment
- **🧪 Test Scenarios**: Multiple test cases for different outcomes
- **⚙️ Local Development**: Easy debugging with consistent ports and configuration
- **🚨 Error Handling**: Robust error handling without MongoDB transactions

## 🔄 Saga Workflow

The order processing saga follows this comprehensive workflow with detailed logging:

1. **🚀 Order Created** → Start saga with comprehensive logging
2. **💳 Process Payment** → Request payment processing
   - ✅ **Success**: Proceed to inventory reservation
   - ❌ **Failure**: Reject order and finalize saga
3. **📦 Reserve Inventory** → Check and reserve product inventory
   - ✅ **Success**: Approve and complete order
   - ❌ **Failure**: Refund payment (if processed) and cancel order

Each step includes detailed emoji-enhanced logging for easy monitoring and debugging.

## 📋 Prerequisites

- **Docker and Docker Compose** (required)
- **.NET 8 SDK** (for local development)
- **PowerShell** (for running test scripts)

## 🚀 Quick Start

1. **Clone and navigate to the repository**:
```bash
git clone <repository-url>
cd SagaDemo
```

2. **Start all services**:
```bash
docker-compose up --build
```

3. **Wait for services** - Check logs for "Application started" messages

4. **Run automated tests**:
```powershell
./test-saga.ps1
```

## 🧪 Testing

### Automated Test Script

The project includes a comprehensive PowerShell test script:

```powershell
# Run all test scenarios
./test-saga.ps1

# The script tests:
# - Successful order processing
# - Payment failure scenarios  
# - Inventory shortage scenarios
# - Order status verification
```

### Manual Testing

#### 1. Create a Successful Order

```bash
curl -X POST "http://localhost:5001/api/orders" \
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
  }'
```

#### 2. Test Payment Failure

```bash
# Payment service has built-in 20% failure rate for testing
curl -X POST "http://localhost:5001/api/orders" \
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
  }'
```

#### 3. Test Inventory Shortage

```bash
# Order exceeds available inventory to trigger failure
curl -X POST "http://localhost:5001/api/orders" \
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
  }'
```

#### 4. Check Order Status

```bash
# Get all orders
curl "http://localhost:5001/api/orders"

# Get specific order by ID
curl "http://localhost:5001/api/orders/{orderId}"
```

## 📦 Available Products

The inventory service initializes with these products:

| Product | ID | Available Stock |
|---------|----|--------------:|
| **Laptop** | `11111111-1111-1111-1111-111111111111` | 10 |
| **Mouse** | `22222222-2222-2222-2222-222222222222` | 50 |
| **Keyboard** | `33333333-3333-3333-3333-333333333333` | 30 |
| **Monitor** | `44444444-4444-4444-4444-444444444444` | 15 |

## 🖥️ Local Development

For debugging and local development, services can run on localhost with consistent port configuration:

### Service Ports

- **Order Service**: `localhost:5001`
- **Payment Service**: `localhost:5002`  
- **Inventory Service**: `localhost:5003`
- **Saga Orchestrator**: `localhost:5004`
- **MongoDB**: `localhost:27018` (avoiding conflicts with default 27017)
- **RabbitMQ**: `localhost:5672` (UI: 15672)

### Local Development Setup

1. **Start infrastructure only**:
```bash
docker-compose up rabbitmq mongodb
```

2. **Run services individually**:
```bash
# In separate terminals
cd src/SagaDemo.OrderService && dotnet run
cd src/SagaDemo.PaymentService && dotnet run
cd src/SagaDemo.InventoryService && dotnet run
cd src/SagaDemo.SagaOrchestrator && dotnet run
```

### MongoDB Connection

- **Docker**: `mongodb://mongodb:27017`
- **Local**: `mongodb://localhost:27018`
- **MongoDB Compass**: `mongodb://localhost:27018`

## ⚙️ Configuration

### Environment-Specific Settings

- **Development**: Uses `appsettings.Development.json` with localhost connections
- **Docker**: Uses `appsettings.json` with container hostnames
- **Ports**: Configured via `launchSettings.json` for consistent local debugging

### Key Configuration Features

- **No MongoDB Transactions**: Optimized for standalone MongoDB (no replica set required)
- **Enum Serialization**: Proper string representation in JSON responses
- **Comprehensive Logging**: Emoji-enhanced logs for easy workflow tracking
- **Fault Tolerance**: Automatic compensation without distributed transactions

## 🔍 Monitoring & Logging

### Web Interfaces

- **RabbitMQ Management**: [http://localhost:15672](http://localhost:15672) (guest/guest)
- **Order Service API**: [http://localhost:5001/swagger](http://localhost:5001/swagger)
- **Payment Service API**: [http://localhost:5002/swagger](http://localhost:5002/swagger)
- **Inventory Service API**: [http://localhost:5003/swagger](http://localhost:5003/swagger)
- **Saga Orchestrator API**: [http://localhost:5004/swagger](http://localhost:5004/swagger)

### Saga Logging

The saga orchestrator provides detailed logging with emojis for easy monitoring:

- 🚀 **Saga Started**: Order creation and initialization
- 💳 **Payment Processing**: Payment requests and outcomes
- 📦 **Inventory Operations**: Reservation attempts and results
- ✅ **Success States**: Approvals and completions
- ❌ **Failure Handling**: Payment failures and inventory issues
- 💰 **Compensation**: Refund processing
- 🏁 **Finalization**: Saga completion or cancellation

## 📊 Order Status Flow

```
Created → PaymentProcessing → InventoryReserving → Approved → Completed
    ↓              ↓                    ↓
Rejected    PaymentFailed      InventoryFailed → Refunding → Cancelled
```

### Status Descriptions

- **Created**: Order initially created, saga started
- **PaymentProcessing**: Payment request sent, awaiting response
- **InventoryReserving**: Payment succeeded, requesting inventory reservation
- **Approved**: Both payment and inventory successful
- **Completed**: Order finalized successfully
- **Rejected**: Payment failed, order rejected immediately
- **Cancelled**: Inventory failed, payment refunded, order cancelled

## 🛠️ Troubleshooting

### Common Issues

1. **MongoDB Connection**: Ensure port 27018 is available (different from default 27017)
2. **RabbitMQ Access**: Check guest/guest credentials and port 5672/15672
3. **Service Startup**: Wait for all services to show "Application started" in logs
4. **Test Failures**: Payment service has intentional 20% failure rate for testing

### Docker Commands

```bash
# View logs for specific service
docker-compose logs -f orderservice

# Restart specific service
docker-compose restart orderservice

# Clean restart
docker-compose down -v && docker-compose up --build

# Check service health
docker-compose ps
```

## 🏁 Stopping the Application

```bash
# Stop services
docker-compose down

# Stop and remove volumes (clean slate)
docker-compose down -v

# Stop and remove images
docker-compose down --rmi all
```

## 📁 Project Structure

```
SagaDemo/
├── src/
│   ├── SagaDemo.Contracts/           # 📋 Shared message contracts and events
│   ├── SagaDemo.OrderService/        # 📝 Order management and coordination
│   ├── SagaDemo.PaymentService/      # 💳 Payment processing with test failures
│   ├── SagaDemo.InventoryService/    # 📦 Inventory management (no transactions)
│   └── SagaDemo.SagaOrchestrator/    # 🎭 Saga state machine with comprehensive logging
├── docker-compose.yml               # 🐳 Container orchestration
├── test-saga.ps1                   # 🧪 Automated test scenarios
├── .gitignore                      # 🚫 Git ignore patterns
└── SagaDemo.sln                    # 🔧 Solution file
```

---

**🎉 Ready to explore distributed transaction management with the Saga pattern!**