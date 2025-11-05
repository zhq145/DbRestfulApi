using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DbRestfulApi.Services;
using MongoDB.Bson;
using MongoDB.Driver;

namespace DbRestfulApi.Services
{
    public class MongoService : IDatabaseService
    {
        private readonly IMongoDatabase _database;

        public MongoService(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("MongoDB connection string cannot be null or empty.");

            var mongoUrl = new MongoUrl(connectionString);
            var client = new MongoClient(mongoUrl);
            _database = client.GetDatabase(mongoUrl.DatabaseName);
        }

        private IMongoCollection<BsonDocument> GetCollection(string name)
            => _database.GetCollection<BsonDocument>(name);

        // 列表
        public async Task<(IEnumerable<Dictionary<string, object>> Items, int Total)> ListAsync(string table, int page, int pageSize)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;

            var skip = (page - 1) * pageSize;
            var collection = GetCollection(table);

            var total = (int)await collection.CountDocumentsAsync(FilterDefinition<BsonDocument>.Empty);
            var docs = await collection.Find(FilterDefinition<BsonDocument>.Empty)
                                       .Skip(skip)
                                       .Limit(pageSize)
                                       .ToListAsync();

            var list = docs.Select(ConvertBsonToDict);
            return (list, total);
        }

        // 获取单条
        public async Task<Dictionary<string, object>?> GetAsync(string table, int id)
        {
            var collection = GetCollection(table);
            var filter = Builders<BsonDocument>.Filter.Eq("ID", id);
            var doc = await collection.Find(filter).FirstOrDefaultAsync();
            return doc == null ? null : ConvertBsonToDict(doc);
        }

        // 新增
        public async Task<int> AddAsync(string table, Dictionary<string, object> data)
        {
            if (data == null || data.Count == 0)
                throw new ArgumentException("data empty");

            var collection = GetCollection(table);

            // 自动分配ID（最大ID+1）
            var last = await collection.Find(FilterDefinition<BsonDocument>.Empty)
                                       .Sort(Builders<BsonDocument>.Sort.Descending("ID"))
                                       .Limit(1)
                                       .FirstOrDefaultAsync();
            int nextId = last != null && last.Contains("ID") ? last["ID"].AsInt32 + 1 : 1;
            data["ID"] = nextId;

            var doc = new BsonDocument(data.Where(kv => kv.Value != null)
                                           .ToDictionary(k => k.Key, v => BsonValue.Create(v.Value)));
            await collection.InsertOneAsync(doc);
            return nextId;
        }

        // 更新
        public async Task<bool> UpdateAsync(string table, int id, Dictionary<string, object> data)
        {
            if (data == null || data.Count == 0)
                return false;

            var collection = GetCollection(table);
            var filter = Builders<BsonDocument>.Filter.Eq("ID", id);

            var update = new BsonDocument("$set", new BsonDocument(
                data.Where(kv => kv.Value != null)
                    .ToDictionary(k => k.Key, v => BsonValue.Create(v.Value))
            ));

            var result = await collection.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }

        // 删除
        public async Task<bool> DeleteAsync(string table, int id)
        {
            var collection = GetCollection(table);
            var result = await collection.DeleteOneAsync(Builders<BsonDocument>.Filter.Eq("ID", id));
            return result.DeletedCount > 0;
        }

        // 工具函数：BsonDocument → Dictionary
        private static Dictionary<string, object> ConvertBsonToDict(BsonDocument doc)
        {
            var dict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            foreach (var el in doc.Elements)
            {
                if (el.Value.IsBsonNull) dict[el.Name] = null!;
                else if (el.Value.IsInt32) dict[el.Name] = el.Value.AsInt32;
                else if (el.Value.IsInt64) dict[el.Name] = el.Value.AsInt64;
                else if (el.Value.IsDouble) dict[el.Name] = el.Value.AsDouble;
                else if (el.Value.IsBoolean) dict[el.Name] = el.Value.AsBoolean;
                else if (el.Value.IsString) dict[el.Name] = el.Value.AsString;
                else if (el.Value.IsBsonDateTime) dict[el.Name] = el.Value.ToUniversalTime();
                else dict[el.Name] = el.Value.ToString();
            }
            return dict;
        }
    }
}
