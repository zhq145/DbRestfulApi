using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Npgsql;
using System.Text.Json;

namespace DbRestfulApi.Services
{
    public class PostgreSqlService : IDatabaseService
    {
        private readonly string _connectionString;

        public PostgreSqlService(string connectionString)
        {
            _connectionString = connectionString;
        }

        private IDbConnection CreateConnection()
            => new NpgsqlConnection(_connectionString);

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

        private static Dictionary<string, object> CleanData(Dictionary<string, object> data)
        {
            var cleanData = new Dictionary<string, object>();
            foreach (var kv in data)
            {
                object value = kv.Value;
                if (value is JsonElement je)
                {
                    switch (je.ValueKind)
                    {
                        case JsonValueKind.String:
                            if (DateTime.TryParse(je.GetString(), out var dt))
                                value = dt;
                            else
                                value = je.GetString();
                            break;
                        case JsonValueKind.Number:
                            if (je.TryGetInt32(out int i)) value = i;
                            else if (je.TryGetDouble(out double d)) value = d;
                            else value = je.GetRawText();
                            break;
                        case JsonValueKind.True:
                        case JsonValueKind.False:
                            value = je.GetBoolean();
                            break;
                        case JsonValueKind.Null:
                            value = DBNull.Value;
                            break;
                        default:
                            value = je.GetRawText();
                            break;
                    }
                }
                cleanData[kv.Key] = value ?? DBNull.Value;
            }
            return cleanData;
        }

        public async Task<(IEnumerable<Dictionary<string, object>> Items, int Total)> ListAsync(string table, int page, int pageSize)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;
            var offset = (page - 1) * pageSize;

            var sqlItems = $@"
SELECT * FROM ""{table}""
ORDER BY ""ID""
OFFSET @Offset LIMIT @PageSize;

SELECT COUNT(1) FROM ""{table}"";
";

            using var conn = CreateConnection();
            using var multi = await conn.QueryMultipleAsync(sqlItems, new { Offset = offset, PageSize = pageSize });
            var rows = (await multi.ReadAsync()).Select(r => (IDictionary<string, object>)r).Select(NormalizeRow).ToList();
            var total = await multi.ReadFirstAsync<int>();
            return (rows, total);
        }

        public async Task<Dictionary<string, object>?> GetAsync(string table, int id)
        {
            var sql = $@"SELECT * FROM ""{table}"" WHERE ""ID"" = @Id";
            using var conn = CreateConnection();
            var row = (await conn.QueryFirstOrDefaultAsync(sql, new { Id = id })) as IDictionary<string, object>;
            return row == null ? null : NormalizeRow(row);
        }

        public async Task<int> AddAsync(string table, Dictionary<string, object> data)
        {
            if (data == null || data.Count == 0)
                throw new ArgumentException("data empty");

            var cleanData = CleanData(data);
            var columns = string.Join(",", cleanData.Keys.Select(k => $"\"{k}\""));
            var paramsList = string.Join(",", cleanData.Keys.Select(k => "@" + k));
            var sql = $@"INSERT INTO ""{table}"" ({columns}) VALUES ({paramsList}) RETURNING ""ID"";";

            using var conn = CreateConnection();
            var id = await conn.ExecuteScalarAsync<int>(sql, cleanData);
            return id;
        }

        public async Task<bool> UpdateAsync(string table, int id, Dictionary<string, object> data)
        {
            if (data == null || data.Count == 0) return false;
            var cleanData = CleanData(data);
            var sets = string.Join(",", cleanData.Keys.Select(k => $"\"{k}\"=@{k}"));
            var sql = $@"UPDATE ""{table}"" SET {sets} WHERE ""ID""=@Id";
            var parameters = new DynamicParameters(cleanData);
            parameters.Add("Id", id);
            using var conn = CreateConnection();
            var changed = await conn.ExecuteAsync(sql, parameters);
            return changed > 0;
        }

        public async Task<bool> DeleteAsync(string table, int id)
        {
            var sql = $@"DELETE FROM ""{table}"" WHERE ""ID""=@Id";
            using var conn = CreateConnection();
            var changed = await conn.ExecuteAsync(sql, new { Id = id });
            return changed > 0;
        }
    }
}
