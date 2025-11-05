using Microsoft.AspNetCore.Http;
using System.Text.Json;
using System.Security.Cryptography;
using System.Text;

namespace DbRestfulApi.Services
{
    public class CachedDatabaseService : IDatabaseService
    {
        private readonly IDatabaseService _inner;
        private readonly RedisCacheService _cache;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CachedDatabaseService(IDatabaseService inner, RedisCacheService cache, IHttpContextAccessor httpContextAccessor)
        {
            _inner = inner;
            _cache = cache;
            _httpContextAccessor = httpContextAccessor;
        }

        private string GetTenantKeyPrefix()
        {
            var domain = _httpContextAccessor.HttpContext?.Request?.Host.Host ?? "default";
            return domain.ToLowerInvariant();
        }

        private string BuildListKey(string table, int page, int pageSize)
        {
            var query = _httpContextAccessor.HttpContext?.Request?.QueryString.Value ?? "";
            var hash = ComputeSha1Hash(query);
            return $"{GetTenantKeyPrefix()}:list:{table}:p{page}_s{pageSize}_{hash}";
        }

        private string BuildItemKey(string table, int id)
            => $"{GetTenantKeyPrefix()}:item:{table}:{id}";

        private static string ComputeSha1Hash(string input)
        {
            using var sha1 = SHA1.Create();
            var bytes = sha1.ComputeHash(Encoding.UTF8.GetBytes(input ?? ""));
            return BitConverter.ToString(bytes).Replace("-", "").Substring(0, 8).ToLower();
        }

        private class PagedResult
        {
            public IEnumerable<Dictionary<string, object>>? Items { get; set; }
            public int Total { get; set; }
        }

        public async Task<(IEnumerable<Dictionary<string, object>> Items, int Total)> ListAsync(string table, int page, int pageSize)
        {
            var cacheKey = BuildListKey(table, page, pageSize);

            // ✅ 尝试读取 Redis 缓存
            try
            {
                var cached = await _cache.GetAsync<PagedResult>(cacheKey);
                if (cached?.Items != null && cached.Items.Any())
                    return (cached.Items, cached.Total);
            }
            catch
            {
                // Redis 异常时忽略
            }

            // 访问数据库
            var data = await _inner.ListAsync(table, page, pageSize);

            // ✅ 尝试写入 Redis 缓存
            try
            {
                var wrapper = new PagedResult { Items = data.Items, Total = data.Total };
                await _cache.SetAsync(cacheKey, wrapper);
            }
            catch
            {
                // Redis 异常时忽略
            }

            return data;
        }

        public async Task<Dictionary<string, object>?> GetAsync(string table, int id)
        {
            var cacheKey = BuildItemKey(table, id);

            try
            {
                var cached = await _cache.GetAsync<Dictionary<string, object>>(cacheKey);
                if (cached != null)
                    return cached;
            }
            catch { }

            var item = await _inner.GetAsync(table, id);

            try
            {
                if (item != null)
                    await _cache.SetAsync(cacheKey, item);
            }
            catch { }

            return item;
        }

        public async Task<int> AddAsync(string table, Dictionary<string, object> data)
        {
            var id = await _inner.AddAsync(table, data);

            try
            {
                // 新增后清空列表缓存
                await _cache.RemoveByPatternAsync($"{GetTenantKeyPrefix()}:list:{table}:*");
            }
            catch { }

            return id;
        }

        public async Task<bool> UpdateAsync(string table, int id, Dictionary<string, object> data)
        {
            var result = await _inner.UpdateAsync(table, id, data);
            if (result)
            {
                try { await _cache.RemoveAsync(BuildItemKey(table, id)); } catch { }
                try { await _cache.RemoveByPatternAsync($"{GetTenantKeyPrefix()}:list:{table}:*"); } catch { }
            }
            return result;
        }

        public async Task<bool> DeleteAsync(string table, int id)
        {
            var result = await _inner.DeleteAsync(table, id);
            if (result)
            {
                try { await _cache.RemoveAsync(BuildItemKey(table, id)); } catch { }
                try { await _cache.RemoveByPatternAsync($"{GetTenantKeyPrefix()}:list:{table}:*"); } catch { }
            }
            return result;
        }
    }
}
