using System.Linq.Expressions;
using MongoDB.Bson;
using MongoDB.Driver;
using web_server.Models.Tables;

namespace web_server.Managers;

public static class DatabaseManipulator
{
    private static IConfiguration? config;
    private static string? databaseName;
    private static string? host;
    private static MongoServerAddress? address;
    private static MongoClientSettings? settings;
    private static MongoClient? client;
    private static IMongoDatabase? database;

    public static void Initialize(IConfiguration configuration)
    {
        config = configuration;
        IConfigurationSection sections = config.GetSection("ConnectionStrings");
        databaseName = sections.GetValue<string>("DatabaseName");
        host = sections.GetValue<string>("MongoConnection");
        address = new MongoServerAddress(host, 27017);
        settings = new MongoClientSettings() { Server = address };
        client = new MongoClient(settings);
        database = client.GetDatabase(databaseName);
    }
    public static async Task Save<T>(T saveableObject)
    {
        var table = database.GetCollection<T>(saveableObject.GetType().Name);
        try
        {
            await table.InsertOneAsync(saveableObject);

        }
        catch
        {
            Console.WriteLine("Error saving object");

        }

    }
    public static async Task Update<T>(T saveableObject, Expression<Func<T, bool>> filter)
    {
        var table = database.GetCollection<T>(saveableObject.GetType().Name);
        try
        {
            await table.ReplaceOneAsync(filter, saveableObject);

        }
        catch
        {

        }

    }

    public static async Task SaveMany<T>(List<T> items)
    {
        var table = database.GetCollection<T>(typeof(T).Name);

        try
        {
            await table.InsertManyAsync(items);
        }
        catch
        {

        }
    }
    public static async Task<T> GetSingle<T>(Expression<Func<T, bool>> filter)
    {
        var table = database.GetCollection<T>(typeof(T).Name);
        try
        {
            return await table.Find(filter).FirstOrDefaultAsync();
        }
        catch
        {
            Console.WriteLine("Error retrieving object");
            return default(T);
        }
    }
    public static async Task<List<T>> GetMany<T>(Expression<Func<T, bool>> filter)
    {

        var table = database.GetCollection<T>(typeof(T).Name);
        try
        {
            List<T> list = await table.Find(filter).ToListAsync();
            return list;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error retrieving objects: {ex.Message}");
        }
        return [];

    }

    public static async Task<List<T>> GetPaged<T>(
        Expression<Func<T, bool>> filter,
        int page,
        int pageSize,
        Expression<Func<T, object>>? sortField = null,
        bool ascending = true) where T : SaveableObject
    {
        var table = database.GetCollection<T>(typeof(T).Name);
        var query = table.Find(filter);

        if (sortField != null)
        {
            query = ascending
                ? query.SortBy(sortField)
                : query.SortByDescending(sortField);
        }

        return await query
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();
    }
    public static async Task DeleteOne<T>(Expression<Func<T, bool>> filter)
    {
        var table = database.GetCollection<T>(typeof(T).Name);
        try
        {
            await table.DeleteOneAsync(filter);
        }
        catch
        {
            Console.WriteLine("Error retrieving objects");
        }

    }

    public class MonthlyExpense
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int Total { get; set; }
    }
}
