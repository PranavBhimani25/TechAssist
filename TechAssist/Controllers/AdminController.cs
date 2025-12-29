using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using TechAssist.Data;
using TechAssist.DOT;
using TechAssist.Models;

namespace TechAssist.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : Controller
    {
        private readonly AppDbContext _db;
        public AdminController(AppDbContext db) { _db = db; }

        [HttpPost("CreateEngineer")]
        public async Task<ActionResult> CreateEngineer([FromBody] RegisterDto dto)
        {
            if (dto == null) {
                return BadRequest(new { message = "Invalid data." });
            }

            var user = await _db.Users.Where(x => x.Email == dto.Email).ToListAsync();
            if (user == null) BadRequest("User is allready Exist!");

            var checkAuth = User.FindFirst(System.Security.Claims.ClaimTypes.Role).Value;

            if (checkAuth != "Admin")  return BadRequest("Unauthozied User");

            Console.WriteLine(checkAuth);

            var neweng = new User
            {
                FullName = dto.FullName,
                Email = dto.Email,
                PasswordHash = HashPassword(dto.Password),
                Role = Role.Engineer,
                CreatedAt = DateTime.UtcNow
            };

            _db.Users.Add(neweng);
            await _db.SaveChangesAsync();
            return Ok(neweng);
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _db.Users.ToListAsync());
        }


        [HttpGet("GetAllUser")]
        public async Task<IActionResult> GetAllUser()
        {
            return Ok(await _db.Users.Where(x => x.Role == Role.User).ToListAsync());
        }

        [HttpGet("GetAllEngineer")]
        public async Task<IActionResult> GetAllEngineer()
        {
            return Ok(await _db.Users.Where(x => x.Role == Role.Engineer).ToListAsync());
        }

        // TODO: replace with secure hashing (e.g., BCrypt)
        private string HashPassword(string plain) => Convert.ToBase64String(SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(plain)));


    }
}
