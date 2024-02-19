using Microsoft.AspNetCore.Http.HttpResults;
using MongoDB.Bson;
using MongoDB.Driver;
//using MongoDB.Driver.Core.Configuration;
using Rinha2024.Models;

// Create a MongoClientSettings object
var settings = new MongoClientSettings()
{
    Server = new MongoServerAddress(Environment.GetEnvironmentVariable("MONGO_HOST", EnvironmentVariableTarget.Process), 27017),
    ConnectTimeout = new TimeSpan(0, 0, 60),
    MaxConnecting = 5,
    MaxConnectionPoolSize = 160,
    MinConnectionPoolSize = 10,
    SocketTimeout = new TimeSpan(0, 0, 3)
};

//var mongoClient = new MongoClient($"mongodb://{Environment.GetEnvironmentVariable("MONGO_HOST", EnvironmentVariableTarget.Process)}:27017");

//var mongoClient = new MongoClient($"mongodb://localhost:27017");

var mongoClient = new MongoClient(settings);

var builder = WebApplication.CreateBuilder(args);

var adm = mongoClient.GetDatabase("admin");
var command = new BsonDocument { { "setParameter", 1 }, { "diagnosticDataCollectionEnabled", 0 } };
BsonDocument parameter = adm.RunCommand<BsonDocument>(command);

var mongoDb = mongoClient.GetDatabase("Rinha");

builder.Services.AddSingleton<IMongoClient>(_ => mongoClient);
builder.Services.AddSingleton<IMongoDatabase>(_ => mongoDb);

//builder.Logging.ClearProviders().AddConsole().SetMinimumLevel(LogLevel.Information);
var app = builder.Build();

var transacaoCollection = mongoDb.GetCollection<Transacao>("transacao");
var clienteCollection = mongoDb.GetCollection<Cliente>("cliente");

var indexKeysDefinitionTransacao = Builders<Transacao>.IndexKeys.Descending(p => p.Id_Cliente);
await transacaoCollection.Indexes.CreateOneAsync(new CreateIndexModel<Transacao>(indexKeysDefinitionTransacao));
var indexKeysDefinitionCliente = Builders<Cliente>.IndexKeys.Descending(p => p.Id_Cliente);
await clienteCollection.Indexes.CreateOneAsync(new CreateIndexModel<Cliente>(indexKeysDefinitionCliente));

app.Logger.LogInformation("Adding Routes");

//app.MapGet("/", () => "Hello World!");

app.MapGet("/clientes/{idCliente}/extrato", async Task<Results<Ok<Extrato>, NotFound, StatusCodeHttpResult>> (int idCliente, IMongoClient mc, IMongoDatabase db, CancellationToken cancellationToken) =>
{
    try
    {
        var clienteCollection = db.GetCollection<Cliente>("cliente");
        var transacaoCollection = db.GetCollection<Transacao>("transacao");

        var cliente = await clienteCollection.FindAsync(a => a.Id_Cliente == idCliente, new FindOptions<Cliente, Cliente>()
        {
            Limit = 1
        });

        var clienteDocument = await cliente.FirstOrDefaultAsync();

        if (clienteDocument == null)
            return TypedResults.StatusCode(404);

        var transacao = await transacaoCollection.FindAsync(a => a.Id_Cliente == idCliente, new FindOptions<Transacao, Transacao>()
        {
            Limit = 10
        });

        Extrato ex = new Extrato();
        ex.saldo = new Saldo();
        ex.saldo.Limite = clienteDocument.Limite;
        ex.saldo.Total = clienteDocument.Saldo;
        ex.saldo.DataExtrato = DateTime.UtcNow.AddHours(-3);

        ex.UltimasTransacoes = await transacao.ToListAsync();

        return TypedResults.Ok(ex);
    }
    catch (OperationCanceledException)
    {
        return TypedResults.StatusCode(408);
    }catch(Exception e)
    {
        //logger.LogError("---------------------------------------------------------------------------------\nErro 422 in GET - Exception - " + e.Message + "\nTrace = " + e.StackTrace);
        return TypedResults.StatusCode(422);
    }
});

app.MapPost("/clientes/{idCliente}/transacoes", async Task<Results<Ok<TransacaoPost>, NotFound, UnprocessableEntity, StatusCodeHttpResult>>
    ( int idCliente, Transacao transacao, IMongoClient mc, IMongoDatabase db, CancellationToken cancellationToken) =>
{
    try
    {
        transacao.Id_Cliente = idCliente;
        transacao.RealizadoEm = DateTime.UtcNow.AddHours(-3);
        var clienteCollection = db.GetCollection<Cliente>("cliente");
        var transacaoCollection = db.GetCollection<Transacao>("transacao");
        var cliente = await clienteCollection.FindAsync(a => a.Id_Cliente == idCliente, new FindOptions<Cliente, Cliente>()
        {
            Limit = 1
        });
        var clienteDocument = cliente.FirstOrDefault();

        if (clienteDocument == null)
            return TypedResults.NotFound();

        var transacaoReturn = new TransacaoPost();
        if (transacao.Tipo == TipoTransacao.d)
        {
            if (clienteDocument.Saldo - transacao.Valor < -clienteDocument.Limite)
            {
                //logger.LogError("Erro 422 - Body - " + transacao.ToString() + "\n cliente object - " + clienteDocument.ToString());
                return TypedResults.UnprocessableEntity();
            }
                

            var update = Builders<Cliente>.Update.Set(x => x.Saldo, clienteDocument.Saldo - transacao.Valor);
            FilterDefinition<Cliente> filter = new BsonDocument("Id_Cliente", idCliente);
            var opts = new FindOneAndUpdateOptions<Cliente>()
            {
                ReturnDocument = ReturnDocument.After
            };

            var newCliente = await clienteCollection.FindOneAndUpdateAsync(filter, update, opts);
            await transacaoCollection.InsertOneAsync(transacao);
            transacaoReturn.Saldo = newCliente.Saldo;
            transacaoReturn.Limite = newCliente.Limite;

            return TypedResults.Ok(transacaoReturn);
        }
        await transacaoCollection.InsertOneAsync(transacao);
        transacaoReturn.Saldo = clienteDocument.Saldo;
        transacaoReturn.Limite = clienteDocument.Limite;

        return TypedResults.Ok(transacaoReturn);
    }
    catch(Exception e)
    {
        //logger.LogError("---------------------------------------------------------------------------------\nErro 422 in POST - Exception - " + e.Message + "\nTrace = " + e.StackTrace + "\n body da requisição = " + System.Text.Json.JsonSerializer.Serialize(transacao));
        return TypedResults.StatusCode(422);
    }
});

app.Run();
