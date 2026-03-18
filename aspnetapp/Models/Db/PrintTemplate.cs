using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace aspnetapp.Models.Db
{
    public class PrintTemplate
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("templateName")]
        public string TemplateName { get; set; } = string.Empty;

        [BsonElement("templateContent")]
        public string TemplateContent { get; set; } = string.Empty;

        [BsonElement("defaultPrintQty")]
        public int DefaultPrintQty { get; set; } = 1;

        [BsonElement("isDefault")]
        public bool IsDefault { get; set; }

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
