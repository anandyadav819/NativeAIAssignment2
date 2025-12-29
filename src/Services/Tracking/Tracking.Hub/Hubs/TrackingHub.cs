using Microsoft.AspNetCore.SignalR;
using Tracking.Application.DTOs;

namespace Tracking.SignalR.Hubs;

public class TrackingHub : Hub
{
    // Join a group to receive updates for a specific order
    public async Task JoinOrderGroup(string orderId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"order_{orderId}");
    }

    // Leave order group
    public async Task LeaveOrderGroup(string orderId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"order_{orderId}");
    }

    // Broadcast location update to all clients tracking this order
    public async Task BroadcastLocationUpdate(string orderId, LocationDto location)
    {
        await Clients.Group($"order_{orderId}").SendAsync("ReceiveLocationUpdate", location);
    }

    // Broadcast delivery status update
    public async Task BroadcastDeliveryStatus(string orderId, string status)
    {
        await Clients.Group($"order_{orderId}").SendAsync("ReceiveDeliveryStatus", status);
    }

    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
    }
}
