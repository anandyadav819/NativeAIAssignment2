using Common.Application.Messaging;
using Tracking.Application.DTOs;

namespace Tracking.Application.Queries;

public record GetAvailableDriversQuery : IQuery<List<DriverDto>>;
