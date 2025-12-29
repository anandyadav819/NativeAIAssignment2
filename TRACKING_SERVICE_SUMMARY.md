# Tracking Service Implementation Summary

## Overview
The Tracking Service is responsible for real-time GPS tracking of delivery drivers, managing driver locations, and providing delivery tracking information to customers. It implements SignalR for real-time WebSocket communication.

## Architecture Layers

### 1. Tracking.Domain
**Purpose**: Core domain entities, value objects, domain events, and repository interfaces.

**Key Components**:
- **Location (Value Object)**: GPS coordinates with Haversine distance calculation
  - Properties: Latitude, Longitude, Timestamp
  - Method: `CalculateDistanceInKm(Location other)` using Haversine formula
  - Validation: Latitude (-90 to 90), Longitude (-180 to 180)

- **Driver (Aggregate Root)**:
  - Properties: Id, Name, PhoneNumber, VehicleNumber, Status, CurrentLocation, CurrentOrderId
  - Status: Offline, Available, Busy, OnBreak, Inactive
  - Methods: UpdateLocation, AssignToOrder, CompleteOrder, ChangeStatus

- **DeliveryTracking (Aggregate Root)**:
  - Properties: Id, OrderId, DriverId, PickupLocation, DeliveryLocation, CurrentLocation, LocationHistory, EstimatedDistanceKm
  - Methods: UpdateLocation, MarkPickedUp, MarkDelivered

- **Domain Events**:
  - DriverCreatedDomainEvent
  - DriverStatusChangedDomainEvent
  - DriverAssignedToOrderDomainEvent
  - DriverLocationUpdatedDomainEvent
  - DeliveryCompletedDomainEvent

- **Repository Interfaces**:
  - IDriverRepository: GetOnlineDrivers, GetAvailableDrivers, GetByOrderId
  - IDeliveryTrackingRepository: GetByOrderId, GetActiveDeliveries

### 2. Tracking.Application
**Purpose**: CQRS commands and queries for tracking operations.

**Key Components**:
- **DTOs**: 
  - DriverDto: Driver information with current location
  - LocationDto: GPS coordinates
  - DeliveryTrackingDto: Full delivery tracking info with location history

- **Commands**:
  - UpdateDriverLocationCommand: Updates driver GPS location
    - Validates coordinates (-90/90 lat, -180/180 lon)
    - Updates DeliveryTracking if driver has active order
  - CreateDeliveryTrackingCommand: Initializes tracking for new delivery
    - Assigns driver to order
    - Sets pickup and delivery locations

- **Queries**:
  - GetDeliveryTrackingByOrderIdQuery: Real-time tracking info (not cached)
  - GetAvailableDriversQuery: Lists available drivers with locations

### 3. Tracking.Infrastructure
**Purpose**: Data access and persistence using Entity Framework Core.

**Key Components**:
- **TrackingDbContext**: EF Core context with "tracking" schema
  - Automatic timestamp updates (CreatedAt, UpdatedAt)

- **Entity Configurations**:
  - DriverConfiguration: Maps Driver with owned CurrentLocation
  - DeliveryTrackingConfiguration: Maps DeliveryTracking with owned locations and location history collection
  - Location history stored in separate table: location_history

- **Repositories**:
  - DriverRepository: Implements IDriverRepository
  - DeliveryTrackingRepository: Implements IDeliveryTrackingRepository with location history loading

- **DependencyInjection**: Registers DbContext and repositories

### 4. Tracking.Hub
**Purpose**: SignalR hub for real-time location broadcasting.

**Key Components**:
- **TrackingHub** (namespace: Tracking.SignalR.Hubs):
  - JoinOrderGroup(orderId): Subscribe to order updates
  - LeaveOrderGroup(orderId): Unsubscribe from order updates
  - BroadcastLocationUpdate(orderId, location): Push location to all subscribers
  - BroadcastDeliveryStatus(orderId, status): Push status updates

**Client Events**:
- ReceiveLocationUpdate: Driver location update
- ReceiveDeliveryStatus: Delivery status change

### 5. Tracking.API
**Purpose**: REST API endpoints and SignalR configuration.

**Key Components**:
- **DriversController**:
  - POST /api/drivers/{driverId}/location: Update driver GPS location

- **TrackingController**:
  - GET /api/tracking/order/{orderId}: Get delivery tracking info
  - GET /api/tracking/drivers/available: Get available drivers

- **SignalR Hub**: Exposed at /hubs/tracking

- **Configuration**:
  - CORS enabled for SignalR (localhost:3000, localhost:5173)
  - Swagger for API documentation
  - Observability (Serilog, OpenTelemetry)
  - Redis for hybrid caching

## Database Schema

### tracking.drivers
```sql
- id (uuid, PK)
- name (varchar(200))
- phone_number (varchar(20))
- vehicle_number (varchar(50))
- status (varchar(50)) - enum as string
- current_order_id (uuid, nullable)
- current_latitude (double)
- current_longitude (double)
- location_timestamp (timestamp)
- created_at (timestamp)
- updated_at (timestamp)
```

### tracking.delivery_tracking
```sql
- id (uuid, PK)
- order_id (uuid, unique)
- driver_id (uuid)
- pickup_latitude (double)
- pickup_longitude (double)
- pickup_timestamp (timestamp)
- delivery_latitude (double)
- delivery_longitude (double)
- delivery_timestamp (timestamp)
- current_latitude (double, nullable)
- current_longitude (double, nullable)
- current_timestamp (timestamp, nullable)
- picked_up_at (timestamp, nullable)
- delivered_at (timestamp, nullable)
- estimated_distance_km (decimal(10,2))
- created_at (timestamp)
- updated_at (timestamp)
```

### tracking.location_history
```sql
- id (int, PK, auto-increment)
- delivery_tracking_id (uuid, FK)
- latitude (double)
- longitude (double)
- timestamp (timestamp)
```

## API Endpoints

### Update Driver Location
```http
POST /api/drivers/{driverId}/location
Content-Type: application/json

{
  "latitude": 37.7749,
  "longitude": -122.4194
}

Response: 200 OK
{
  "id": "guid",
  "name": "John Doe",
  "phoneNumber": "+1234567890",
  "vehicleNumber": "ABC123",
  "status": "Busy",
  "currentLocation": {
    "latitude": 37.7749,
    "longitude": -122.4194,
    "timestamp": "2024-01-01T12:00:00Z"
  }
}
```

### Get Delivery Tracking
```http
GET /api/tracking/order/{orderId}

Response: 200 OK
{
  "id": "guid",
  "orderId": "guid",
  "driverId": "guid",
  "pickupLocation": { ... },
  "deliveryLocation": { ... },
  "currentLocation": { ... },
  "locationHistory": [ ... ],
  "pickedUpAt": "2024-01-01T12:00:00Z",
  "deliveredAt": null,
  "estimatedDistanceKm": 5.2
}
```

### Get Available Drivers
```http
GET /api/tracking/drivers/available

Response: 200 OK
[
  {
    "id": "guid",
    "name": "John Doe",
    "status": "Available",
    "currentLocation": { ... }
  }
]
```

## SignalR Real-Time Updates

### Connect to Hub
```javascript
const connection = new signalR.HubConnectionBuilder()
    .withUrl("https://localhost:5001/hubs/tracking")
    .build();

await connection.start();
```

### Subscribe to Order Updates
```javascript
// Join order group
await connection.invoke("JoinOrderGroup", orderId);

// Listen for location updates
connection.on("ReceiveLocationUpdate", (location) => {
    console.log("Driver location:", location);
    // Update map marker
});

// Listen for status updates
connection.on("ReceiveDeliveryStatus", (status) => {
    console.log("Delivery status:", status);
});

// Leave order group
await connection.invoke("LeaveOrderGroup", orderId);
```

## Performance Considerations

1. **GPS Update Rate**: Supports 2,000 GPS events/second target
2. **Location History**: Stored in separate table to avoid loading entire history on every query
3. **SignalR Groups**: Order-based grouping ensures only relevant clients receive updates
4. **No Caching**: Tracking queries are not cached due to real-time nature
5. **Distance Calculation**: Haversine formula for accurate GPS distance calculation

## Configuration

### appsettings.json
```json
{
  "ConnectionStrings": {
    "FoodDeliveryDb": "Host=localhost;Port=5432;Database=fooddelivery;Username=postgres;Password=postgres"
  },
  "Redis": {
    "ConnectionString": "localhost:6379",
    "InstanceName": "Tracking:"
  },
  "Cors": {
    "AllowedOrigins": ["http://localhost:3000", "http://localhost:5173"]
  }
}
```

## Next Steps

1. Run database migration:
   ```bash
   dotnet ef migrations add InitialCreate --project src/Services/Tracking/Tracking.Infrastructure --startup-project src/Services/Tracking/Tracking.API --context TrackingDbContext
   dotnet ef database update --project src/Services/Tracking/Tracking.Infrastructure --startup-project src/Services/Tracking/Tracking.API --context TrackingDbContext
   ```

2. Test REST endpoints with Swagger (https://localhost:5001/swagger)

3. Test SignalR real-time updates with a client application

4. Load test GPS updates (target: 2,000 events/sec)

5. Implement integration with Order Service for automatic tracking creation

## Build Status
✅ All 5 Tracking projects build successfully
- Tracking.Domain
- Tracking.Application
- Tracking.Infrastructure
- Tracking.Hub
- Tracking.API
