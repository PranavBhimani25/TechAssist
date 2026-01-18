using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TechAssist.Data;
using TechAssist.Models;

namespace TechAssist.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TicketRepliesController : ControllerBase
    {
        private readonly AppDbContext _db;
        public TicketRepliesController (AppDbContext db) { _db = db; }

        [Authorize]
        [HttpPost("{id}")]
        public async Task<ActionResult> AddReplies(int id, [FromBody] string messages)
        {

            if (string.IsNullOrWhiteSpace(messages))
                return BadRequest(new { message = "Message cannot be empty" });

            var ticket = await _db.Tickets.FindAsync(id);
            if (ticket == null) return BadRequest("Ticket is not Found");

            var userid = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value);

            var reply = new TicketReply
            {
                TicketId = id,
                AuthorId = userid,
                Message = messages,
                CreatedAt = DateTime.UtcNow,
            };

            _db.TicketReplies.Add(reply);
            await _db.SaveChangesAsync();

            // 👇 Fetch again with author info
            var result = await _db.TicketReplies
                .Include(r => r.Author)
                .Where(r => r.Id == reply.Id)
                .Select(r => new
                {
                    r.Id,
                    r.Message,
                    r.CreatedAt,
                    Author = new
                    {
                        r.Author.Id,
                        r.Author.FullName,
                        r.Author.Role
                    }
                })
                .FirstOrDefaultAsync();

            return Ok(new
            {
                message = "Ticket reply is Added!",
                result
                
            });
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> GetRepliesByTicketId(int id)
        {
            var reply = await _db.TicketReplies
                .Include(r => r.Author)
                .Where(r => r.TicketId == id)
                .Select(r => new
                {
                    r.Id,
                    r.Message,
                    r.CreatedAt,
                    Author = new
                    {
                        r.Author.Id,
                        r.Author.FullName,
                        r.Author.Email,
                        r.Author.Role
                    }
                }).ToListAsync();
            if (reply == null || reply.Count == 0)
                return BadRequest("No replies found for this ticket.");

            return Ok(reply);
        }


        [HttpGet("GetRepliesForUserTicket/{ticketId}")]
        public async Task<IActionResult> GetRepliesForUserTicket(int ticketId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var ticket = await _db.Tickets.FirstOrDefaultAsync(x => x.Id == ticketId && x.CreatorId == userId);

            if (ticket == null) return Forbid();

            var replies  = await _db.TicketReplies
                .Where(r => r.TicketId == ticketId)
                .OrderBy(r => r.CreatedAt)
                .Select(r =>new
                {
                    r.Id ,
                    r.Message,
                    r.CreatedAt,
                    Author = r.Author.FullName,
                    Role = r.Author.Role.ToString()
                }).ToListAsync();
            return Ok(new
            {
                ticket.Id,
                ticket.Title,
                ticket.Status,
                replies
            });
        }

        [HttpGet("GetRepliesForEngTicket/{ticketId}")]
        public async Task<IActionResult> getrepliesforengticket(int ticketId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var ticket = await _db.Tickets.FirstOrDefaultAsync(x => x.Id == ticketId && x.AssignedEngineerId == userId);
            if (ticket == null) return Forbid();
            var replies = await _db.TicketReplies
                .Where(r => r.TicketId == ticketId)
                .OrderBy(r => r.CreatedAt)
                .Select(r => new
                {
                    r.Id,
                    r.Message,
                    r.CreatedAt,
                    Author = r.Author.FullName,
                    Role = r.Author.Role.ToString()
                }).ToListAsync();
            return Ok(new
            {
                ticket.Id,
                ticket.Title,
                ticket.Status,
                replies
            });
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("/api/{replyId}")]
        public async Task<IActionResult> DeleteReply(int replyId)
        {
            var reply = await _db.TicketReplies.FindAsync(replyId);
            if (reply == null)
                return NotFound("Reply not found.");

            _db.TicketReplies.Remove(reply);
            await _db.SaveChangesAsync();

            return Ok(new { Message = "Reply deleted successfully." });
        }
    }
}
