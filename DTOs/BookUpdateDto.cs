namespace LibraryAuthApi.DTOs
{
    public class BookUpdateDto
    {
        public string? Title { get; set; }
        public int PublishedYear { get; set; }
        public string? ImageUrl { get; set; }
    }
}
