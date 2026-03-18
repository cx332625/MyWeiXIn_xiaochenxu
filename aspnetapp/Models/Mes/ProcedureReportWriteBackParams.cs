namespace aspnetapp.Models.Mes
{
    public class ProcedureReportWriteBackParams
    {
        public decimal? GoodQty { get; set; }
        public decimal? BadQty { get; set; }
        public decimal? BadQtyManufacturing { get; set; }
        public decimal? BadQtyIncoming { get; set; }
        public long? Woid { get; set; }
        public string? WorkorderNo { get; set; }
        public string? WorkorderType { get; set; }
        public string? SalesorderNo { get; set; }
        public string? PlanFollowWordNo { get; set; }
        public string? MaterialNo { get; set; }
        public string? MaterialDesc { get; set; }
        public string? Unit { get; set; }
        public string? MaterialSpec { get; set; }
        public string? Warehouse { get; set; }
        public string? ProductionBatchNo { get; set; }
        public decimal? MaterialQty { get; set; }
        public decimal? Weight { get; set; }
        public string? MaterialUnit { get; set; }
        public decimal? KnifeWeight { get; set; }
        public decimal? MaterialWeight { get; set; }
        public decimal? PieceWeight { get; set; }
        public decimal? ScrapWeight { get; set; }
        public string? TaskActionRemark { get; set; }
        public decimal? InStockQty { get; set; }
        public string? TransferCardNo { get; set; }
        public long? Wtaid { get; set; }
        public long? Tid { get; set; }
        public string? ProcedureNo { get; set; }
        public int? ProcedureOrder { get; set; }
        public string? ProcedureName { get; set; }
        public decimal? ActualTime { get; set; }
        public decimal? EarnedHours { get; set; }
        public string? UserNames { get; set; }
        public string? EmployeeNos { get; set; }
        public string? MachineNo { get; set; }
        public string? MachineName { get; set; }
        public string? WorkcenterNo { get; set; }
        public string? WorkcenterName { get; set; }
        public string? WorkshopName { get; set; }
        public string? ProductLine { get; set; }
        public string? TaskStartTime { get; set; }
        public string? TaskEndTime { get; set; }
        public decimal? WorkorderPlanQty { get; set; }
        public string? Component { get; set; }
        public string? MaterialCode { get; set; }
        public string? MaterialBatchNo { get; set; }
        public string? ProcedureRemark { get; set; }
        public decimal? SingleTripQty { get; set; }
        public decimal? SingleTripTime { get; set; }
        public int? FrequencySize { get; set; }
        public string? OneLevelReason { get; set; }
        public string? TwoLevelReason { get; set; }
        public string? ThreeLevelReason { get; set; }
        public string? ApproverName { get; set; }
        public string? ApproverTime { get; set; }
        public string? WorkDate { get; set; }
        public string? ShiftName { get; set; }
        public string? MouldNo { get; set; }
        public string? MouldName { get; set; }
        public decimal? PlanQty { get; set; }
        public string? CreateTime { get; set; }
        public string? Note { get; set; }
        public int? CompletionStage { get; set; }
    }

    public class MesApiResult<T>
    {
        public bool Success { get; set; }
        public int Code { get; set; }
        public string? Message { get; set; }
        public T? Data { get; set; }
    }
}
