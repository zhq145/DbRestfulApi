using DbRestfulApi.Models;
using DbRestfulApi.Services;
using Microsoft.OpenApi.Models;

// ✅ 新增：引入 Scrutor（用于 Decorate 装饰器注册）
using Scrutor;

var builder = WebApplication.CreateBuilder(args);

// load appsettings with reloadOnChange
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

// bind to AppSettings
var appSettings = new AppSettings();
builder.Configuration.Bind(appSettings);
builder.Services.Configure<AppSettings>(builder.Configuration);

// controllers + JSON options (camelCase)
builder.Services.AddControllers().AddJsonOptions(opts =>
{
    opts.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    opts.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
});

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "DbRestfulApi", Version = "v1" });
});

// ✅ 【新增】注册 Redis 支持与 HttpContext
builder.Services.AddSingleton<RedisCacheService>();
builder.Services.AddHttpContextAccessor();


// DI: choose DB implementation based on CurrentDb
var current = appSettings.CurrentDb ?? "SqlServer";

if (appSettings.Databases != null && appSettings.Databases.TryGetValue(current, out var dbConfig) && dbConfig != null)
{
    var type = dbConfig.Type ?? current;

    switch (type.Trim().ToLowerInvariant())
    {
        case "sqlserver":
            builder.Services.AddSingleton<IDatabaseService>(
                new SqlServerService(dbConfig.ConnectionString ?? "")
            );
            break;

        case "mysql":
            builder.Services.AddSingleton<IDatabaseService>(
                new MySqlService(dbConfig.ConnectionString ?? "")
            );
            break;
        case "mariadb":
            builder.Services.AddSingleton<IDatabaseService>(
                new MariaDbService(dbConfig.ConnectionString ?? "")
            );
            break;
        case "mongodb":
        case "mongo":
            builder.Services.AddSingleton<IDatabaseService>(
                new MongoService(dbConfig.ConnectionString ?? "")
            );
            break;

        // ✅ 预留扩展位置（未来只需添加新 case）
        case "postgresql":
            builder.Services.AddSingleton<IDatabaseService>(
                new PostgreSqlService(dbConfig.ConnectionString ?? "")
            );
            break;

        case "oracle":
            builder.Services.AddSingleton<IDatabaseService>(
                new OracleService(dbConfig.ConnectionString ?? "")
            );
            break;

        case "polardb":
            builder.Services.AddSingleton<IDatabaseService>(
                new PolarDbService(dbConfig.ConnectionString ?? "")
            );
            break;

        default:
            // fallback
            builder.Services.AddSingleton<IDatabaseService>(
                new SqlServerService(dbConfig.ConnectionString ?? "")
            );
            break;
    }
}
else
{
    // fallback to default local (for dev convenience)
    builder.Services.AddSingleton<IDatabaseService>(
        new SqlServerService(builder.Configuration.GetConnectionString("DefaultConnection") ?? "")
    );
}

// ✅ 【新增】在数据库服务注册之后，追加缓存装饰器注册
// 这一行让 CachedDatabaseService 自动包裹原有数据库实现
builder.Services.Decorate<IDatabaseService, CachedDatabaseService>();


var app = builder.Build();

// Swagger and middleware
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseAuthorization();
app.MapControllers();

app.Run();
