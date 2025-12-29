using TechAssist.Models;
using TechAssist.DOT;

namespace TechAssist.Services.Interface
{
    public interface IAuthService
    {
        Task<User> RegisterAsync(RegisterDto dto);
        Task<string> LoginAsync(LoginRequestDto dto); // returns JWT token
        Task<User> GetUserByEmailAsync(string email);
    }
}
