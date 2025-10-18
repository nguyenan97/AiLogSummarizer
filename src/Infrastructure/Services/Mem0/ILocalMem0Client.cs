using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Domain.Models.HistoryLayer;

namespace Infrastructure.Services.Mem0
{
    /// <summary>
    /// Interface for the local Mem0 API client.
    /// Covers Create Memories and Get Memories operations.
    /// </summary>
    public interface ILocalMem0Client
    {
        /// <summary>
        /// POST /memories — Store new memories.
        /// </summary>
        Task<LocalCreateMemoriesResponse> CreateMemoriesAsync(
            List<Message> messages,
            string? userId = null,
            string? agentId = null,
            string? runId = null,
            object? metadata = null,
            CancellationToken ct = default);

        /// <summary>
        /// GET /memories — Retrieve stored memories with optional filters.
        /// </summary>
        Task<LocalGetMemoriesResponse> GetMemoriesAsync(
            string? userId = null,
            string? runId = null,
            string? agentId = null,
            CancellationToken ct = default);
    }

    // Response models for Local Mem0 API

    /// <summary>
    /// Request model for creating memories in the local Mem0 API.
    /// </summary>
    public record LocalMemoryCreateRequest(
        [property: JsonPropertyName("messages")] List<Message> Messages,
        [property: JsonPropertyName("user_id")] string? UserId = null,
        [property: JsonPropertyName("agent_id")] string? AgentId = null,
        [property: JsonPropertyName("run_id")] string? RunId = null,
        [property: JsonPropertyName("metadata")] object? Metadata = null
    );

    /// <summary>
    /// Response model for creating memories in the local Mem0 API.
    /// </summary>
    public record LocalCreateMemoriesResponse(
        [property: JsonPropertyName("results")] List<LocalMemoryResult> Results,
        [property: JsonPropertyName("relations")] LocalMemoryRelations? Relations = null
    );

    /// <summary>
    /// Individual memory result in the create memories response.
    /// </summary>
    public record LocalMemoryResult(
        [property: JsonPropertyName("id")] string Id,
        [property: JsonPropertyName("memory")] string Memory,
        [property: JsonPropertyName("event")] string Event
    );

    /// <summary>
    /// Relations data including added and deleted entities.
    /// </summary>
    public record LocalMemoryRelations(
        [property: JsonPropertyName("deleted_entities")] List<List<LocalEntityRelation>>? DeletedEntities = null,
        [property: JsonPropertyName("added_entities")] List<List<LocalEntityRelation>>? AddedEntities = null
    );

    /// <summary>
    /// Entity relationship information.
    /// </summary>
    public record LocalEntityRelation(
        [property: JsonPropertyName("source")] string Source,
        [property: JsonPropertyName("target")] string Target,
        [property: JsonPropertyName("relationship")] string Relationship
    );

    /// <summary>
    /// Response model for getting memories from the local Mem0 API.
    /// </summary>
    public record LocalGetMemoriesResponse(
        [property: JsonPropertyName("results")] List<LocalMemoryItem> Results,
        [property: JsonPropertyName("relations")] List<LocalEntityRelation>? Relations = null
    );

    /// <summary>
    /// Memory item in the get memories response.
    /// </summary>
    public record LocalMemoryItem(
        [property: JsonPropertyName("id")] string Id,
        [property: JsonPropertyName("memory")] string Memory,
        [property: JsonPropertyName("hash")] string Hash,
        [property: JsonPropertyName("metadata")] object? Metadata,
        [property: JsonPropertyName("created_at")] DateTimeOffset CreatedAt,
        [property: JsonPropertyName("updated_at")] DateTimeOffset? UpdatedAt,
        [property: JsonPropertyName("user_id")] string UserId
    );
}