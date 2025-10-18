using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Interfaces;
using Domain.Models.HistoryLayer;

namespace Infrastructure.Services.Mem0
{
    public class LocalHistoryLayerService : IHistoryLayerService
    {
        private readonly ILocalMem0Client _localMem0Client;
        public LocalHistoryLayerService(ILocalMem0Client localMem0Client)
        {
            _localMem0Client = localMem0Client;
        }
        public async Task<List<AddMemoriesResponseEntry>> AddMemoryAsync(string userMessage, string userId, string? assistantResponse = null)
        {
            var response = await _localMem0Client.CreateMemoriesAsync(new List<Message>
            {
                new Message("user", userMessage),
                new Message("assistant", assistantResponse ?? string.Empty)
            }, userId: userId);

            return response.Results.Select(a => new AddMemoriesResponseEntry (
                a.Id, new AddMemoriesResponseData(a.Memory), a.Event
            )).ToList();
        }

        public Task DeleteMemoryAsync(string id)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<MemoryItem>> GetMemoriesAsync(object filters, string[]? fields = null, int? page = null, int? pageSize = null, bool? enableGraph = null)
        {
            string? userId = null;
            string? runId = null;
            string? agentId = null;

            // Extract filter values safely using reflection
            var filtersType = filters.GetType();
            var userIdProp = filtersType.GetProperty("user_id");
            var runIdProp = filtersType.GetProperty("run_id");
            var agentIdProp = filtersType.GetProperty("agent_id");

            userId = userIdProp?.GetValue(filters)?.ToString();
            runId = runIdProp?.GetValue(filters)?.ToString();
            agentId = agentIdProp?.GetValue(filters)?.ToString();

            var response = await _localMem0Client.GetMemoriesAsync(
                userId: userId,
                runId: runId,
                agentId: agentId
            );

            var memoryItems = new List<MemoryItem>();
            foreach (var item in response.Results)
            {
                memoryItems.Add(new MemoryItem(
                    Id: item.Id,
                    Memory: null,
                    Text: item.Memory,
                    UserId: item.UserId,
                    AgentId: null,
                    AppId: null,
                    RunId: null,
                    Immutable: null,
                    ExpirationDate: null,
                    CreatedAt: item.CreatedAt,
                    UpdatedAt: item.UpdatedAt,
                    Owner: null,
                    Organization: null,
                    Metadata: item.Metadata,
                    Categories: null
                ));
            }

            return memoryItems;
        }

        public Task<MemoryItem> GetMemoryAsync(string id)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<MemoryItem>> GetUserMemoriesAsync(string userId)
        {
            var filters = FilterHelpers.UserIdFilter(userId);
            var memories = await GetMemoriesAsync(filters, null, null, null);
            return memories;
        }

        public Task<IEnumerable<MemoryItem>> SearchMemoriesAsync(string query, string userId, int? limit = 5)
        {
            throw new NotImplementedException();
        }

        public Task UpdateMemoryAsync(string id, string newText, object? metadata = null)
        {
            throw new NotImplementedException();
        }
    }
}
