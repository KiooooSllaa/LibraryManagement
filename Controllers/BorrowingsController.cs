using LibraryAuthApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LibraryAuthApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BorrowingsController : ControllerBase
    {
        private readonly LibraryManagementDbContext _context;

        public BorrowingsController(LibraryManagementDbContext context)
        {
            _context = context;
        }

        private int GetCurrentUserId()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var records = await _context.BorrowRecords
                .Include(b => b.User)
                .Include(b => b.Book)
                .Select(b => new
                {
                    b.Id,
                    UserName = b.User.FullName, 
                    BookTitle = b.Book.Title,
                    b.BorrowDate,
                    b.ReturnDate,
                    b.LibrarianId,
                    Overdue = b.LibrarianId != null && b.ReturnDate == null && b.DueDate < DateTime.Now,
                    b.Status
                })
                .OrderByDescending(b => b.BorrowDate)
                .ToListAsync();

            return Ok(records);
        }
        // từ chối mượn sách
        [HttpPost("reject/{id}")]
        public async Task<IActionResult> rejectBorrow(int id)
        {
            var record = await _context.BorrowRecords
                .Include(b => b.Book)
                .Include(b => b.Book.BookInventory)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (record == null || record.LibrarianId != null)
                return BadRequest("Yêu cầu không hợp lệ.");

            record.LibrarianId = -1;
            record.Status = "Rejected";

            await _context.SaveChangesAsync();
            return Ok("Đã từ chối mượn sách.");
        }
        [HttpPost("pay-up/{id}")]
        public async Task<IActionResult> PayUp(int id)
        {
            var record = await _context.BorrowRecords
                .Include(b => b.Book)
                .Include(b => b.Book.BookInventory)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (record == null || record.ReturnDate != null)
                return BadRequest("Yêu cầu không hợp lệ.");


            record.Status = "Paid";
            record.ReturnDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok("Đã duyệt bồi thường sách.");
        }
        [HttpPost("lost-book/{id}")]
        public async Task<IActionResult> LostBook(int id)
        {
            var record = await _context.BorrowRecords
                .Include(b => b.Book)
                .Include(b => b.Book.BookInventory)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (record == null || record.ReturnDate != null)
                return BadRequest("Yêu cầu không hợp lệ.");


            record.LibrarianId = -1;
            record.Status = "Lost";
            record.Book.BookInventory.TotalCopies--;

            await _context.SaveChangesAsync();
            return Ok("Đã duyệt mượn sách.");
        }
        // duyệt sách
        [HttpPost("approve/{id}")]
        public async Task<IActionResult> ApproveBorrow(int id)
        {
            var record = await _context.BorrowRecords
                .Include(b => b.Book)
                .Include(b => b.Book.BookInventory)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (record == null || record.LibrarianId != null)
                return BadRequest("Yêu cầu không hợp lệ.");

            if (record.Book.BookInventory.AvailableCopies <= 0)
                return BadRequest("Sách hiện không còn sẵn.");

            int librarianId = GetCurrentUserId();
            record.LibrarianId = librarianId;
            record.Status = "Accepted";
            record.Book.BookInventory.AvailableCopies--;

            await _context.SaveChangesAsync();
            return Ok("Đã duyệt mượn sách.");
        }
        // duyệt trả sách
        [HttpPost("return/{id}")]
        public async Task<IActionResult> ReturnBook(int id)
        {
            var record = await _context.BorrowRecords
                .Include(b => b.Book)
                .Include(b => b.Book.BookInventory)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (record == null || record.ReturnDate != null)
                return BadRequest("Yêu cầu không hợp lệ.");

            record.ReturnDate = DateTime.UtcNow;
            record.Status = "Returned";
            record.Book.BookInventory.AvailableCopies++;

            await _context.SaveChangesAsync();
            return Ok("Đã ghi nhận trả sách.");
        }
    }
}