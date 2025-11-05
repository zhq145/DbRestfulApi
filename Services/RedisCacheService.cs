using StackExchange.Redis;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace DbRestfulApi.Services
{
    public class RedisCacheService
    {
        private readonly ConnectionMultiplexer _redis;
        private readonly IDatabase _db;
        private readonly int _ttl;

        public RedisCacheService(IConfiguration config)
        {
            var redisConfig = config.GetSection("Redis");
            var host = redisConfig["Host"] ?? "localhost";
            var port = redisConfig["Port"] ?? "6379";
            var username = redisConfig["Username"];
            var password = redisConfig["Password"];
            _ttl = int.TryParse(redisConfig["DefaultTTLSeconds"], out var ttl) ? ttl : 300;

            var options = new ConfigurationOptions
            {
                EndPoints = { $"{host}:{port}" },
                AbortOnConnectFail = false,      // ⚡ 保证连接失败时不抛异常
                User = username,
                Password = password,
                ConnectRetry = 3,
                ConnectTimeout = 5000,
                AllowAdmin = false
            };

            try
            {
                _redis = ConnectionMultiplexer.Connect(options);
                _db = _redis.GetDatabase();
            }
            catch
            {
                // Redis 启动失败，不影响数据库访问
                _redis = null!;
                _db = null!;
            }
        }

        private bool RedisAvailable => _db != null && _redis != null && _redis.IsConnected;

        public async Task<T?> GetAsync<T>(string key)
        {
            if (!RedisAvailable) return default;

            try
            {
                var value = await _db.StringGetAsync(key);
                if (value.IsNullOrEmpty) return default;
                return JsonSerializer.Deserialize<T>(value!);
            }
            catch
            {
                // Redis 异常时忽略
                return default;
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
        {
            if (!RedisAvailable) return;

            try
            {
                var json = JsonSerializer.Serialize(value);
                await _db.StringSetAsync(key, json, expiry ?? TimeSpan.FromSeconds(_ttl));
            }
            catch
            {
                // Redis 异常时忽略
            }
        }

        public async Task RemoveAsync(string key)
        {
            if (!RedisAvailable) return;

            try
            {
                await _db.KeyDeleteAsync(key);
            }
            catch
            {
                // Redis 异常时忽略
            }
        }

        public async Task RemoveByPatternAsync(string pattern)
        {
            if (!RedisAvailable) return;

            try
            {
                foreach (var endpoint in _redis.GetEndPoints())
                {
                    var server = _redis.GetServer(endpoint);
                    if (!server.IsConnected) continue;

                    var keys = server.Keys(pattern: pattern).ToArray();
                    if (keys.Length > 0)
                        await _db.KeyDeleteAsync(keys);
                }
            }
            catch
            {
                // Redis 异常时忽略
            }
        }
    }
}
