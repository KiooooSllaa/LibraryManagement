using System.Security.Claims;
using LibraryAuthApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryAuthApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BorrowRecordsController : ControllerBase
    {
        private readonly LibraryManagementDbContext _context;
        public BorrowRecordsController(LibraryManagementDbContext context)
        {
            _context = context;
        }

        // gửi yêu cầu mượn sách
        [HttpPost("Borrow")]
        public async Task<IActionResult> BorrowBook([FromBody] BorrowRequest request)
        {
            if (request == null || request.BookId == 0 || request.UserId == 0)
                return BadRequest("Yêu cầu không hợp lệ");

            var uid = request.UserId;


            var isBorrowed = await _context.BorrowRecords
                .AnyAsync(b => b.BookId == request.BookId && b.ReturnDate == null && b.UserId == uid);

            if (isBorrowed)
                return BadRequest("Sách hiện đang được mượn");

            var isOverdue = await _context.BorrowRecords
                .AnyAsync(b => b.ReturnDate == null && b.LibrarianId != null && b.DueDate < DateTime.Now && b.UserId == uid);

            if (isOverdue)
                return BadRequest("Bạn có một cuốn sách quá hạn, bạn không thể mượn ngay bây giờ.");

            var isLost = await _context.BorrowRecords
                .AnyAsync(b => b.ReturnDate == null && b.Status.Equals("Lost") && b.UserId == uid);

            if(isLost)
                return BadRequest("Bạn đã làm hỏng hoặc làm mất 1 cuốn sách, vui lòng đền trước khi mượn sách tiếp.");

            var borrowRecord = new BorrowRecord
            {
                UserId = request.UserId,
                BookId = request.BookId,
                BorrowDate = request.BorrowDate.ToDateTime(TimeOnly.MinValue),
                DueDate = request.DueDate.ToDateTime(TimeOnly.MinValue)
            };

            _context.BorrowRecords.Add(borrowRecord);
            await _context.SaveChangesAsync();

            return Ok(borrowRecord);
        }

        // danh sách sách đã mượn của người dùng
        [HttpGet("User/{userId}")]
        public async Task<ActionResult<IEnumerable<object>>> GetBorrowedBooks(int userId)
        {
            var records = await _context.BorrowRecords
                .Include(b => b.Book)
                .Where(b => b.UserId == userId)
                .Select(b => new
                {
                    b.Id,
                    b.BookId,
                    b.Book.Title,
                    b.BorrowDate,
                    b.DueDate,
                    b.ReturnDate,
                    Status = b.ReturnDate == null ?
                        (b.DueDate < DateTime.Now ? "Quá Hạn" : (b.LibrarianId == null ? "Chưa duyệt" : (b.LibrarianId != -1 ? "Đang mượn" : (b.Status.Equals("Lost") ? "Mất Sách" : "Từ chối duyệt")))) :
                        "Đã Trả"
                })
                .ToListAsync();


            return Ok(records);
        }

        // ngày trả sách
        [HttpPut("Return/{id}")]
        public async Task<IActionResult> ReturnBook(int id)
        {
            var record = await _context.BorrowRecords.FindAsync(id);
            if (record == null)
                return NotFound();

            record.ReturnDate = DateTime.Now;
            await _context.SaveChangesAsync();

            return Ok(record);
        }
    }

    public class BorrowRequest
    {
        public int UserId { get; set; }
        public int BookId { get; set; }
        public DateOnly BorrowDate { get; set; }
        public DateOnly DueDate { get; set; }
    }
}
