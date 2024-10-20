namespace MyApiApp.Models
{
    public class History
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public required string HttpMethod { get; set; }
        public required string Path { get; set; }
        public DateTime Timestamp { get; set; }
        public required string QueryString { get; set; }
        public required string BodyContent { get; set; }
    }
}
