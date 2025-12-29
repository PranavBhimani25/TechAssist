using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TechAssist.Data;
using TechAssist.Models;

namespace TechAssist.Controllers
{
    public class UpdateProfileDto
    {
        public string FullName { get; set; }
        public string Email { get; set; }
    }

    [Route("api/[controller]")]
    [ApiController]
    public class CommonController : ControllerBase
    {
        private readonly AppDbContext _appDbContext;

        public CommonController(AppDbContext appDbContext   )
        {
            _appDbContext = appDbContext;
        }

        [HttpGet("GetDashboardStats")]
        public async Task<IActionResult> GetDashboardStats()
        {
            var totalTickets = _appDbContext.Tickets.Count();
            var engineers = _appDbContext.Users.Count(e => e.Role == Role.Engineer);
            var activeUsers = _appDbContext.Users.Count(u => u.Role == Role.User);
            var openTickets = await _appDbContext.Tickets.CountAsync(o => o.Status == "Open");
            var inProgressTickets = await _appDbContext.Tickets.CountAsync(i => i.Status == "InProgress");
            var closedTickets = await _appDbContext.Tickets.CountAsync(c => c.Status == "Closed");
            var totalUsers = await _appDbContext.Users.CountAsync();

            return Ok(new {
                totalTickets,
                openTickets,
                inProgressTickets,
                closedTickets,
                engineers, 
                totalUsers,
                activeUsers
            });
        }

        [Authorize]
        [HttpGet("GetUserDashboardStats")]
        public async Task<IActionResult> GetUserDashboardStats()
        {

            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value);

            var totalTickets = await _appDbContext.Tickets.CountAsync(i => i.CreatorId == userId);
            var openTickets = await _appDbContext.Tickets.CountAsync(o => o.Status == "Open" && o.CreatorId == userId);
            var inProgressTickets = await _appDbContext.Tickets.CountAsync(i => i.Status == "InProgress" && i.CreatorId == userId);
            var closedTickets = await _appDbContext.Tickets.CountAsync(c => c.Status == "Closed" && c.CreatorId == userId);

            return Ok(new {
                totalTickets,
                openTickets,
                inProgressTickets, 
                closedTickets
            });
        }

        [Authorize(Roles = "Engineer")]
        [HttpGet("GetEngineerDashboard")]
        public async Task<IActionResult> GetEngineerDashboard()
        {
            var engineerId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var assignedTickets = await _appDbContext.Tickets.CountAsync(t => t.AssignedEngineerId == engineerId);
            var openTickets = await _appDbContext.Tickets.CountAsync(t =>
                t.AssignedEngineerId == engineerId && t.Status == "Open");

            var inProgressTickets = await _appDbContext.Tickets.CountAsync(t =>
                t.AssignedEngineerId == engineerId && t.Status == "InProgress");

            var closedTickets = await _appDbContext.Tickets.CountAsync(t =>
                t.AssignedEngineerId == engineerId && t.Status == "Closed");

            return Ok(new
            {
                assignedTickets,
                openTickets,
                inProgressTickets,
                closedTickets
            });
        }



        [HttpGet("GetProduct")]
        public async Task<IActionResult> GetProduct()
        {
            var products = await _appDbContext.Products.ToListAsync();
            return Ok(products);
        }

        [HttpGet("GetAllEngineerName")]
        public async Task<IActionResult> GetAllEngineerName()
        {
            var engName = await _appDbContext.Users.Where(x => x.Role == Role.Engineer).ToListAsync();

            return Ok(engName);

        }

        [HttpGet("GetProfile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var user = await _appDbContext.Users.Where(u => u.Id == userId)
                .Select(u => new
                {
                    u.Id,
                    u.FullName,
                    u.Email,
                    Role = u.Role.ToString(),
                    u.CreatedAt
                }).FirstOrDefaultAsync();

            return Ok(user);
        }

        [HttpPut("UpDateProfile")]
        public async Task<IActionResult> UpDateProfile([FromBody] UpdateProfileDto dto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var user = await _appDbContext.Users.FindAsync(userId);

            if(user == null) return NotFound();

            user.FullName = dto.FullName;
            user.Email = dto.Email;

            await _appDbContext.SaveChangesAsync();
            return Ok(new {message = "Profile is Updated!"});
        }
    }
}
