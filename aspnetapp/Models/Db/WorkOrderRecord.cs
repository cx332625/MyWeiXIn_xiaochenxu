using aspnetapp.Models.Shared;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace aspnetapp.Models.Db
{
    public class WorkOrderRecord
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("workorderNo")]
        public string WorkorderNo { get; set; } = string.Empty;

        [BsonElement("procedureNo")]
        public string ProcedureNo { get; set; } = string.Empty;

        [BsonElement("woid")]
        public long Woid { get; set; }

        [BsonElement("wtaid")]
        public long Wtaid { get; set; }

        [BsonElement("materialNo")]
        public string? MaterialNo { get; set; }

        [BsonElement("materialDesc")]
        public string? MaterialDesc { get; set; }

        [BsonElement("procedureName")]
        public string? ProcedureName { get; set; }

        [BsonElement("goodQty")]
        public decimal GoodQty { get; set; }

        [BsonElement("badQty")]
        public decimal BadQty { get; set; }

        [BsonElement("planQty")]
        public decimal PlanQty { get; set; }

        [BsonElement("userNames")]
        public string? UserNames { get; set; }

        [BsonElement("machineNo")]
        public string? MachineNo { get; set; }

        [BsonElement("machineName")]
        public string? MachineName { get; set; }

        [BsonElement("taskStartTime")]
        public string? TaskStartTime { get; set; }

        [BsonElement("taskEndTime")]
        public string? TaskEndTime { get; set; }

        [BsonElement("approverTime")]
        public string? ApproverTime { get; set; }

        [BsonElement("printStatus")]
        public PrintStatus PrintStatus { get; set; } = PrintStatus.Pending;

        [BsonElement("printedTotalQty")]
        public int PrintedTotalQty { get; set; }

        [BsonElement("printRecords")]
        public List<PrintRecord> PrintRecords { get; set; } = new();

        [BsonElement("rawData")]
        public string? RawData { get; set; }

        [BsonElement("syncTime")]
        public DateTime SyncTime { get; set; } = DateTime.UtcNow;
    }

    public class PrintRecord
    {
        [BsonElement("printTime")]
        public DateTime PrintTime { get; set; }

        [BsonElement("printQty")]
        public int PrintQty { get; set; }

        [BsonElement("operatorName")]
        public string? OperatorName { get; set; }

        [BsonElement("deviceId")]
        public string? DeviceId { get; set; }

        [BsonElement("status")]
        public PrintStatus Status { get; set; }
    }
}
