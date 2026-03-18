using aspnetapp.Models.Shared;
using aspnetapp.Services;
using Microsoft.AspNetCore.SignalR;

namespace aspnetapp.Hubs
{
    public class PrintHub : Hub
    {
        private readonly PrintTaskService _printTaskService;
        private readonly ILogger<PrintHub> _logger;

        public PrintHub(PrintTaskService printTaskService, ILogger<PrintHub> logger)
        {
            _printTaskService = printTaskService;
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, SignalRGroups.AllDevices);
            _logger.LogInformation("Device connected: {ConnectionId}", Context.ConnectionId);
            await Clients.Caller.SendAsync(SignalRMethods.DeviceRegistered, Context.ConnectionId);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, SignalRGroups.AllDevices);
            _logger.LogInformation("Device disconnected: {ConnectionId}", Context.ConnectionId);
            await Clients.Others.SendAsync(SignalRMethods.DeviceDisconnected, Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }

        public async Task ReportPrintResult(string workOrderId, int printQty, string? deviceId, string? operatorName)
        {
            _logger.LogInformation("Print result reported: WorkOrder={WorkOrderId}, Qty={PrintQty}, Device={DeviceId}",
                workOrderId, printQty, deviceId);
            await _printTaskService.UpdatePrintResultAsync(workOrderId, printQty, deviceId, operatorName);
        }

        public async Task JoinGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            _logger.LogInformation("Device {ConnectionId} joined group {GroupName}", Context.ConnectionId, groupName);
        }

        public async Task LeaveGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            _logger.LogInformation("Device {ConnectionId} left group {GroupName}", Context.ConnectionId, groupName);
        }
    }
}
