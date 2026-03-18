using aspnetapp.Models.Db;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace aspnetapp.Repositories
{
    public class WorkOrderRepository
    {
        private readonly IMongoCollection<WorkOrderRecord> _collection;

        public WorkOrderRepository(IMongoClient mongoClient, IOptions<MongoDbSettings> settings)
        {
            var database = mongoClient.GetDatabase(settings.Value.DatabaseName);
            _collection = database.GetCollection<WorkOrderRecord>(settings.Value.WorkOrdersCollectionName);

            CreateIndexes();
        }

        private void CreateIndexes()
        {
            var indexModels = new List<CreateIndexModel<WorkOrderRecord>>
            {
                new CreateIndexModel<WorkOrderRecord>(
                    Builders<WorkOrderRecord>.IndexKeys
                        .Ascending(x => x.WorkorderNo)
                        .Ascending(x => x.ProcedureNo),
                    new CreateIndexOptions { Name = "workorderNo_procedureNo" }),
                new CreateIndexModel<WorkOrderRecord>(
                    Builders<WorkOrderRecord>.IndexKeys
                        .Ascending(x => x.ApproverTime)
                        .Ascending(x => x.TaskEndTime),
                    new CreateIndexOptions { Name = "approverTime_taskEndTime" }),
                new CreateIndexModel<WorkOrderRecord>(
                    Builders<WorkOrderRecord>.IndexKeys
                        .Text(x => x.UserNames),
                    new CreateIndexOptions { Name = "userNames_text" })
            };

            try
            {
                _collection.Indexes.CreateMany(indexModels);
            }
            catch (Exception)
            {
                // Indexes already exist; no action needed
            }
        }

        public async Task<WorkOrderRecord?> GetByWorkorderAndProcedureAsync(string workorderNo, string procedureNo)
        {
            return await _collection.Find(x =>
                x.WorkorderNo == workorderNo && x.ProcedureNo == procedureNo)
                .FirstOrDefaultAsync();
        }

        public async Task<WorkOrderRecord?> GetByIdAsync(string id)
        {
            return await _collection.Find(x => x.Id == id).FirstOrDefaultAsync();
        }

        public async Task<List<WorkOrderRecord>> GetByTimeRangeAsync(DateTime startTime, DateTime endTime)
        {
            return await _collection.Find(x =>
                x.SyncTime >= startTime && x.SyncTime <= endTime)
                .ToListAsync();
        }

        public async Task<List<WorkOrderRecord>> GetPendingPrintJobsAsync()
        {
            return await _collection.Find(x =>
                x.PrintStatus == Models.Shared.PrintStatus.Pending)
                .ToListAsync();
        }

        public async Task UpsertAsync(WorkOrderRecord record)
        {
            var filter = Builders<WorkOrderRecord>.Filter.And(
                Builders<WorkOrderRecord>.Filter.Eq(x => x.WorkorderNo, record.WorkorderNo),
                Builders<WorkOrderRecord>.Filter.Eq(x => x.ProcedureNo, record.ProcedureNo)
            );

            var options = new ReplaceOptions { IsUpsert = true };
            await _collection.ReplaceOneAsync(filter, record, options);
        }

        public async Task UpdatePrintStatusAsync(string id, Models.Shared.PrintStatus status, int printedQty)
        {
            var update = Builders<WorkOrderRecord>.Update
                .Set(x => x.PrintStatus, status)
                .Set(x => x.PrintedTotalQty, printedQty);

            await _collection.UpdateOneAsync(x => x.Id == id, update);
        }

        public async Task AddPrintRecordAsync(string id, Models.Db.PrintRecord printRecord)
        {
            var update = Builders<WorkOrderRecord>.Update
                .Push(x => x.PrintRecords, printRecord);

            await _collection.UpdateOneAsync(x => x.Id == id, update);
        }
    }
}
