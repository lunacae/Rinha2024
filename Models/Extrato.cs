using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.Text.Json.Serialization;

namespace Rinha2024.Models
{
    [JsonConverter(typeof(JsonStringEnumConverter<TipoTransacao>))]
    public enum TipoTransacao { Incorrect, c, d }

    public class Transacao
    {
        [BsonIgnoreIfDefault]
        public ObjectId _id { get; set; }
        public int Valor { get; set; }
        public TipoTransacao Tipo { get; set; }
        public string Descricao { get; set; }
        public DateTime RealizadoEm { get; set; }
        public int Id_Cliente { get; set; }
    }

    public class Extrato
    {
        public Saldo saldo { get; set; }
        public List<Transacao> UltimasTransacoes { get; set; }
    }

    public class Saldo
    {
        public int Total { get; set; }
        public DateTime DataExtrato { get; set; }
        public int Limite { get; set; }
    }
}
