using System.Collections.Generic;

namespace DbRestfulApi.Models
{
    public class DatabaseConfig
    {
        public string? Type { get; set; }
        public string? ConnectionString { get; set; }
    }

    public class ModuleConfig
    {
        public string? Table { get; set; }
        public List<string>? AllowedFields { get; set; } = new();
        public List<string>? RequiredFields { get; set; } = new();
    }

    public class AppSettings
    {
        public string? CurrentDb { get; set; }
        public Dictionary<string, DatabaseConfig>? Databases { get; set; } = new();
        public Dictionary<string, ModuleConfig>? Modules { get; set; } = new();
    }
}
