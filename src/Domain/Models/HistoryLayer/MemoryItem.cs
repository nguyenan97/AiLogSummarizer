using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Domain.Models.HistoryLayer
{
    public record Message(
           [property: JsonPropertyName("role")] string Role,
           [property: JsonPropertyName("content")] string Content
       );

    public record AddMemoriesRequest(
        [property: JsonPropertyName("messages")] List<Message> Messages,
        [property: JsonPropertyName("agent_id")] string? AgentId = null,
        [property: JsonPropertyName("user_id")] string? UserId = null,
        [property: JsonPropertyName("app_id")] string? AppId = null,
        [property: JsonPropertyName("run_id")] string? RunId = null,
        [property: JsonPropertyName("metadata")] object? Metadata = null,
        [property: JsonPropertyName("includes")] string? Includes = null,
        [property: JsonPropertyName("excludes")] string? Excludes = null,
        [property: JsonPropertyName("infer")] bool? Infer = null,
        [property: JsonPropertyName("output_format")] string? OutputFormat = null,
        [property: JsonPropertyName("custom_categories")] object? CustomCategories = null,
        [property: JsonPropertyName("custom_instructions")] string? CustomInstructions = null,
        [property: JsonPropertyName("immutable")] bool? Immutable = null,
        [property: JsonPropertyName("async_mode")] bool? AsyncMode = null,
        [property: JsonPropertyName("timestamp")] long? Timestamp = null,
        [property: JsonPropertyName("expiration_date")] string? ExpirationDate = null,
        [property: JsonPropertyName("org_id")] string? OrgId = null,
        [property: JsonPropertyName("project_id")] string? ProjectId = null,
        [property: JsonPropertyName("version")] string? Version = "v2",
        [property: JsonPropertyName("enable_graph")] bool? EnableGraph = null
    );

    public record AddMemoriesResponseEntry(
        string Id,
        AddMemoriesResponseData Data,
        string Event
    );

    public record AddMemoriesResponseData(
        [property: JsonPropertyName("memory")] string Memory
    );

    public record SearchMemoriesV2Request(
        [property: JsonPropertyName("query")] string Query,
        [property: JsonPropertyName("filters")] object? Filters = null,
        [property: JsonPropertyName("fields")] string[]? Fields = null,
        [property: JsonPropertyName("page")] int? Page = null,
        [property: JsonPropertyName("page_size")] int? PageSize = null,
        [property: JsonPropertyName("org_id")] string? OrgId = null,
        [property: JsonPropertyName("project_id")] string? ProjectId = null,
        [property: JsonPropertyName("enable_graph")] bool? EnableGraph = null
    );

    public record MemoryItem(
        [property: JsonPropertyName("id")] string Id,
        [property: JsonPropertyName("memory")] string? Memory,
        [property: JsonPropertyName("text")] string? Text,
        [property: JsonPropertyName("user_id")] string? UserId,
        [property: JsonPropertyName("agent_id")] string? AgentId,
        [property: JsonPropertyName("app_id")] string? AppId,
        [property: JsonPropertyName("run_id")] string? RunId,
        [property: JsonPropertyName("immutable")] bool? Immutable,
        [property: JsonPropertyName("expiration_date")] string? ExpirationDate,
        [property: JsonPropertyName("created_at")] DateTimeOffset? CreatedAt,
        [property: JsonPropertyName("updated_at")] DateTimeOffset? UpdatedAt,
        [property: JsonPropertyName("owner")] string? Owner,
        [property: JsonPropertyName("organization")] string? Organization,
        [property: JsonPropertyName("metadata")] object? Metadata,
        [property: JsonPropertyName("categories")] string[]? Categories
    );

    public record GetMemoriesV2Request(
        [property: JsonPropertyName("filters")] object Filters,
        [property: JsonPropertyName("fields")] string[]? Fields = null,
        [property: JsonPropertyName("page")] int? Page = null,
        [property: JsonPropertyName("page_size")] int? PageSize = null,
        [property: JsonPropertyName("org_id")] string? OrgId = null,
        [property: JsonPropertyName("project_id")] string? ProjectId = null,
        [property: JsonPropertyName("enable_graph")] bool? EnableGraph = null
    );

    // Helper classes for complex filters
    public record FilterCondition(
        [property: JsonPropertyName("AND")] object[]? And = null,
        [property: JsonPropertyName("OR")] object[]? Or = null
    );

    public record DateRangeFilter(
        [property: JsonPropertyName("gte")] string? GreaterThanOrEqual = null,
        [property: JsonPropertyName("lte")] string? LessThanOrEqual = null,
        [property: JsonPropertyName("gt")] string? GreaterThan = null,
        [property: JsonPropertyName("lt")] string? LessThan = null
    );

    // Helper methods for creating filters
    public static class FilterHelpers
    {
        public static FilterCondition And(params object[] conditions) => new(And: conditions);
        public static FilterCondition Or(params object[] conditions) => new(Or: conditions);

        public static object UserIdFilter(string userId) => new { user_id = userId };
        public static object AgentIdFilter(string agentId) => new { agent_id = agentId };
        public static object AppIdFilter(string appId) => new { app_id = appId };

        public static object CreatedAtFilter(string? gte = null, string? lte = null, string? gt = null, string? lt = null) =>
            new { created_at = new DateRangeFilter(gte, lte, gt, lt) };

        public static object UpdatedAtFilter(string? gte = null, string? lte = null, string? gt = null, string? lt = null) =>
            new { updated_at = new DateRangeFilter(gte, lte, gt, lt) };
    }

    public record UpdateMemoryRequest(
        [property: JsonPropertyName("text")] string? Text = null,
        [property: JsonPropertyName("metadata")] object? Metadata = null
    );

    public record DeleteResponse(
        [property: JsonPropertyName("message")] string Message
    );

    public record HistoryItem(
        [property: JsonPropertyName("id")] string Id,
        [property: JsonPropertyName("memory_id")] string MemoryId,
        [property: JsonPropertyName("input")] List<Message> Input,
        [property: JsonPropertyName("old_memory")] string? OldMemory,
        [property: JsonPropertyName("new_memory")] string NewMemory,
        [property: JsonPropertyName("user_id")] string UserId,
        [property: JsonPropertyName("event")] string Event,
        [property: JsonPropertyName("metadata")] object? Metadata,
        [property: JsonPropertyName("created_at")] DateTimeOffset CreatedAt,
        [property: JsonPropertyName("updated_at")] DateTimeOffset UpdatedAt
    );

}
