using aspnetapp.Models.Db;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace aspnetapp.Repositories
{
    public class PrintTemplateRepository
    {
        private readonly IMongoCollection<PrintTemplate> _collection;

        public PrintTemplateRepository(IMongoClient mongoClient, IOptions<MongoDbSettings> settings)
        {
            var database = mongoClient.GetDatabase(settings.Value.DatabaseName);
            _collection = database.GetCollection<PrintTemplate>(settings.Value.TemplatesCollectionName);
        }

        public async Task<List<PrintTemplate>> GetAllAsync()
        {
            return await _collection.Find(_ => true).ToListAsync();
        }

        public async Task<PrintTemplate?> GetDefaultAsync()
        {
            return await _collection.Find(x => x.IsDefault).FirstOrDefaultAsync();
        }

        public async Task<PrintTemplate?> GetByIdAsync(string id)
        {
            return await _collection.Find(x => x.Id == id).FirstOrDefaultAsync();
        }

        public async Task<string> CreateAsync(PrintTemplate template)
        {
            await _collection.InsertOneAsync(template);
            return template.Id!;
        }

        public async Task UpdateAsync(PrintTemplate template)
        {
            template.UpdatedAt = DateTime.UtcNow;
            await _collection.ReplaceOneAsync(x => x.Id == template.Id, template);
        }

        public async Task DeleteAsync(string id)
        {
            await _collection.DeleteOneAsync(x => x.Id == id);
        }
    }
}
