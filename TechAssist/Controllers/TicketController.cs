using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Runtime.Intrinsics.Arm;
using System.Security.Claims;
using TechAssist.Data;
using TechAssist.DOT;
using TechAssist.Models;

namespace TechAssist.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class TicketController : Controller
    {
        private readonly AppDbContext _db;
        public TicketController(AppDbContext db) { _db = db; }

        [Authorize]
        [HttpPost("CreateTicket")]
        public async Task<ActionResult> CreateTicket([FromBody] TicketCreateDOT dot) 
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value);
            //Console.WriteLine(userId);
          
            var newticket = new Ticket
            {
                Title = dot.Title,
                Description = dot.Description,
                CreatorId = userId,
                ProductId = dot.ProductId,
                Priority = dot.Priority,
                Status = "Open",
                CreatedAt = DateTime.UtcNow,
            };
            _db.AddAsync(newticket);
            await _db.SaveChangesAsync();
            return Ok(newticket);
        }


        [Authorize]
        [HttpGet("GetAllTicket")]
        public async Task<ActionResult<IEnumerable<TicketResponseDTO>>> GetAllTicket()
        {
            var tickets = await _db.Tickets
        .Include(t => t.Creator)
        .Include(t => t.AssignedEngineer)
        .Include(t => t.Product)
        .OrderByDescending(t => t.CreatedAt)
        .Select(t => new TicketResponseDTO
        {
            Id = t.Id,
            Title = t.Title,
            Description = t.Description,
            Priority = t.Priority,
            Status = t.Status,
            CreatedAt = t.CreatedAt,

            Creator = t.Creator == null ? null : new TicketResponseDTO.UserDTO
            {
                Id = t.Creator.Id,
                FullName = t.Creator.FullName,
                Email = t.Creator.Email
            },

            AssignedEngineer = t.AssignedEngineer == null ? null : new TicketResponseDTO.UserDTO
            {
                Id = t.AssignedEngineer.Id,
                FullName = t.AssignedEngineer.FullName,
                Email = t.AssignedEngineer.Email
            },

            Product = t.Product == null ? null : new TicketResponseDTO.ProductDTO
            {
                Id = t.Product.Id,
                Name = t.Product.Name
            }
        }).ToListAsync();

            return Ok(tickets);

        }

        [Authorize(Roles = "Admin")]
        [HttpGet("AdminDashboardTicket")]
        public async Task<IActionResult> AdminDashboardTicket()
        {
            var tickets = await _db.Tickets
                .Include(t => t.Creator)
                .Include(t => t.AssignedEngineer)
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => new
                {
                    t.Id,
                    t.Title,
                    t.Priority,
                    t.Status,
                    CreatedAt = t.CreatedAt,
                    Creator = t.Creator.FullName,
                    Engineer = t.AssignedEngineer != null ? t.AssignedEngineer.FullName : "Unassigned"
                })
                .ToListAsync();

            return Ok(tickets);
        }

        [Authorize(Roles = "Engineer")]
        [HttpGet("GetEngineerTickets")]
        public async Task<IActionResult> GetEngineerTickets()
        {
            var engineerId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var tickets = await _db.Tickets
                .Where(t => t.AssignedEngineerId == engineerId)
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => new
                {
                    t.Id,
                    t.Title,
                    t.Description,
                    t.Priority,
                    t.Status,
                    CreatedAt = t.CreatedAt,
                    Creator = t.Creator.FullName
                })
                .ToListAsync();

            return Ok(tickets);
        }



        [Authorize]
        [HttpGet("GetTicketBySpecificUser")]
        public async Task<ActionResult<IEnumerable<TicketResponseDTO>>> GetTicketBySpecificUser()
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value);

            var tickets = await _db.Tickets
        .Include(t => t.Creator)
        .Include(t => t.AssignedEngineer)
        .Include(t => t.Product)
        .OrderByDescending(t => t.CreatedAt)
        .Select(t => new TicketResponseDTO
        {
            Id = t.Id,
            Title = t.Title,
            Description = t.Description,
            Priority = t.Priority,
            Status = t.Status,
            CreatedAt = t.CreatedAt,

            Creator = t.Creator == null ? null : new TicketResponseDTO.UserDTO
            {
                Id = t.Creator.Id,
                FullName = t.Creator.FullName,
                Email = t.Creator.Email
            },

            AssignedEngineer = t.AssignedEngineer == null ? null : new TicketResponseDTO.UserDTO
            {
                Id = t.AssignedEngineer.Id,
                FullName = t.AssignedEngineer.FullName,
                Email = t.AssignedEngineer.Email
            },

            Product = t.Product == null ? null : new TicketResponseDTO.ProductDTO
            {
                Id = t.Product.Id,
                Name = t.Product.Name
            }
        }).Where(t => t.Creator.Id == userId).ToListAsync();

            return Ok(tickets);

        }

        [HttpGet("GetTicketById/{id}")]
        public async Task<ActionResult> GetTicketById(int id)
        {
            var ticket = await _db.Tickets
                .Include(t => t.Creator)
                .Include(t => t.AssignedEngineer)
                .Include(t => t.Product)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (ticket == null) return BadRequest("Ticket is not exist"); 
            return Ok(ticket);
        }

        [Authorize(Roles = "Admin")]
        [HttpPatch("{ticketId}/assign")]
        public async Task<IActionResult> AssignTicket(int ticketId,[FromBody] int engineerId)
        {
            var ticket = await _db.Tickets.FindAsync(ticketId);
            if (ticket == null) return NotFound("Ticket not found");

            ticket.AssignedEngineerId = engineerId;
            ticket.Status = "InProgress";
            ticket.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return Ok("Engineer is assign!");
        }

        [HttpPatch("{id}/status")]
        public async Task<ActionResult> UpdateTicketStatus(int id, [FromBody] string newStatus)
        {
            var ticket = await _db.Tickets.FirstOrDefaultAsync(t => t.Id == id);
            if (ticket == null) return BadRequest("This Ticket is not exist !");

            var validStatus = new List<string>
            {
                "Open","In Progress","Resolved","Closed"
            };

            if(string.IsNullOrEmpty(newStatus) || !validStatus.Contains(newStatus)){
                return BadRequest("Invalid Status! Valid options are Open, In Progress, Resolves, Closed");
            }

            var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role).Value;

            if (userRole.Equals(Role.Engineer) || userRole.Equals(Role.User))   { return BadRequest("unauthorized User!"); }

            ticket.Status = newStatus;
            ticket.UpdatedAt = DateTime.UtcNow;

            //_db.Tickets.Update(ticket);
            await _db.SaveChangesAsync();
            return Ok(new
            {
                message = "Status Updated Successfully",
                ticketid = ticket.Id,
                newStatus
            });
        }

        [HttpDelete("delete/{id}")]
        public async Task<ActionResult> DeleteTicket(int id)
        {
            var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role).Value;
            if (userRole != Role.Admin.ToString()){ return BadRequest(new { message = "Unauthorized User " }); }

            var ticket = await _db.Tickets.FirstOrDefaultAsync(t => t.Id == id);
            if (ticket == null) return BadRequest("Ticket is not exist");

            _db.Tickets.Remove(ticket);
            await _db.SaveChangesAsync();

            return Ok(new
            {
                message = "Ticket deleted successfully !",
                ticketid = id
            });
        }
    }
}
