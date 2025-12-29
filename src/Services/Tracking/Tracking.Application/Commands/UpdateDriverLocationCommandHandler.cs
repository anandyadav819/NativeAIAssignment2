using Common.Application.Messaging;
using Common.Contracts.Models;
using Microsoft.Extensions.Logging;
using Tracking.Domain.Repositories;
using Tracking.Domain.ValueObjects;

namespace Tracking.Application.Commands;

public class UpdateDriverLocationCommandHandler : ICommandHandler<UpdateDriverLocationCommand, Result>
{
    private readonly IDriverRepository _driverRepository;
    private readonly IDeliveryTrackingRepository _deliveryTrackingRepository;
    private readonly ILogger<UpdateDriverLocationCommandHandler> _logger;

    public UpdateDriverLocationCommandHandler(
        IDriverRepository driverRepository,
        IDeliveryTrackingRepository deliveryTrackingRepository,
        ILogger<UpdateDriverLocationCommandHandler> logger)
    {
        _driverRepository = driverRepository;
        _deliveryTrackingRepository = deliveryTrackingRepository;
        _logger = logger;
    }

    public async Task<Result> Handle(UpdateDriverLocationCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var driver = await _driverRepository.GetByIdAsync(request.DriverId, cancellationToken);
            if (driver == null)
            {
                return Result.Failure($"Driver {request.DriverId} not found");
            }

            driver.UpdateLocation(request.Latitude, request.Longitude);
            await _driverRepository.UpdateAsync(driver, cancellationToken);

            // If driver has an active order, update delivery tracking
            if (driver.CurrentOrderId.HasValue)
            {
                var tracking = await _deliveryTrackingRepository.GetByOrderIdAsync(
                    driver.CurrentOrderId.Value,
                    cancellationToken);

                if (tracking != null)
                {
                    var location = new Location(request.Latitude, request.Longitude);
                    tracking.UpdateCurrentLocation(location);
                    await _deliveryTrackingRepository.UpdateAsync(tracking, cancellationToken);
                }
            }

            _logger.LogInformation("Driver {DriverId} location updated to ({Lat}, {Lon})",
                request.DriverId, request.Latitude, request.Longitude);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating driver location for {DriverId}", request.DriverId);
            return Result.Failure($"Failed to update driver location: {ex.Message}");
        }
    }
}
