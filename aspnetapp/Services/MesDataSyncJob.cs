using System.Text.Json;
using aspnetapp.Models.Db;
using aspnetapp.Models.Shared;
using aspnetapp.Repositories;
using Quartz;

namespace aspnetapp.Services
{
    [DisallowConcurrentExecution]
    public class MesDataSyncJob : IJob
    {
        private readonly MesApiClient _mesApiClient;
        private readonly WorkOrderRepository _workOrderRepository;
        private readonly PrintTaskService _printTaskService;
        private readonly ILogger<MesDataSyncJob> _logger;

        public MesDataSyncJob(
            MesApiClient mesApiClient,
            WorkOrderRepository workOrderRepository,
            PrintTaskService printTaskService,
            ILogger<MesDataSyncJob> logger)
        {
            _mesApiClient = mesApiClient;
            _workOrderRepository = workOrderRepository;
            _printTaskService = printTaskService;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var endTime = DateTime.UtcNow;
            var startTime = endTime.AddMinutes(-1);

            var startStr = startTime.ToString("yyyy-MM-dd HH:mm:ss");
            var endStr = endTime.ToString("yyyy-MM-dd HH:mm:ss");

            _logger.LogInformation("MES data sync started for {StartTime} - {EndTime}", startStr, endStr);

            var result = await _mesApiClient.QueryProcedureReportByTimeAsync(startStr, endStr, context.CancellationToken);

            if (result == null || !result.Success || result.Data == null)
            {
                _logger.LogWarning("MES API returned no data or failed for {StartTime} - {EndTime}", startStr, endStr);
                return;
            }

            _logger.LogInformation("Received {Count} records from MES API", result.Data.Count);

            foreach (var item in result.Data)
            {
                try
                {
                    var workorderNo = item.WorkorderNo ?? string.Empty;
                    var procedureNo = item.ProcedureNo ?? string.Empty;

                    if (string.IsNullOrEmpty(workorderNo) || string.IsNullOrEmpty(procedureNo))
                        continue;

                    var existing = await _workOrderRepository.GetByWorkorderAndProcedureAsync(workorderNo, procedureNo);

                    if (existing != null)
                    {
                        _logger.LogDebug("Skipping duplicate record for WorkOrder {WorkorderNo}, Procedure {ProcedureNo}", workorderNo, procedureNo);
                        continue;
                    }

                    var record = new WorkOrderRecord
                    {
                        WorkorderNo = workorderNo,
                        ProcedureNo = procedureNo,
                        Woid = item.Woid ?? 0,
                        Wtaid = item.Wtaid ?? 0,
                        MaterialNo = item.MaterialNo,
                        MaterialDesc = item.MaterialDesc,
                        ProcedureName = item.ProcedureName,
                        GoodQty = item.GoodQty ?? 0,
                        BadQty = item.BadQty ?? 0,
                        PlanQty = item.PlanQty ?? 0,
                        UserNames = item.UserNames,
                        MachineNo = item.MachineNo,
                        MachineName = item.MachineName,
                        TaskStartTime = item.TaskStartTime,
                        TaskEndTime = item.TaskEndTime,
                        ApproverTime = item.ApproverTime,
                        PrintStatus = PrintStatus.Pending,
                        RawData = JsonSerializer.Serialize(item, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }),
                        SyncTime = DateTime.UtcNow
                    };

                    await _workOrderRepository.UpsertAsync(record);
                    _logger.LogInformation("Saved WorkOrder {WorkorderNo}, Procedure {ProcedureName}", workorderNo, item.ProcedureName);

                    await _printTaskService.TriggerPrintJobAsync(record, context.CancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing MES record for WorkOrder {WorkorderNo}", item.WorkorderNo);
                }
            }

            _logger.LogInformation("MES data sync completed for {StartTime} - {EndTime}", startStr, endStr);
        }
    }
}
