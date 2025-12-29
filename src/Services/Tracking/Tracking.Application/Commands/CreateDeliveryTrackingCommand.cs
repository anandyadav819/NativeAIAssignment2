using Common.Application.Messaging;
using Common.Contracts.Models;

namespace Tracking.Application.Commands;

public record CreateDeliveryTrackingCommand(
    Guid OrderId,
    Guid DriverId,
    double PickupLatitude,
    double PickupLongitude,
    double DeliveryLatitude,
    double DeliveryLongitude) : ICommand<Result<Guid>>;
