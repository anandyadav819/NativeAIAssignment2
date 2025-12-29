using Common.Application.Messaging;
using Common.Caching;
using Tracking.Application.DTOs;
using Tracking.Domain.Repositories;

namespace Tracking.Application.Queries;

public class GetDeliveryTrackingByOrderIdQueryHandler : IQueryHandler<GetDeliveryTrackingByOrderIdQuery, DeliveryTrackingDto?>
{
    private readonly IDeliveryTrackingRepository _deliveryTrackingRepository;
    private readonly ICacheService _cacheService;

    public GetDeliveryTrackingByOrderIdQueryHandler(
        IDeliveryTrackingRepository deliveryTrackingRepository,
        ICacheService cacheService)
    {
        _deliveryTrackingRepository = deliveryTrackingRepository;
        _cacheService = cacheService;
    }

    public async Task<DeliveryTrackingDto?> Handle(GetDeliveryTrackingByOrderIdQuery request, CancellationToken cancellationToken)
    {
        // Don't cache active deliveries as they change frequently
        var tracking = await _deliveryTrackingRepository.GetByOrderIdAsync(request.OrderId, cancellationToken);

        if (tracking == null)
        {
            return null;
        }

        return new DeliveryTrackingDto
        {
            Id = tracking.Id,
            OrderId = tracking.OrderId,
            DriverId = tracking.DriverId,
            PickupLocation = new LocationDto
            {
                Latitude = tracking.PickupLocation.Latitude,
                Longitude = tracking.PickupLocation.Longitude,
                Timestamp = tracking.PickupLocation.Timestamp
            },
            DeliveryLocation = new LocationDto
            {
                Latitude = tracking.DeliveryLocation.Latitude,
                Longitude = tracking.DeliveryLocation.Longitude,
                Timestamp = tracking.DeliveryLocation.Timestamp
            },
            CurrentLocation = tracking.CurrentLocation != null ? new LocationDto
            {
                Latitude = tracking.CurrentLocation.Latitude,
                Longitude = tracking.CurrentLocation.Longitude,
                Timestamp = tracking.CurrentLocation.Timestamp
            } : null,
            PickedUpAt = tracking.PickedUpAt,
            DeliveredAt = tracking.DeliveredAt,
            EstimatedDistanceKm = tracking.EstimatedDistanceKm,
            DistanceFromDelivery = tracking.GetDistanceFromDelivery()
        };
    }
}
