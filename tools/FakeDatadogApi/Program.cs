using FakeDatadogApi;
using Serilog;
using System.Text;
using System.Text.Json;


var builder = WebApplication.CreateBuilder(args);
var configuration = new ConfigurationBuilder()
  .AddJsonFile("appsettings.json")
  .Build();

builder.Host.UseSerilog((ctx, services, cfg) =>
{
    cfg.ReadFrom.Configuration(ctx.Configuration)
       .ReadFrom.Services(services)
       .Enrich.FromLogContext();
});
builder.Services.AddHostedService<FakeLogBackgroundService>();


var app = builder.Build();

// Config via env  
int totalLogs = int.TryParse(Environment.GetEnvironmentVariable("FAKE_TOTAL_LOGS"), out var t) ? t : 2500;
int defaultPageLimit = int.TryParse(Environment.GetEnvironmentVariable("FAKE_DEFAULT_LIMIT"), out var l) ? l : 1000;
string serviceA = Environment.GetEnvironmentVariable("FAKE_SERVICE_A") ?? "web";
string serviceB = Environment.GetEnvironmentVariable("FAKE_SERVICE_B") ?? "api";

// In-memory log generation  
var logs = GenerateLogs(totalLogs, new[] { serviceA, serviceB }).ToList();

// health  
app.MapGet("/health", () => Results.Ok(new { status = "ok", total = logs.Count }));

// POST /api/v2/logs/events/search  
// Body expected to include: filter.query, filter.from, filter.to, page.limit, page.cursor, sort  
app.MapPost("/api/v2/logs/events/search", async (HttpContext ctx) =>
{
    var json = await JsonDocument.ParseAsync(ctx.Request.Body);
    var root = json.RootElement;

    // default filter values  
    string query = "*";
    DateTimeOffset? from = null;
    DateTimeOffset? to = null;
    int pageLimit = defaultPageLimit;
    string? cursor = null;
    string sort = "-timestamp";

    if (root.TryGetProperty("filter", out var filter))
    {
        if (filter.TryGetProperty("query", out var q)) query = q.GetString() ?? "*";
        if (filter.TryGetProperty("from", out var f) && DateTimeOffset.TryParse(f.GetString(), out var ff)) from = ff;
        if (filter.TryGetProperty("to", out var tto) && DateTimeOffset.TryParse(tto.GetString(), out var tt)) to = tt;
    }

    if (root.TryGetProperty("page", out var page))
    {
        if (page.TryGetProperty("limit", out var pl) && pl.TryGetInt32(out var plv)) pageLimit = plv;
        if (page.TryGetProperty("cursor", out var c) && c.ValueKind == JsonValueKind.String) cursor = c.GetString();
    }

    if (root.TryGetProperty("sort", out var s) && s.ValueKind == JsonValueKind.String) sort = s.GetString() ?? sort;

    // Apply time filter (if not provided => take all)  
    var filtered = logs.AsEnumerable();
    if (from.HasValue) filtered = filtered.Where(l => l.Timestamp >= from.Value);
    if (to.HasValue) filtered = filtered.Where(l => l.Timestamp <= to.Value);

    // Apply query simple matching: supports '*' or substrings or basic OR: "service:web OR service:api" or "timeout" etc.  
    filtered = ApplyQueryFilter(filtered, query);

    // Sorting: newest first if "-timestamp"  
    bool desc = sort?.StartsWith("-") ?? true;
    filtered = desc ? filtered.OrderByDescending(l => l.Timestamp) : filtered.OrderBy(l => l.Timestamp);

    // Decode cursor -> integer offset. Cursor scheme: base64("idx:{startIndex}")  
    int startIndex = 0;
    if (!string.IsNullOrEmpty(cursor))
    {
        try
        {
            var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(cursor));
            if (decoded.StartsWith("idx:") && int.TryParse(decoded.Substring(4), out var idx)) startIndex = idx;
        }
        catch { startIndex = 0; }
    }

    var pageItems = filtered.Skip(startIndex).Take(pageLimit).ToList();
    int nextIndex = startIndex + pageItems.Count;

    // Build response in Datadog-like shape  
    var resp = new
    {
        data = pageItems.Select(item => new
        {
            id = item.Id,
            type = "logs_event",
            attributes = new
            {
                message = item.Message,
                timestamp = item.Timestamp.ToString("o"),
                service = item.Service,
                host = item.Host
            }
        }).ToArray(),
        meta = new
        {
            page = new
            {
                // If there are more results, return after cursor  
                after = filtered.Skip(nextIndex).Any() ? Convert.ToBase64String(Encoding.UTF8.GetBytes($"idx:{nextIndex}")) : null,
                limit = pageLimit
            },
            // sample extra meta fields  
            total = filtered.Count()
        }
    };

    ctx.Response.ContentType = "application/json";
    await ctx.Response.WriteAsync(JsonSerializer.Serialize(resp, new JsonSerializerOptions { WriteIndented = true }));
});

// Simple GET to fetch tail (simulate dotnet CLI convenience)  
app.MapGet("/tail", (HttpRequest req) =>
{
    // ?limit=200&from=...&to=...&query=...  
    int limit = int.TryParse(req.Query["limit"], out var l) ? l : 200;
    var fromQs = req.Query["from"].FirstOrDefault();
    var toQs = req.Query["to"].FirstOrDefault();
    var query = req.Query["query"].FirstOrDefault() ?? "*";

    DateTimeOffset? from = null, to = null;
    if (!string.IsNullOrEmpty(fromQs) && DateTimeOffset.TryParse(fromQs, out var f)) from = f;
    if (!string.IsNullOrEmpty(toQs) && DateTimeOffset.TryParse(toQs, out var tval)) to = tval;

    var filtered = logs.AsEnumerable();
    if (from.HasValue) filtered = filtered.Where(l => l.Timestamp >= from.Value);
    if (to.HasValue) filtered = filtered.Where(l => l.Timestamp <= to.Value);
    filtered = ApplyQueryFilter(filtered, query);
    var result = filtered.OrderByDescending(l => l.Timestamp).Take(limit).Select(l => new { l.Timestamp, l.Service, l.Message }).ToList();
    return Results.Json(result);
});

app.Run();

static IEnumerable<LogItem> GenerateLogs(int n, string[] services)
{
    var rng = new Random(1234);
    var now = DateTimeOffset.UtcNow;
    for (int i = 0; i < n; i++)
    {
        var time = now.AddSeconds(-i * 5); // spaced by 5 seconds
        var service = services[i % services.Length];
        var statusProb = rng.NextDouble();
        string message;
        if (statusProb < 0.03)
            message = $"System.NullReferenceException: Object reference not set to an instance of an object. user=john.doe email=john.doe@example.com ip=192.168.1.{rng.Next(1, 255)}";
        else if (statusProb < 0.06)
            message = $"Microsoft.Data.SqlClient.SqlException (0x80131904): Timeout expired while executing query. connectionString=Server=db;User Id=sa;";
        else if (statusProb < 0.10)
            message = $"HTTP 503 Service Unavailable - upstream_timeout=true path=/api/orders service={service}";
        else if (statusProb < 0.13)
            message = $"TaskCanceledException: A task was canceled. token=eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...";
        else if (statusProb < 0.2)
            message = $"INFO processed request path=/health status=200 service={service}";
        else
            message = $"INFO processed request path=/api/products status=200 service={service} userId=user{rng.Next(1, 500)}";

        yield return new LogItem
        {
            Id = Guid.NewGuid().ToString(),
            Timestamp = time,
            Service = service,
            Host = $"host-{rng.Next(1, 10)}",
            Message = message
        };
    }
}

static IEnumerable<LogItem> ApplyQueryFilter(IEnumerable<LogItem> items, string query)
{
    if (string.IsNullOrWhiteSpace(query) || query == "*") return items;
    query = query.Trim();

    // support simple OR: "service:web OR service:api"
    if (query.Contains(" OR ", StringComparison.OrdinalIgnoreCase))
    {
        var parts = query.Split(" OR ", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var sets = parts.Select(p => p.ToLowerInvariant());
        return items.Where(it => sets.Any(s => SimpleMatch(it, s)));
    }

    // otherwise treat as substring search
    return items.Where(it => SimpleMatch(it, query.ToLowerInvariant()));
}

static bool SimpleMatch(LogItem it, string q)
{
    if (q.StartsWith("service:", StringComparison.OrdinalIgnoreCase))
    {
        var svc = q.Substring("service:".Length);
        return it.Service.Equals(svc, StringComparison.OrdinalIgnoreCase);
    }
    // other quick pattern: status:(5\d\d)
    if (q.StartsWith("status:") && q.Contains("5"))
    {
        return it.Message.Contains("503") || it.Message.Contains("504") || it.Message.Contains("500") || it.Message.Contains("Timeout");
    }
    return it.Message.Contains(q, StringComparison.OrdinalIgnoreCase) || it.Service.Contains(q, StringComparison.OrdinalIgnoreCase);
}

internal record LogItem
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public DateTimeOffset Timestamp { get; init; }
    public string Service { get; init; } = "";
    public string Host { get; init; } = "";
    public string Message { get; init; } = "";
}
