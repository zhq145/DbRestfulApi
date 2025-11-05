using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Oracle.ManagedDataAccess.Client;
using System.Text.Json;

namespace DbRestfulApi.Services
{
    public class OracleService : IDatabaseService
    {
        private readonly string _connectionString;

        public OracleService(string connectionString)
        {
            _connectionString = connectionString;
        }

        private OracleConnection CreateConnection()
            => new OracleConnection(_connectionString);

        private static string ToCamel(string s)
        {
            if (string.IsNullOrEmpty(s)) return s;
            return char.ToLowerInvariant(s[0]) + s.Substring(1);
        }

        private static Dictionary<string, object> NormalizeRow(IDictionary<string, object> row)
        {
            var dict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            foreach (var kv in row)
            {
                var key = ToCamel(kv.Key);
                var value = kv.Value is DBNull ? null : kv.Value;
                dict[key] = value!;
            }
            return dict;
        }

        // ⚡ 统一 JsonElement 转换
        private static object NormalizeValue(object value)
        {
            if (value is JsonElement je)
            {
                return je.ValueKind switch
                {
                    JsonValueKind.String => DateTime.TryParse(je.GetString(), out var dt) ? dt : je.GetString() ?? "",
                    JsonValueKind.Number => je.TryGetInt32(out int i) ? i :
                                           je.TryGetInt64(out long l) ? l :
                                           je.TryGetDouble(out double d) ? d : (object)je.GetRawText(),
                    JsonValueKind.True => true,
                    JsonValueKind.False => false,
                    JsonValueKind.Null => DBNull.Value,
                    _ => je.GetRawText()
                };
            }
            return value ?? DBNull.Value;
        }

        private static string MapTableName(string table)
        {
            // Oracle 中表名大写
            if (table.Equals("ZGBao_About", StringComparison.OrdinalIgnoreCase))
                return "ZGBAO_ABOUT";
            return table.ToUpperInvariant();
        }

        // ✅ 分页查询
        public async Task<(IEnumerable<Dictionary<string, object>> Items, int Total)> ListAsync(string table, int page, int pageSize)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;
            var offset = (page - 1) * pageSize;

            table = MapTableName(table);

            var sqlItems = $@"
SELECT * 
FROM {table}
ORDER BY ID
OFFSET :Offset ROWS FETCH NEXT :PageSize ROWS ONLY";

            var sqlCount = $"SELECT COUNT(1) FROM {table}";

            using var conn = CreateConnection();
            await conn.OpenAsync();

            var itemsRaw = await conn.QueryAsync(sqlItems, new { Offset = offset, PageSize = pageSize });
            var rows = itemsRaw.Select(r => (IDictionary<string, object>)r).Select(NormalizeRow).ToList();

            var total = await conn.ExecuteScalarAsync<int>(sqlCount);

            return (rows, total);
        }

        // ✅ 查询单条
        public async Task<Dictionary<string, object>?> GetAsync(string table, int id)
        {
            table = MapTableName(table);
            var sql = $"SELECT * FROM {table} WHERE ID = :Id";

            using var conn = CreateConnection();
            await conn.OpenAsync();

            var row = (await conn.QueryFirstOrDefaultAsync(sql, new { Id = id })) as IDictionary<string, object>;
            return row == null ? null : NormalizeRow(row);
        }

        // ✅ 新增
        public async Task<int> AddAsync(string table, Dictionary<string, object> data)
        {
            if (data == null || data.Count == 0) throw new ArgumentException("data empty");

            table = MapTableName(table);

            var cleanData = data.ToDictionary(kv => kv.Key, kv => NormalizeValue(kv.Value));

            var columns = string.Join(",", cleanData.Keys.Select(k => k));
            var paramsList = string.Join(",", cleanData.Keys.Select(k => ":" + k));

            var sql = $@"INSERT INTO {table} ({columns}) VALUES ({paramsList}) RETURNING ID INTO :NewId";

            using var conn = CreateConnection();
            await conn.OpenAsync();

            var parameters = new DynamicParameters(cleanData);
            parameters.Add("NewId", dbType: DbType.Int32, direction: ParameterDirection.Output);

            await conn.ExecuteAsync(sql, parameters);

            return parameters.Get<int>("NewId");
        }

        // ✅ 更新
        public async Task<bool> UpdateAsync(string table, int id, Dictionary<string, object> data)
        {
            if (data == null || data.Count == 0) return false;

            table = MapTableName(table);

            var cleanData = data.ToDictionary(kv => kv.Key, kv => NormalizeValue(kv.Value));
            var sets = string.Join(",", cleanData.Keys.Select(k => $"{k} = :{k}"));
            var sql = $"UPDATE {table} SET {sets} WHERE ID = :Id";

            using var conn = CreateConnection();
            await conn.OpenAsync();

            var parameters = new DynamicParameters(cleanData);
            parameters.Add("Id", id);

            var changed = await conn.ExecuteAsync(sql, parameters);
            return changed > 0;
        }

        // ✅ 删除
        public async Task<bool> DeleteAsync(string table, int id)
        {
            table = MapTableName(table);
            var sql = $"DELETE FROM {table} WHERE ID = :Id";

            using var conn = CreateConnection();
            await conn.OpenAsync();

            var changed = await conn.ExecuteAsync(sql, new { Id = id });
            return changed > 0;
        }
    }
}
