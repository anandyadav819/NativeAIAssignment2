using Common.Application.Messaging;
using Tracking.Application.DTOs;

namespace Tracking.Application.Queries;

public record GetDeliveryTrackingByOrderIdQuery(Guid OrderId) : IQuery<DeliveryTrackingDto?>;
