namespace LibraryAuthApi.Services
{
    public class EmailSettings
    {
        public string SenderEmail { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty; 
        public string SmtpServer { get; set; } = "smtp.gmail.com";
        public int Port { get; set; } = 587;
    }

}
