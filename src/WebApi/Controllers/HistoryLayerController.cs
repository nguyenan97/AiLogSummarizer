using Application.Interfaces;
using Domain.Models.HistoryLayer;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [ApiController]
    public class HistoryLayerController : ControllerBase
    {
        // this controller is for testing the Mem0 vector database service
        // it will be removed in the future
        private readonly ILogger<HistoryLayerController> _logger;
        private readonly IHistoryLayerService _historyLayerService;

        public HistoryLayerController(ILogger<HistoryLayerController> logger, IHistoryLayerService historyLayerService)
        {
            _logger = logger;
            _historyLayerService = historyLayerService;
        }

        [HttpGet("api/historylayer/test")]
        public async Task<IActionResult> Test()
        {
            try
            {
                // Test adding a memory using the service
                await _historyLayerService.AddMemoryAsync(
                    "Hi, I'm Tien Nguyen. I'm a software developer", 
                    "alex", 
                    "Hello Tien Nguyen!"
                );

                return Ok(new { 
                    message = "HistoryLayerController Test endpoint called",
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing HistoryLayerService");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("api/historylayer/add")]
        public async Task<IActionResult> AddMemory([FromBody] AddMemoryRequest request)
        {
            try
            {
                var response = await _historyLayerService.AddMemoryAsync(
                    request.UserMessage, 
                    request.UserId, 
                    request.AssistantResponse
                );

                _logger.LogInformation("Memory added for user {UserId}", request.UserId);
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding memory for user {UserId}", request.UserId);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("api/historylayer/search")]
        public async Task<IActionResult> SearchMemories([FromBody] SearchMemoryRequest request)
        {
            try
            {
                var memories = await _historyLayerService.SearchMemoriesAsync(
                    request.Query, 
                    request.UserId, 
                    request.Limit
                );

                _logger.LogInformation("Found {Count} memories for user {UserId}", memories.Count(), request.UserId);
                
                return Ok(new { results = memories });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching memories for user {UserId}", request.UserId);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("api/historylayer/memories/v2")]
        public async Task<IActionResult> GetMemoriesV2([FromBody] GetMemoriesV2Request request)
        {
            try
            {
                var memories = await _historyLayerService.GetMemoriesAsync(
                    request.Filters,
                    request.Fields,
                    request.Page,
                    request.PageSize,
                    request.EnableGraph
                );

                _logger.LogInformation("Retrieved {Count} memories with filters", memories.Count());
                
                return Ok(new { results = memories });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting memories v2");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("api/historylayer/memory/{id}")]
        public async Task<IActionResult> GetMemory(string id)
        {
            try
            {
                var memory = await _historyLayerService.GetMemoryAsync(id);
                
                _logger.LogInformation("Retrieved memory {MemoryId}", id);
                
                return Ok(memory);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving memory {MemoryId}", id);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpDelete("api/historylayer/memory/{id}")]
        public async Task<IActionResult> DeleteMemory(string id)
        {
            try
            {
                await _historyLayerService.DeleteMemoryAsync(id);
                
                _logger.LogInformation("Deleted memory {MemoryId}", id);
                
                return Ok(new { message = $"Memory {id} deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting memory {MemoryId}", id);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("api/historylayer/memories/user/{userId}")]
        public async Task<IActionResult> GetMemoriesByUser(string userId)
        {
            try
            {
                var memories = await _historyLayerService.GetUserMemoriesAsync(userId);

                _logger.LogInformation("Retrieved {Count} memories for user {UserId}", memories.Count(), userId);
                
                return Ok(new { results = memories });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting memories for user {UserId}", userId);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("api/historylayer/memories/user/{userId}/recent")]
        public async Task<IActionResult> GetRecentMemoriesByUser(string userId, [FromQuery] string? since = null, [FromQuery] int? pageSize = 10)
        {
            try
            {
                var sinceDate = since ?? DateTime.UtcNow.AddDays(-30).ToString("yyyy-MM-dd");
                
                var filters = FilterHelpers.And(
                    FilterHelpers.UserIdFilter(userId),
                    FilterHelpers.CreatedAtFilter(gte: sinceDate)
                );
                
                var memories = await _historyLayerService.GetMemoriesAsync(filters, null, null, pageSize);

                _logger.LogInformation("Retrieved {Count} recent memories for user {UserId} since {SinceDate}", memories.Count(), userId, sinceDate);
                
                return Ok(new { results = memories, since = sinceDate });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recent memories for user {UserId}", userId);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("api/historylayer/memories/user/{userId}/daterange")]
        public async Task<IActionResult> GetMemoriesByUserAndDateRange(
            string userId, 
            [FromQuery] string startDate, 
            [FromQuery] string endDate, 
            [FromQuery] int? page = null, 
            [FromQuery] int? pageSize = null)
        {
            try
            {
                var filters = FilterHelpers.And(
                    FilterHelpers.UserIdFilter(userId),
                    FilterHelpers.CreatedAtFilter(gte: startDate, lte: endDate)
                );
                
                var memories = await _historyLayerService.GetMemoriesAsync(filters, null, page, pageSize);

                _logger.LogInformation("Retrieved {Count} memories for user {UserId} between {StartDate} and {EndDate}", 
                    memories.Count(), userId, startDate, endDate);
                
                return Ok(new { 
                    results = memories, 
                    dateRange = new { start = startDate, end = endDate } 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting memories for user {UserId} in date range {StartDate} to {EndDate}", 
                    userId, startDate, endDate);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // Request DTOs
        public record AddMemoryRequest(
            string UserMessage,
            string UserId,
            string? AssistantResponse = null
        );

        public record SearchMemoryRequest(
            string Query,
            string UserId,
            int? Limit = 5
        );

        public record GetMemoriesV2Request(
            object Filters,
            string[]? Fields = null,
            int? Page = null,
            int? PageSize = null,
            bool? EnableGraph = null
        );
    }
}
