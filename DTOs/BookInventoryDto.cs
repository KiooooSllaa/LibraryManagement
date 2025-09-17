namespace LibraryAuthApi.DTOs
{
    public class BookInventoryDto
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? AuthorName { get; set; }
        public int? PublishedYear { get; set; }
        public string? ImageUrl { get; set; }
        public int? TotalCopies { get; set; }
        public int? AvailableCopies { get; set; }
    }
}
