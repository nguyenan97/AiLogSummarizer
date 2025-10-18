using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Domain.Models.HistoryLayer;

namespace Infrastructure.Services.Mem0
{
    public interface IMem0Client
    {
        /// <summary>
        /// POST /v1/memories — Add memories extracted from messages.
        /// </summary>
        Task<List<AddMemoriesResponseEntry>> AddMemoriesAsync(
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
            CancellationToken ct = default);

        /// <summary>
        /// POST /v2/memories/search — Semantic search with optional filters.
        /// </summary>
        Task<List<MemoryItem>> SearchMemoriesV2Async(
            string query,
            object? filters = null,
            string[]? fields = null,
            int? page = null,
            int? pageSize = null,
            bool? enableGraph = null,
            CancellationToken ct = default);

        /// <summary>
        /// POST /v2/memories — Filtered listing without a query.
        /// </summary>
        Task<List<MemoryItem>> GetMemoriesV2Async(
            object filters,
            string[]? fields = null,
            int? page = null,
            int? pageSize = null,
            bool? enableGraph = null,
            CancellationToken ct = default);

        /// <summary>
        /// GET /v1/memories/{id}
        /// </summary>
        Task<MemoryItem> GetMemoryAsync(string memoryId, CancellationToken ct = default);

        /// <summary>
        /// PUT /v1/memories/{id}
        /// </summary>
        Task<MemoryItem> UpdateMemoryAsync(string memoryId, string? text = null, object? metadata = null, CancellationToken ct = default);

        /// <summary>
        /// DELETE /v1/memories/{id}
        /// </summary>
        Task<bool> DeleteMemoryAsync(string memoryId, CancellationToken ct = default);

        /// <summary>
        /// GET /v1/memories/{id}/history
        /// </summary>
        Task<List<HistoryItem>> GetMemoryHistoryAsync(string memoryId, CancellationToken ct = default);
    }
}