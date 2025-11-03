using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ChatSupport.Data;
using ChatSupport.Models;

namespace ChatSupport.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly ChatDbContext _db;
        private readonly ILogger<ChatController> _logger;

        public ChatController(ChatDbContext db, ILogger<ChatController> logger)
        {
            _db = db;
            _logger = logger;
        }

        // GET: api/chat/sessions
        [HttpGet("sessions")]
        public async Task<ActionResult<IEnumerable<ChatSession>>> GetAllSessions()
        {
            try
            {
                var sessions = await _db.ChatSessions
                    .OrderByDescending(s => s.CreatedAt)
                    .ToListAsync();
                return Ok(sessions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetAllSessions hatası");
                return StatusCode(500, "Bir hata oluştu");
            }
        }

        // GET: api/chat/sessions/open
        [HttpGet("sessions/open")]
        public async Task<ActionResult<IEnumerable<ChatSession>>> GetOpenSessions()
        {
            try
            {
                var sessions = await _db.ChatSessions
                    .Where(s => !s.ClaimedBySupport)
                    .OrderByDescending(s => s.CreatedAt)
                    .ToListAsync();
                return Ok(sessions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetOpenSessions hatası");
                return StatusCode(500, "Bir hata oluştu");
            }
        }

        // GET: api/chat/sessions/claimed
        [HttpGet("sessions/claimed")]
        public async Task<ActionResult<IEnumerable<ChatSession>>> GetClaimedSessions()
        {
            try
            {
                var sessions = await _db.ChatSessions
                    .Where(s => s.ClaimedBySupport)
                    .OrderByDescending(s => s.CreatedAt)
                    .ToListAsync();
                return Ok(sessions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetClaimedSessions hatası");
                return StatusCode(500, "Bir hata oluştu");
            }
        }

        // GET: api/chat/{chatId}
        [HttpGet("{chatId}")]
        public async Task<ActionResult<ChatSession>> GetSession(string chatId)
        {
            try
            {
                var session = await _db.ChatSessions.FindAsync(chatId);
                if (session == null)
                    return NotFound();

                return Ok(session);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetSession hatası - ChatId: {ChatId}", chatId);
                return StatusCode(500, "Bir hata oluştu");
            }
        }

        // GET: api/chat/{chatId}/messages
        [HttpGet("{chatId}/messages")]
        public async Task<ActionResult<IEnumerable<ChatMessage>>> GetMessages(string chatId)
        {
            try
            {
                var messages = await _db.ChatMessages
                    .Where(m => m.ChatId == chatId)
                    .OrderBy(m => m.Timestamp)
                    .ToListAsync();
                return Ok(messages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetMessages hatası - ChatId: {ChatId}", chatId);
                return StatusCode(500, "Bir hata oluştu");
            }
        }

        // DELETE: api/chat/{chatId}
        [HttpDelete("{chatId}")]
        public async Task<ActionResult> DeleteSession(string chatId)
        {
            try
            {
                var session = await _db.ChatSessions.FindAsync(chatId);
                if (session == null)
                    return NotFound();

                _db.ChatSessions.Remove(session);
                await _db.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DeleteSession hatası - ChatId: {ChatId}", chatId);
                return StatusCode(500, "Bir hata oluştu");
            }
        }

        // GET: api/chat/stats
        [HttpGet("stats")]
        public async Task<ActionResult> GetStats()
        {
            try
            {
                var totalSessions = await _db.ChatSessions.CountAsync();
                var openSessions = await _db.ChatSessions.CountAsync(s => !s.ClaimedBySupport);
                var claimedSessions = await _db.ChatSessions.CountAsync(s => s.ClaimedBySupport);
                var totalMessages = await _db.ChatMessages.CountAsync();

                return Ok(new
                {
                    totalSessions,
                    openSessions,
                    claimedSessions,
                    totalMessages
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetStats hatası");
                return StatusCode(500, "Bir hata oluştu");
            }
        }
    }
}

