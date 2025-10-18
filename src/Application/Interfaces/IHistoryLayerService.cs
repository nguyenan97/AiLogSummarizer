using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Models.HistoryLayer;

namespace Application.Interfaces
{
    public interface IHistoryLayerService
    {
        Task<List<AddMemoriesResponseEntry>> AddMemoryAsync(string userMessage, string userId, string? assistantResponse = null);
        Task<IEnumerable<MemoryItem>> SearchMemoriesAsync(string query, string userId, int? limit = 5);
        Task<IEnumerable<MemoryItem>> GetMemoriesAsync(object filters, string[]? fields = null, int? page = null, int? pageSize = null, bool? enableGraph = null);
        Task<MemoryItem> GetMemoryAsync(string id);
        Task UpdateMemoryAsync(string id, string newText, object? metadata = null);
        Task DeleteMemoryAsync(string id);
        Task<IEnumerable<MemoryItem>> GetUserMemoriesAsync(string userId);
    }
}
