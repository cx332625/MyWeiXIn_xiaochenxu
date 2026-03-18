using aspnetapp.Hubs;
using aspnetapp.Models.Db;
using aspnetapp.Models.Shared;
using aspnetapp.Repositories;
using Microsoft.AspNetCore.SignalR;

namespace aspnetapp.Services
{
    public class PrintTaskService
    {
        private readonly WorkOrderRepository _workOrderRepository;
        private readonly IHubContext<PrintHub> _hubContext;
        private readonly PrintTaskSettings _settings;
        private readonly ILogger<PrintTaskService> _logger;

        public PrintTaskService(
            WorkOrderRepository workOrderRepository,
            IHubContext<PrintHub> hubContext,
            PrintTaskSettings settings,
            ILogger<PrintTaskService> logger)
        {
            _workOrderRepository = workOrderRepository;
            _hubContext = hubContext;
            _settings = settings;
            _logger = logger;
        }

        public async Task TriggerPrintJobAsync(WorkOrderRecord record, CancellationToken cancellationToken = default)
        {
            if (record.PrintStatus != PrintStatus.Pending)
            {
                _logger.LogDebug("Record {WorkorderNo}-{ProcedureNo} already processed, skipping.", record.WorkorderNo, record.ProcedureNo);
                return;
            }

            var printQty = CalculatePrintQty(record);
            if (printQty <= 0)
            {
                _logger.LogInformation("Calculated print qty is 0 for {WorkorderNo}, skipping.", record.WorkorderNo);
                await _workOrderRepository.UpdatePrintStatusAsync(record.Id!, PrintStatus.Skipped, 0);
                return;
            }

            var printJob = new PrintJobMessage
            {
                WorkOrderId = record.Id!,
                WorkorderNo = record.WorkorderNo,
                ProcedureNo = record.ProcedureNo,
                ProcedureName = record.ProcedureName,
                MaterialNo = record.MaterialNo,
                MaterialDesc = record.MaterialDesc,
                GoodQty = record.GoodQty,
                UserNames = record.UserNames,
                MachineNo = record.MachineNo,
                PrintQty = printQty,
                IssuedAt = DateTime.UtcNow
            };

            await _hubContext.Clients.Group(SignalRGroups.AllDevices)
                .SendAsync(SignalRMethods.ReceivePrintJob, printJob, cancellationToken);

            _logger.LogInformation("Print job dispatched for {WorkorderNo}-{ProcedureNo}, qty={PrintQty}", record.WorkorderNo, record.ProcedureNo, printQty);
        }

        public async Task UpdatePrintResultAsync(string workOrderId, int printQty, string? deviceId, string? operatorName)
        {
            var record = await _workOrderRepository.GetByIdAsync(workOrderId);
            if (record == null)
            {
                _logger.LogWarning("WorkOrder record not found: {WorkOrderId}", workOrderId);
                return;
            }

            var printRecord = new PrintRecord
            {
                PrintTime = DateTime.UtcNow,
                PrintQty = printQty,
                OperatorName = operatorName,
                DeviceId = deviceId,
                Status = PrintStatus.Completed
            };

            await _workOrderRepository.AddPrintRecordAsync(workOrderId, printRecord);

            var newTotal = record.PrintedTotalQty + printQty;
            var newStatus = newTotal >= (int)record.PlanQty ? PrintStatus.Completed : PrintStatus.Printing;
            await _workOrderRepository.UpdatePrintStatusAsync(workOrderId, newStatus, newTotal);

            await _hubContext.Clients.All.SendAsync(SignalRMethods.PrintStatusUpdated, new
            {
                WorkOrderId = workOrderId,
                WorkorderNo = record.WorkorderNo,
                PrintedTotalQty = newTotal,
                Status = newStatus.ToString()
            });

            _logger.LogInformation("Print result updated for {WorkOrderId}: qty={PrintQty}, total={NewTotal}, status={Status}",
                workOrderId, printQty, newTotal, newStatus);
        }

        private int CalculatePrintQty(WorkOrderRecord record)
        {
            var goodQty = (int)record.GoodQty;
            var perBatchSize = _settings.DefaultPerBatchSize;

            if (goodQty <= 0) return 0;

            if (goodQty <= perBatchSize)
            {
                return 1;
            }

            var batches = (int)Math.Ceiling((double)goodQty / perBatchSize);
            return Math.Min(batches, _settings.MaxPrintQty);
        }
    }

    public class PrintJobMessage
    {
        public string WorkOrderId { get; set; } = string.Empty;
        public string WorkorderNo { get; set; } = string.Empty;
        public string? ProcedureNo { get; set; }
        public string? ProcedureName { get; set; }
        public string? MaterialNo { get; set; }
        public string? MaterialDesc { get; set; }
        public decimal GoodQty { get; set; }
        public string? UserNames { get; set; }
        public string? MachineNo { get; set; }
        public int PrintQty { get; set; }
        public DateTime IssuedAt { get; set; }
    }

    public class PrintTaskSettings
    {
        public int DefaultPerBatchSize { get; set; } = 100;
        public int MaxPrintQty { get; set; } = 10;
    }
}
