using Common.Application.Messaging;
using Common.Contracts.Models;

namespace Tracking.Application.Commands;

public record UpdateDriverLocationCommand(
    Guid DriverId,
    double Latitude,
    double Longitude) : ICommand<Result>;
