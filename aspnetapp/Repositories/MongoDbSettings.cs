namespace aspnetapp.Repositories
{
    public class MongoDbSettings
    {
        public string ConnectionString { get; set; } = "mongodb://localhost:27017";
        public string DatabaseName { get; set; } = "MesAutoPrintSystem";
        public string WorkOrdersCollectionName { get; set; } = "WorkOrders";
        public string TemplatesCollectionName { get; set; } = "Templates";
    }
}
