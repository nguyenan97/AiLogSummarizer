using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Domain.Models.HistoryLayer;

namespace Infrastructure.Services.Mem0
{
    /// <summary>
    /// Thin C# wrapper for the Mem0 REST API.
    /// Covers core Memory endpoints: add, search (v2), list/get (v2), get by id,
    /// update, delete, and history. See method docs for endpoint mapping.
    /// </summary>
    public class Mem0Client : IMem0Client, IDisposable
    {
        private readonly HttpClient _http;
        private readonly string? _orgId;
        private readonly string? _projectId;
        private readonly JsonSerializerOptions _json;
        private bool _disposed;

        /// <param name="apiKey">Your Mem0 API key. Sent as <c>Authorization: Token &lt;key&gt;</c>.</param>
        /// <param name="baseUrl">API origin. Default is the public Mem0 API.</param>
        /// <param name="orgId">Optional organization id to scope calls.</param>
        /// <param name="projectId">Optional project id to scope calls.</param>
        /// <param name="httpClient">Optional HttpClient. If null, a new instance is created and owned by this client.</param>
        public Mem0Client(
            string apiKey,
            string baseUrl = "https://api.mem0.ai",
            string? orgId = null,
            string? projectId = null,
            HttpClient? httpClient = null)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new ArgumentException("API key is required", nameof(apiKey));

            _orgId = orgId;
            _projectId = projectId;

            _http = httpClient ?? new HttpClient { BaseAddress = new Uri(baseUrl) };
            _http.DefaultRequestHeaders.Add("Authorization", $"Token {apiKey}");

            _json = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
            };
        }

        // -------------------- Memory APIs --------------------

        /// <summary>
        /// POST /v1/memories — Add memories extracted from messages.
        /// </summary>
        public async Task<List<AddMemoriesResponseEntry>> AddMemoriesAsync(
            List<Message> messages,
            string? userId = null,
            string? agentId = null,
            string? appId = null,
            string? runId = null,
            object? metadata = null,
            string? includes = null,
            string? excludes = null,
            bool? infer = null,
            string? outputFormat = null,
            object? customCategories = null,
            string? customInstructions = null,
            bool? immutable = null,
            bool? asyncMode = null,
            long? timestamp = null,
            string? expirationDate = null,
            string version = "v2",
            bool? enableGraph = null,
            CancellationToken ct = default)
        {
            var body = new AddMemoriesRequest(
                Messages: messages,
                AgentId: agentId,
                UserId: userId,
                AppId: appId,
                RunId: runId,
                Metadata: metadata,
                Includes: includes,
                Excludes: excludes,
                Infer: infer,
                OutputFormat: outputFormat,
                CustomCategories: customCategories,
                CustomInstructions: customInstructions,
                Immutable: immutable,
                AsyncMode: asyncMode,
                Timestamp: timestamp,
                ExpirationDate: expirationDate,
                OrgId: _orgId,
                ProjectId: _projectId,
                Version: version,
                EnableGraph: enableGraph
            );

            var res = await _http.PostAsJsonAsync("/v1/memories/", body, _json, ct);
            res.EnsureSuccessStatusCode();
            return await res.Content.ReadFromJsonAsync<List<AddMemoriesResponseEntry>>(_json, ct) ?? new List<AddMemoriesResponseEntry>();
        }

        /// <summary>
        /// POST /v2/memories/search — Semantic search with optional filters.
        /// </summary>
        public async Task<List<MemoryItem>> SearchMemoriesV2Async(
            string query,
            object? filters = null,
            string[]? fields = null,
            int? page = null,
            int? pageSize = null,
            bool? enableGraph = null,
            CancellationToken ct = default)
        {
            var body = new SearchMemoriesV2Request(
                Query: query,
                Filters: filters,
                Fields: fields,
                Page: page,
                PageSize: pageSize,
                OrgId: _orgId,
                ProjectId: _projectId,
                EnableGraph: enableGraph
            );
            var res = await _http.PostAsJsonAsync("/v2/memories/search/", body, _json, ct);
            res.EnsureSuccessStatusCode();
            return await res.Content.ReadFromJsonAsync<List<MemoryItem>>(_json, ct) ?? new List<MemoryItem>();
        }

        /// <summary>
        /// POST /v2/memories — Filtered listing without a query.
        /// </summary>
        public async Task<List<MemoryItem>> GetMemoriesV2Async(
            object filters,
            string[]? fields = null,
            int? page = null,
            int? pageSize = null,
            bool? enableGraph = null,
            CancellationToken ct = default)
        {
            var body = new GetMemoriesV2Request(
                Filters: filters,
                Fields: fields,
                Page: page,
                PageSize: pageSize,
                OrgId: _orgId,
                ProjectId: _projectId,
                EnableGraph: enableGraph
            );
            var res = await _http.PostAsJsonAsync("/v2/memories/", body, _json, ct);
            res.EnsureSuccessStatusCode();
            return await res.Content.ReadFromJsonAsync<List<MemoryItem>>(_json, ct) ?? new List<MemoryItem>();
        }

        /// <summary>
        /// GET /v1/memories/{id}
        /// </summary>
        public async Task<MemoryItem> GetMemoryAsync(string memoryId, CancellationToken ct = default)
        {
            var res = await _http.GetAsync($"/v1/memories/{Uri.EscapeDataString(memoryId)}/", ct);
            res.EnsureSuccessStatusCode();
            return await res.Content.ReadFromJsonAsync<MemoryItem>(_json, ct) ?? throw new InvalidOperationException("Failed to deserialize response");
        }

        /// <summary>
        /// PUT /v1/memories/{id}
        /// </summary>
        public async Task<MemoryItem> UpdateMemoryAsync(string memoryId, string? text = null, object? metadata = null, CancellationToken ct = default)
        {
            var body = new UpdateMemoryRequest(text, metadata);
            var res = await _http.PutAsJsonAsync($"/v1/memories/{Uri.EscapeDataString(memoryId)}/", body, _json, ct);
            res.EnsureSuccessStatusCode();
            return await res.Content.ReadFromJsonAsync<MemoryItem>(_json, ct) ?? throw new InvalidOperationException("Failed to deserialize response");
        }

        /// <summary>
        /// DELETE /v1/memories/{id}
        /// </summary>
        public async Task<bool> DeleteMemoryAsync(string memoryId, CancellationToken ct = default)
        {
            var res = await _http.DeleteAsync($"/v1/memories/{Uri.EscapeDataString(memoryId)}/", ct);
            // API returns 204 with a small JSON body; treat any 2xx as success.
            return res.IsSuccessStatusCode;
        }

        /// <summary>
        /// GET /v1/memories/{id}/history
        /// </summary>
        public async Task<List<HistoryItem>> GetMemoryHistoryAsync(string memoryId, CancellationToken ct = default)
        {
            var res = await _http.GetAsync($"/v1/memories/{Uri.EscapeDataString(memoryId)}/history/", ct);
            res.EnsureSuccessStatusCode();
            return await res.Content.ReadFromJsonAsync<List<HistoryItem>>(_json, ct) ?? new List<HistoryItem>();
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _http.Dispose();
                _disposed = true;
            }
        }
    }

    public class Mem0HttpException : Exception
    {
        public int StatusCode { get; }
        public string ResponseBody { get; }

        public Mem0HttpException(int statusCode, string body)
            : base($"Mem0 API error {statusCode}: {Truncate(body, 500)}")
        {
            StatusCode = statusCode;
            ResponseBody = body;
        }

        private static string Truncate(string? s, int length)
            => string.IsNullOrEmpty(s) || s!.Length <= length ? (s ?? string.Empty) : s.Substring(0, length) + "…";
    }
}