using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibraryAuthApi.DTOs;
using LibraryAuthApi.Models;
using System.Linq;

namespace LibraryAuthApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BooksController : ControllerBase
    {
        private readonly LibraryManagementDbContext _context;

        public BooksController(LibraryManagementDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetBooks()
        {
            var books = await _context.Books
                .Include(b => b.Category)
                .Include(b => b.Author)
                .OrderByDescending(b => b.CreatedAt)
                .Select(b => new
                {
                    b.Id,
                    b.Title,
                    b.Description,
                    b.CategoryId,
                    b.AuthorId,
                    b.PublishedYear,
                    b.CreatedAt,
                    b.ImageUrl,
                    CategoryName = b.Category.Name,
                    AuthorName = b.Author.Name,
                })
                .ToListAsync();

            return Ok(books);
        }
        
        [HttpPost]
        public async Task<IActionResult> CreateBook([FromBody] CreateBookDto dto)
        {
            var newBook = new Book
            {
                Title = dto.Title,
                Description = dto.Description,
                CategoryId = dto.CategoryId,
                AuthorId = dto.AuthorId,
                PublishedYear = dto.PublishedYear,
                CreatedAt = DateTime.UtcNow,
                ImageUrl = dto.ImageUrl
            };
            var newBookInventory = new BookInventory
            {
                Book = newBook,
                TotalCopies = dto.totalCopies,
                AvailableCopies = dto.totalCopies
            };
            _context.Books.Add(newBook);
            _context.BookInventories.Add(newBookInventory);

            await _context.SaveChangesAsync();

            return Ok();
        }

   

        // update
        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateBook(int id, [FromBody] BookUpdateDto updatedBook)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null)

                return NotFound("Không tìm thấy sách");
            // Cập nhật thuộc tính
            book.Title = updatedBook.Title;
            book.PublishedYear = updatedBook.PublishedYear;
            book.ImageUrl = updatedBook.ImageUrl;

            await _context.SaveChangesAsync();
            return Ok("Cập nhật thành công");
        }



        //Delete

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBook(int id)
        {
            var book = await _context.Books
        .Include(b => b.BookInventory)
        .Include(b => b.BorrowRecords)
        .Where(b => b.Id == id)
        .Select(b => b).FirstOrDefaultAsync();
            if (book == null)
                return NotFound();

            if(book.BookInventory != null)
                _context.BookInventories.Remove(book.BookInventory);

            if (book.BorrowRecords.Count() > 0)
                _context.BorrowRecords.RemoveRange(book.BorrowRecords);

            _context.Books.Remove(book);
            await _context.SaveChangesAsync();

            return Ok();
        }


        // GET: api/Books/with-inventory
        [HttpGet("with-inventory")]
        public async Task<IActionResult> GetBooksWithInventory()
        {
            var books = await _context.Books
                .Include(b => b.BookInventory)
                .Include(b => b.Author)
                .ToListAsync();

            var result = books.Select(b => new BookInventoryDto
            {
                Id = b.Id,
                Title = b.Title,
                AuthorName = b.Author != null ? b.Author.Name : "Unknown",
                PublishedYear = b.PublishedYear,
                ImageUrl = b.ImageUrl,
                TotalCopies = b.BookInventory?.TotalCopies ?? 0,
                AvailableCopies = b.BookInventory?.AvailableCopies ?? 0
            }).ToList();

            return Ok(result);
        }

        //upload ảnh 
        [HttpPost("upload")]
        public async Task<IActionResult> UploadImage(IFormFile image)
        {
            if (image == null || image.Length == 0)
                return BadRequest("No file selected");


            var uploadsFolder = Path.Combine("wwwroot", "uploads");

            // Ensure folder exists
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var fileName = Path.GetFileName(image.FileName);
            var path = Path.Combine("wwwroot/uploads", fileName);

            using (var stream = new FileStream(path, FileMode.Create))
            {
                await image.CopyToAsync(stream);
            }

            var imageUrl = $"https://localhost:7250/uploads/{fileName}";
            return Ok(new { imageUrl });
        }

    }
}
