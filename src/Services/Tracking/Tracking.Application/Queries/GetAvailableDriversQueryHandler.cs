using Common.Application.Messaging;
using Tracking.Application.DTOs;
using Tracking.Domain.Repositories;

namespace Tracking.Application.Queries;

public class GetAvailableDriversQueryHandler : IQueryHandler<GetAvailableDriversQuery, List<DriverDto>>
{
    private readonly IDriverRepository _driverRepository;

    public GetAvailableDriversQueryHandler(IDriverRepository driverRepository)
    {
        _driverRepository = driverRepository;
    }

    public async Task<List<DriverDto>> Handle(GetAvailableDriversQuery request, CancellationToken cancellationToken)
    {
        var drivers = await _driverRepository.GetAvailableDriversAsync(cancellationToken);

        return drivers.Select(d => new DriverDto
        {
            Id = d.Id,
            Name = d.Name,
            PhoneNumber = d.PhoneNumber,
            VehicleNumber = d.VehicleNumber,
            Status = d.Status,
            CurrentLocation = d.CurrentLocation != null ? new LocationDto
            {
                Latitude = d.CurrentLocation.Latitude,
                Longitude = d.CurrentLocation.Longitude,
                Timestamp = d.CurrentLocation.Timestamp
            } : null,
            CurrentOrderId = d.CurrentOrderId
        }).ToList();
    }
}
