using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Rinha2024.Models
{
    public class Cliente
    {
        [BsonIgnoreIfDefault]
        public ObjectId _id { get; set; }
        public int Id_Cliente { get; set; }

        public int Limite { get; set; }
        public int Saldo { get; set; }
    }
}
