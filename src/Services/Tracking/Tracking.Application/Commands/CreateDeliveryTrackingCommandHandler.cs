using Common.Application.Messaging;
using Common.Contracts.Models;
using Microsoft.Extensions.Logging;
using Tracking.Domain.Entities;
using Tracking.Domain.Repositories;
using Tracking.Domain.ValueObjects;

namespace Tracking.Application.Commands;

public class CreateDeliveryTrackingCommandHandler : ICommandHandler<CreateDeliveryTrackingCommand, Result<Guid>>
{
    private readonly IDeliveryTrackingRepository _deliveryTrackingRepository;
    private readonly IDriverRepository _driverRepository;
    private readonly ILogger<CreateDeliveryTrackingCommandHandler> _logger;

    public CreateDeliveryTrackingCommandHandler(
        IDeliveryTrackingRepository deliveryTrackingRepository,
        IDriverRepository driverRepository,
        ILogger<CreateDeliveryTrackingCommandHandler> logger)
    {
        _deliveryTrackingRepository = deliveryTrackingRepository;
        _driverRepository = driverRepository;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(CreateDeliveryTrackingCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var driver = await _driverRepository.GetByIdAsync(request.DriverId, cancellationToken);
            if (driver == null)
            {
                return Result.Failure<Guid>($"Driver {request.DriverId} not found");
            }

            var pickupLocation = new Location(request.PickupLatitude, request.PickupLongitude);
            var deliveryLocation = new Location(request.DeliveryLatitude, request.DeliveryLongitude);

            var tracking = new DeliveryTracking(
                request.OrderId,
                request.DriverId,
                pickupLocation,
                deliveryLocation);

            driver.AssignOrder(request.OrderId);

            await _deliveryTrackingRepository.AddAsync(tracking, cancellationToken);
            await _driverRepository.UpdateAsync(driver, cancellationToken);

            _logger.LogInformation("Delivery tracking {TrackingId} created for order {OrderId} with driver {DriverId}",
                tracking.Id, request.OrderId, request.DriverId);

            return Result.Success(tracking.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating delivery tracking for order {OrderId}", request.OrderId);
            return Result.Failure<Guid>($"Failed to create delivery tracking: {ex.Message}");
        }
    }
}
