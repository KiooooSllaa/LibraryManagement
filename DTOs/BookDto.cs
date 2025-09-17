namespace LibraryAuthApi.DTOs
{
    public class BookDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public int? CategoryId { get; set; }  
        public int? AuthorId { get; set; }    
        public int? PublishedYear { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? ImageUrl { get; set; }
    }
}
