using System.Text.Json.Serialization;

namespace DbRestfulApi.Models
{
    public class ApiResponse<T>
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("data")]
        public T? Data { get; set; }

        [JsonPropertyName("timestamp")]
        public long Timestamp { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }

    // Pagination wrapper for data
    public class PagedResult
    {
        public int Total { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public object? Items { get; set; }
    }
}
