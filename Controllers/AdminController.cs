using LibraryAuthApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryAuthApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "1")] // role admin
    public class AdminController : ControllerBase
    {
        private readonly LibraryManagementDbContext _context;

        public AdminController(LibraryManagementDbContext context)
        {
            _context = context;
        }

        [HttpGet("book-count")]
        public async Task<IActionResult> GetBookCount()
        {
            var count = await _context.Books.CountAsync();
            return Ok(count);
        }

        [HttpGet("all-role")]
        public async Task<IActionResult> getAllRole()
        {
            var allRoles = await _context.Roles.ToListAsync();

            return Ok(allRoles);
        }

        [HttpGet("top-books")]
        public async Task<IActionResult> GetMostBorrowedBooks()
        {
            var topBooks = await _context.BorrowRecords
                .Where(r => r.LibrarianId != null)
                .GroupBy(r => r.BookId)
                .Select(g => new
                {
                    BookId = g.Key,
                    BorrowCount = g.Count(),
                    Title = g.First().Book.Title
                })
                .OrderByDescending(g => g.BorrowCount)
                .Take(5)
                .ToListAsync();

            return Ok(topBooks);
        }

        [HttpPut("update-role/{userId}")]
        public async Task<IActionResult> UpdateUserRole(int userId, [FromBody] int newRoleId)
        {
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
                return NotFound("User not found.");


            user.RoleId = newRoleId;
            await _context.SaveChangesAsync();

            return Ok("Cập nhật vai trò thành công.");
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _context.Users
                .Include(u => u.Role)
                .Select(u => new
                {
                    u.Id,
                    u.Email,
                    u.FullName,
                    u.RoleId
                })
                .ToListAsync();

            return Ok(users);
        }
    }
}
