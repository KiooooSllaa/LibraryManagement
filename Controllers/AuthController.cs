using LibraryAuthApi.Models;
using LibraryAuthApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using LibraryAuthApi.DTOs;
using System.Net;

namespace LibraryAuthApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly LibraryManagementDbContext _context;
        private readonly JwtService _jwtService;
        private readonly EmailService _emailService;
        private readonly PasswordHasher<User> _hasher;

        public AuthController(
            LibraryManagementDbContext context,
            JwtService jwtService,
            EmailService emailService)
        {
            _context = context;
            _jwtService = jwtService;
            _emailService = emailService;
            _hasher = new PasswordHasher<User>();
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Password))
            {
                return BadRequest("Password is required.");
            }

            var userExists = await _context.Users.AnyAsync(u => u.Email == dto.Email);
            if (userExists)
            {
                return BadRequest("Email already exists.");
            }

            var hasher = new PasswordHasher<User>();
            var user = new User
            {
                FullName = dto.FullName,
                Email = dto.Email,
                RoleId = 3,
              
                CreatedAt = DateTime.UtcNow
            };
            user.PasswordHash = hasher.HashPassword(user, dto.Password);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return StatusCode(201);
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest("Dữ liệu không hợp lệ.");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null || string.IsNullOrEmpty(user.PasswordHash))
                return Unauthorized("Email hoặc mật khẩu không đúng.");

            var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);
            if (result != PasswordVerificationResult.Success)
                return Unauthorized("Email hoặc mật khẩu không đúng.");

            var token = _jwtService.GenerateToken(user);

            return Ok(new
            {
                token,
                user.Id,
                user.FullName,
                user.Email,
                user.RoleId
            });

        }


        [HttpPost("forgot")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (dto == null || string.IsNullOrWhiteSpace(dto.Email))
                return BadRequest("Email không được để trống.");

            var email = dto.Email.Trim().ToLower();

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email);

            if (user == null)
                return NotFound("Email không tồn tại.");

            var oldTokens = _context.PasswordResetTokens
                .Where(p => p.UserId == user.Id && (p.IsUsed == false)); 
            _context.PasswordResetTokens.RemoveRange(oldTokens);

            // Tạo token mới
            var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            var resetToken = new PasswordResetToken
            {
                UserId = user.Id,
                Token = token,
                Expiration = DateTime.UtcNow.AddMinutes(30),
                IsUsed = false
            };

            _context.PasswordResetTokens.Add(resetToken);
            await _context.SaveChangesAsync();

            var encodedToken = WebUtility.UrlEncode(token);
            var resetLink = $"https://localhost:7250/reset-password.html?token={encodedToken}";


            await _emailService.SendAsync(user.Email, "Đặt lại mật khẩu", $"Click vào đây để đặt lại mật khẩu: {resetLink}");

            return Ok("Đã gửi email khôi phục mật khẩu.");
        }
        [HttpPost("reset")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            var rawToken = System.Net.WebUtility.UrlDecode(dto.Token);
        
            var tokenEntry = await _context.PasswordResetTokens
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Token == rawToken && p.IsUsed == false);

            if (tokenEntry == null)
            {
                return BadRequest("Token không hợp lệ hoặc đã hết hạn.");
            }

            if (tokenEntry.Expiration < DateTime.UtcNow)
            {
              
                return BadRequest("Token không hợp lệ hoặc đã hết hạn.");
            }

            if (tokenEntry.User == null)
            {
                return BadRequest("Người dùng không tồn tại.");
            }

            tokenEntry.User.PasswordHash = _hasher.HashPassword(tokenEntry.User, dto.NewPassword);
            tokenEntry.IsUsed = true;

            await _context.SaveChangesAsync();
            return Ok("Đặt lại mật khẩu thành công.");
        }


    }
}
