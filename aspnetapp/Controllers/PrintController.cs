using aspnetapp.Models.Db;
using aspnetapp.Models.Mes;
using aspnetapp.Repositories;
using aspnetapp.Services;
using Microsoft.AspNetCore.Mvc;

namespace aspnetapp.Controllers
{
    [Route("api/print")]
    [ApiController]
    public class PrintController : ControllerBase
    {
        private readonly WorkOrderRepository _workOrderRepository;
        private readonly PrintTemplateRepository _templateRepository;
        private readonly PrintTaskService _printTaskService;
        private readonly MesApiClient _mesApiClient;
        private readonly ILogger<PrintController> _logger;

        public PrintController(
            WorkOrderRepository workOrderRepository,
            PrintTemplateRepository templateRepository,
            PrintTaskService printTaskService,
            MesApiClient mesApiClient,
            ILogger<PrintController> logger)
        {
            _workOrderRepository = workOrderRepository;
            _templateRepository = templateRepository;
            _printTaskService = printTaskService;
            _mesApiClient = mesApiClient;
            _logger = logger;
        }

        [HttpGet("workorders")]
        public async Task<IActionResult> GetWorkOrders([FromQuery] DateTime? startTime, [FromQuery] DateTime? endTime)
        {
            var start = startTime ?? DateTime.UtcNow.AddHours(-24);
            var end = endTime ?? DateTime.UtcNow;
            var records = await _workOrderRepository.GetByTimeRangeAsync(start, end);
            return Ok(new { success = true, data = records });
        }

        [HttpGet("workorders/pending")]
        public async Task<IActionResult> GetPendingPrintJobs()
        {
            var records = await _workOrderRepository.GetPendingPrintJobsAsync();
            return Ok(new { success = true, data = records });
        }

        [HttpPost("workorders/{id}/reprint")]
        public async Task<IActionResult> ReprintWorkOrder(string id)
        {
            var record = await _workOrderRepository.GetByIdAsync(id);
            if (record == null)
                return NotFound(new { success = false, message = "WorkOrder not found" });

            // Reset to Pending so TriggerPrintJobAsync won't skip it
            await _workOrderRepository.UpdatePrintStatusAsync(id, Models.Shared.PrintStatus.Pending, record.PrintedTotalQty);
            record.PrintStatus = Models.Shared.PrintStatus.Pending;

            await _printTaskService.TriggerPrintJobAsync(record);
            return Ok(new { success = true, message = "Reprint job dispatched" });
        }

        [HttpPost("result")]
        public async Task<IActionResult> ReportPrintResult([FromBody] PrintResultRequest request)
        {
            if (string.IsNullOrEmpty(request.WorkOrderId))
                return BadRequest(new { success = false, message = "WorkOrderId is required" });

            await _printTaskService.UpdatePrintResultAsync(
                request.WorkOrderId,
                request.PrintQty,
                request.DeviceId,
                request.OperatorName);

            return Ok(new { success = true, message = "Print result recorded" });
        }

        [HttpGet("templates")]
        public async Task<IActionResult> GetTemplates()
        {
            var templates = await _templateRepository.GetAllAsync();
            return Ok(new { success = true, data = templates });
        }

        [HttpPost("templates")]
        public async Task<IActionResult> CreateTemplate([FromBody] PrintTemplate template)
        {
            var id = await _templateRepository.CreateAsync(template);
            return Ok(new { success = true, id });
        }

        [HttpPut("templates/{id}")]
        public async Task<IActionResult> UpdateTemplate(string id, [FromBody] PrintTemplate template)
        {
            var existing = await _templateRepository.GetByIdAsync(id);
            if (existing == null)
                return NotFound(new { success = false, message = "Template not found" });

            template.Id = id;
            await _templateRepository.UpdateAsync(template);
            return Ok(new { success = true, message = "Template updated" });
        }

        [HttpDelete("templates/{id}")]
        public async Task<IActionResult> DeleteTemplate(string id)
        {
            var existing = await _templateRepository.GetByIdAsync(id);
            if (existing == null)
                return NotFound(new { success = false, message = "Template not found" });

            await _templateRepository.DeleteAsync(id);
            return Ok(new { success = true, message = "Template deleted" });
        }

        [HttpPost("sync")]
        public async Task<IActionResult> ManualSync([FromBody] SelectByDataTimeDTO request)
        {
            if (string.IsNullOrEmpty(request.StartTime) || string.IsNullOrEmpty(request.EndTime))
                return BadRequest(new { success = false, message = "StartTime and EndTime are required" });

            _logger.LogInformation("Manual sync triggered for {StartTime} - {EndTime}", request.StartTime, request.EndTime);

            var result = await _mesApiClient.QueryProcedureReportByTimeAsync(request.StartTime, request.EndTime);
            if (result == null || !result.Success)
                return StatusCode(502, new { success = false, message = "MES API call failed" });

            return Ok(new { success = true, count = result.Data?.Count ?? 0, data = result.Data });
        }
    }

    public class PrintResultRequest
    {
        public string WorkOrderId { get; set; } = string.Empty;
        public int PrintQty { get; set; }
        public string? DeviceId { get; set; }
        public string? OperatorName { get; set; }
    }
}
