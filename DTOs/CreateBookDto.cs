namespace LibraryAuthApi.DTOs
{
    public class CreateBookDto
    {
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public int CategoryId { get; set; }
        public int totalCopies { get; set; }
        public int AuthorId { get; set; }
        public int PublishedYear { get; set; }
        public string? ImageUrl { get; set; }

    }
}
