using Microsoft.AspNetCore.Mvc;
using MyApiApp.Data;
using MyApiApp.Models;
using MyApiApp.Services;

namespace MyApiApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly TokenService _tokenService;
        private readonly PasswordService _passwordService;

        public AuthController(ApplicationDbContext context, TokenService tokenService, PasswordService passwordService)
        {
            _context = context;
            _tokenService = tokenService;
            _passwordService = passwordService;
        }

        [HttpPost("register")]
        public IActionResult Register(User user)
        {
            if (_context.Users.Any(u => u.Username == user.Username))
            {
                return BadRequest("Пользователь с таким именем уже существует.");
            }

            user.PasswordHash = _passwordService.HashPassword(user.PasswordHash, out string salt);
            user.Salt = salt;

            user.Token = _tokenService.GenerateToken(user.Username, user.Id);

            _context.Users.Add(user);
            _context.SaveChanges();

            HttpContext.Session.SetString("UserId", user.Id.ToString());

            return Ok(new { Token = user.Token });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginRequest loginRequest)
        {
            var user = _context.Users.SingleOrDefault(u => u.Username == loginRequest.Username);
            if (user == null || !_passwordService.VerifyPassword(loginRequest.Password, user.PasswordHash, user.Salt))
            {
                return Unauthorized("Неправильный логин или пароль.");
            }

            user.Token = _tokenService.GenerateToken(user.Username, user.Id);
            await _context.SaveChangesAsync();

            HttpContext.Session.SetString("UserId", user.Id.ToString());

            return Ok(new { Token = user.Token });
        }


        [HttpPatch("password")]
        public IActionResult ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (userId == null)
            {
                return Unauthorized("Пользователь не авторизован.");
            }

            var user = _context.Users.Find(int.Parse(userId));
            if (user == null || !_passwordService.VerifyPassword(request.CurrentPassword, user.PasswordHash, user.Salt))
            {
                return Unauthorized("Неправильный текущий пароль.");
            }

            user.PasswordHash = _passwordService.HashPassword(request.NewPassword, out string newSalt);
            user.Salt = newSalt;

            user.Token = _tokenService.GenerateToken(user.Username, user.Id);

            _context.SaveChanges();

            return Ok(new { Token = user.Token });
        }
    }
}
