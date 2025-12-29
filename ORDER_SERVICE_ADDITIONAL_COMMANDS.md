# Order Service Additional Commands Implementation

## Overview
Extended the Order Service with three critical commands to support payment processing, order cancellation, and driver assignment functionality.

## New Commands Implemented

### 1. Process Payment Command

**Purpose**: Handle payment processing for orders

**Files Created**:
- `ProcessPaymentCommand.cs` - Command definition
- `ProcessPaymentCommandHandler.cs` - Command handler with payment gateway integration placeholder
- `ProcessPaymentCommandValidator.cs` - FluentValidation rules

**Command Structure**:
```csharp
public record ProcessPaymentCommand(
    Guid OrderId, 
    string PaymentMethod, 
    string PaymentTransactionId
) : ICommand<ProcessPaymentResult>;

public record ProcessPaymentResult(bool Success, string Message);
```

**Validation Rules**:
- Order ID is required
- Payment method is required (max 50 characters)
- Payment transaction ID is required (max 100 characters)

**Business Logic**:
- Validates order exists
- Confirms payment using `Order.ConfirmPayment(transactionId)`
- Updates order status from `Pending`/`PaymentProcessing` → `PaymentConfirmed`
- On failure: marks payment as failed using `Order.MarkPaymentFailed(reason)`
- Emits `OrderPaymentConfirmedDomainEvent` or `OrderPaymentFailedDomainEvent`

**API Endpoint**:
```http
POST /api/orders/{id}/payment
Content-Type: application/json

{
  "paymentMethod": "CreditCard",
  "paymentTransactionId": "TXN123456789"
}

Response 200 OK:
{
  "message": "Payment processed successfully"
}

Response 400 Bad Request:
{
  "error": "Cannot confirm payment for order in Delivered status"
}
```

---

### 2. Cancel Order Command

**Purpose**: Allow order cancellation with reason tracking

**Files Created**:
- `CancelOrderCommand.cs` - Command definition
- `CancelOrderCommandHandler.cs` - Command handler
- `CancelOrderCommandValidator.cs` - FluentValidation rules

**Command Structure**:
```csharp
public record CancelOrderCommand(
    Guid OrderId, 
    string Reason
) : ICommand<CancelOrderResult>;

public record CancelOrderResult(bool Success, string Message);
```

**Validation Rules**:
- Order ID is required
- Cancellation reason is required (max 500 characters)

**Business Logic**:
- Validates order exists
- Cancels order using `Order.Cancel(reason)`
- Can cancel from any status except `Delivered` and `Cancelled`
- Updates order status to `Cancelled`
- Emits `OrderCancelledDomainEvent` with previous status and reason

**API Endpoint**:
```http
POST /api/orders/{id}/cancel
Content-Type: application/json

{
  "reason": "Customer requested cancellation"
}

Response 200 OK:
{
  "message": "Order cancelled successfully"
}

Response 400 Bad Request:
{
  "error": "Cannot cancel order in Delivered status"
}
```

---

### 3. Assign Driver Command

**Purpose**: Assign a delivery driver to an order ready for pickup

**Files Created**:
- `AssignDriverCommand.cs` - Command definition
- `AssignDriverCommandHandler.cs` - Command handler
- `AssignDriverCommandValidator.cs` - FluentValidation rules

**Command Structure**:
```csharp
public record AssignDriverCommand(
    Guid OrderId, 
    Guid DriverId
) : ICommand<AssignDriverResult>;

public record AssignDriverResult(bool Success, string Message);
```

**Validation Rules**:
- Order ID is required
- Driver ID is required

**Business Logic**:
- Validates order exists
- Assigns driver using `Order.AssignDriver(driverId)`
- Can only assign when order status is `ReadyForPickup`
- Updates order status to `OutForDelivery`
- Sets `DriverId` on the order
- Emits `OrderAssignedToDriverDomainEvent`

**API Endpoint**:
```http
POST /api/orders/{id}/assign-driver
Content-Type: application/json

{
  "driverId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}

Response 200 OK:
{
  "message": "Driver assigned successfully"
}

Response 400 Bad Request:
{
  "error": "Cannot assign driver to order in Pending status"
}
```

---

## Order State Machine Flow

The new commands fit into the complete order lifecycle:

```
1. Pending (order created)
   ↓
2. PaymentProcessing
   ↓ [ProcessPayment Command]
3. PaymentConfirmed ──→ [Cancel Command]
   ↓                         ↓
4. Confirmed ────────→ [Cancelled]
   ↓
5. Preparing
   ↓
6. ReadyForPickup
   ↓ [AssignDriver Command]
7. OutForDelivery
   ↓
8. Delivered
```

**Valid State Transitions**:
- `ProcessPayment`: Pending/PaymentProcessing → PaymentConfirmed/PaymentFailed
- `Cancel`: Any status (except Delivered/Cancelled) → Cancelled
- `AssignDriver`: ReadyForPickup → OutForDelivery

---

## Updated Controller

**File Modified**: `OrdersController.cs`

**New Endpoints Added**:
1. `POST /api/orders/{id}/payment` - Process payment
2. `POST /api/orders/{id}/cancel` - Cancel order
3. `POST /api/orders/{id}/assign-driver` - Assign driver

**Request DTOs**:
```csharp
public record ProcessPaymentRequest(string PaymentMethod, string PaymentTransactionId);
public record CancelOrderRequest(string Reason);
public record AssignDriverRequest(Guid DriverId);
```

---

## Error Handling

All commands follow consistent error handling patterns:

1. **Order Not Found**: Returns 400 with message "Order {id} not found"
2. **Invalid State Transition**: Returns 400 with specific state error message
3. **Validation Errors**: Caught by FluentValidation middleware before reaching handler
4. **General Exceptions**: Logged and returned as generic failure messages

---

## Logging

All handlers include comprehensive logging:
- Information: Successful operations
- Warning: Business rule violations (invalid state transitions)
- Error: Unexpected exceptions

**Log Examples**:
```
[INFO] Processing payment for order {OrderId}
[INFO] Payment confirmed for order {OrderId} with transaction {TransactionId}
[WARN] Failed to process payment for order {OrderId}
[ERROR] Error processing payment for order {OrderId}
```

---

## Domain Events

The commands leverage existing domain events:

1. **OrderPaymentConfirmedDomainEvent**
   - Triggered by: `ProcessPaymentCommand`
   - Contains: OrderId, TransactionId

2. **OrderPaymentFailedDomainEvent**
   - Triggered by: `ProcessPaymentCommand` (on failure)
   - Contains: OrderId, Reason

3. **OrderCancelledDomainEvent**
   - Triggered by: `CancelOrderCommand`
   - Contains: OrderId, Reason, PreviousStatus

4. **OrderAssignedToDriverDomainEvent**
   - Triggered by: `AssignDriverCommand`
   - Contains: OrderId, DriverId

---

## Integration Points

### Payment Gateway Integration
Currently, `ProcessPaymentCommandHandler` has a placeholder for payment gateway integration:
```csharp
// Simulate payment processing (in real scenario, integrate with payment gateway)
// For now, we'll assume payment is successful
order.ConfirmPayment(request.PaymentTransactionId);
```

**Future Enhancement**: Integrate with real payment providers like Stripe, PayPal, or Square.

### Tracking Service Integration
The `AssignDriverCommand` sets the stage for integration with the Tracking Service:
- When a driver is assigned, the Tracking Service can create a `DeliveryTracking` entity
- Use domain events or integration events to notify the Tracking Service

---

## Testing the New Endpoints

### 1. Create an Order
```bash
curl -X POST http://localhost:5000/api/orders \
  -H "Content-Type: application/json" \
  -d '{
    "customerId": "customer-guid",
    "restaurantId": "restaurant-guid",
    "deliveryAddress": "123 Main St",
    "items": [...]
  }'
```

### 2. Process Payment
```bash
curl -X POST http://localhost:5000/api/orders/{orderId}/payment \
  -H "Content-Type: application/json" \
  -d '{
    "paymentMethod": "CreditCard",
    "paymentTransactionId": "TXN123456789"
  }'
```

### 3. Assign Driver (after order is ReadyForPickup)
```bash
curl -X POST http://localhost:5000/api/orders/{orderId}/assign-driver \
  -H "Content-Type: application/json" \
  -d '{
    "driverId": "driver-guid"
  }'
```

### 4. Cancel Order
```bash
curl -X POST http://localhost:5000/api/orders/{orderId}/cancel \
  -H "Content-Type: application/json" \
  -d '{
    "reason": "Customer changed mind"
  }'
```

---

## Build Status
✅ All Order Service projects build successfully
- Order.Domain: No changes (already had methods)
- Order.Application: Added 9 new files (3 commands × 3 files each)
- Order.API: Updated controller with 3 new endpoints
- Order.Infrastructure: No changes needed

---

## Summary

Successfully implemented three critical commands for the Order Service:
1. ✅ **ProcessPaymentCommand** - Payment processing with success/failure handling
2. ✅ **CancelOrderCommand** - Order cancellation with reason tracking
3. ✅ **AssignDriverCommand** - Driver assignment for delivery

All commands include:
- Proper validation using FluentValidation
- Comprehensive error handling
- Domain event emission
- Structured logging
- REST API endpoints with Swagger documentation

The Order Service now supports the complete order lifecycle from creation through payment, assignment, and delivery or cancellation.
