using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Application.Interfaces;
using Domain.Models.HistoryLayer;
using Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services.Mem0
{
    public class HistoryLayerService : IHistoryLayerService
    {
        private readonly IMem0Client _mem0Client;
        private readonly ILogger<HistoryLayerService> _logger;

        public HistoryLayerService(IMem0Client mem0Client, ILogger<HistoryLayerService> logger)
        {
            _mem0Client = mem0Client;
            _logger = logger;
        }

        public async Task<List<AddMemoriesResponseEntry>> AddMemoryAsync(string userMessage, string userId, string? assistantResponse = null)
        {
            var messages = new List<Message>
            {
                new("user", userMessage)
            };

            if (!string.IsNullOrWhiteSpace(assistantResponse))
            {
                messages.Add(new Message("assistant", assistantResponse));
            }

            var result = await _mem0Client.AddMemoriesAsync(messages, userId);
            var memoryId = result.FirstOrDefault()?.Id;
            if(!string.IsNullOrEmpty(memoryId))
                _logger.LogInformation("Memory created with ID: {MemoryId}", memoryId);
            return result;
        }

        public async Task<IEnumerable<MemoryItem>> GetUserMemoriesAsync(string userId)
        {
            var filters = FilterHelpers.UserIdFilter(userId);
            var memories = await GetMemoriesAsync(filters, null, null, null);
            return memories;
        }

        public async Task<IEnumerable<MemoryItem>> SearchMemoriesAsync(string query, string userId, int? limit = 5)
        {
            var filters = new { user_id = userId };
            var memories = await _mem0Client.SearchMemoriesV2Async(query, filters, pageSize: limit);
            
            return memories;
        }

        public async Task<MemoryItem> GetMemoryAsync(string id)
        {
            var memory = await _mem0Client.GetMemoryAsync(id);
            return memory;
        }

        public async Task UpdateMemoryAsync(string id, string newText, object? metadata = null)
        {
            await _mem0Client.UpdateMemoryAsync(id, newText, metadata);
        }

        public async Task DeleteMemoryAsync(string id)
        {
            await _mem0Client.DeleteMemoryAsync(id);
        }


        public async Task<IEnumerable<MemoryItem>> GetMemoriesAsync(object filters, string[]? fields = null, int? page = null, int? pageSize = null, bool? enableGraph = null)
        {
            var memories = await _mem0Client.GetMemoriesV2Async(filters, fields, page, pageSize, enableGraph);
            
            return memories;
        }
    }
}
