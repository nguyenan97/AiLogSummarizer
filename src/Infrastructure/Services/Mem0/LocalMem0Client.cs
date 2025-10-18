using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Domain.Models.HistoryLayer;

namespace Infrastructure.Services.Mem0
{
    /// <summary>
    /// Local Mem0 API client implementation.
    /// Wraps the local Mem0 REST API for Create Memories and Get Memories operations.
    /// </summary>
    public class LocalMem0Client : ILocalMem0Client, IDisposable
    {
        private readonly HttpClient _http;
        private readonly JsonSerializerOptions _json;
        private readonly bool _ownsHttpClient;
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the LocalMem0Client class.
        /// </summary>
        /// <param name="baseUrl">Local Mem0 API base URL (e.g., http://localhost:8000)</param>
        /// <param name="httpClient">Optional HttpClient. If null, a new instance is created and owned by this client.</param>
        public LocalMem0Client(
            string baseUrl,
            HttpClient? httpClient = null)
        {
            if (string.IsNullOrWhiteSpace(baseUrl))
                throw new ArgumentException("Base URL is required", nameof(baseUrl));

            _ownsHttpClient = httpClient == null;
            _http = httpClient ?? new HttpClient { BaseAddress = new Uri(baseUrl) };

            _json = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
            };
        }

        /// <summary>
        /// POST /memories — Store new memories.
        /// </summary>
        public async Task<LocalCreateMemoriesResponse> CreateMemoriesAsync(
            List<Message> messages,
            string? userId = null,
            string? agentId = null,
            string? runId = null,
            object? metadata = null,
            CancellationToken ct = default)
        {
            var body = new LocalMemoryCreateRequest(
                Messages: messages,
                UserId: userId,
                AgentId: agentId,
                RunId: runId,
                Metadata: metadata
            );

            var res = await _http.PostAsJsonAsync("/memories", body, _json, ct);
            res.EnsureSuccessStatusCode();
            return await res.Content.ReadFromJsonAsync<LocalCreateMemoriesResponse>(_json, ct) 
                ?? throw new InvalidOperationException("Failed to deserialize response");
        }

        /// <summary>
        /// GET /memories — Retrieve stored memories with optional filters.
        /// </summary>
        public async Task<LocalGetMemoriesResponse> GetMemoriesAsync(
            string? userId = null,
            string? runId = null,
            string? agentId = null,
            CancellationToken ct = default)
        {
            var queryParams = new List<string>();
            
            if (!string.IsNullOrEmpty(userId))
                queryParams.Add($"user_id={HttpUtility.UrlEncode(userId)}");
            
            if (!string.IsNullOrEmpty(runId))
                queryParams.Add($"run_id={HttpUtility.UrlEncode(runId)}");
            
            if (!string.IsNullOrEmpty(agentId))
                queryParams.Add($"agent_id={HttpUtility.UrlEncode(agentId)}");

            var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
            var url = $"/memories{queryString}";

            var res = await _http.GetAsync(url, ct);
            res.EnsureSuccessStatusCode();
            return await res.Content.ReadFromJsonAsync<LocalGetMemoriesResponse>(_json, ct) 
                ?? throw new InvalidOperationException("Failed to deserialize response");
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing && _ownsHttpClient)
                {
                    _http?.Dispose();
                }
                _disposed = true;
            }
        }

        /// <summary>
        /// Disposes the resources used by the LocalMem0Client.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}