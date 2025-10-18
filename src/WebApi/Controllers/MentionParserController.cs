using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [ApiController]
    public class MentionParserController : ControllerBase
    {
        private readonly IMentionParserService _mentionParserService;

        public MentionParserController(
            IMentionParserService mentionParserService)
        {
            _mentionParserService = mentionParserService;
        }

        [HttpPost("api/mention/parse")]
        public async Task<IActionResult> Parse([FromBody] ParseRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest(new { error = "Message is required" });
            }

            var result = await _mentionParserService.ParseAsync(request.Message);
            return Ok(result);
        }

        public sealed record ParseRequest(
            string Message
        );
    }
}

