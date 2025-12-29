using Microsoft.AspNetCore.Http.HttpResults;
using TechAssist.Models;
using TechAssist.DOT;
using TechAssist.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;


namespace TechAssist.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : Controller
    {
        private readonly IAuthService _auth;
        public AuthController(IAuthService auth) { _auth = auth; }

        [HttpPost("register")]
        public async Task<ActionResult> Register([FromBody] RegisterDto dto)
        {
            var user = await _auth.RegisterAsync(dto);
            return Ok(new { user.Id, user.Email, user.FullName });
        }

        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] LoginRequestDto dto)
        {

            // Find the user
            var user = await _auth.GetUserByEmailAsync(dto.Email);
            if (user == null)
                return Unauthorized(new { message = "Invalid credentials Username and password" });

            var token = await _auth.LoginAsync(dto);

            if (token == null)
            {
                return Unauthorized(new { message = "Invalid credentials Username and password" });
            }

            return Ok(new
            {
                message = "Login successful",
                Token = token,
                Role = user.Role.ToString()
            });
        }
    }
}
