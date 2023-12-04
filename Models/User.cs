namespace AuthWebApi.Models
{
    public class User
    {
        public int Id { get; set; }
        public  string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime? TokenCreated { get; set; }
        public DateTime? TokenExpires { get; set; }
        public string? RefreshToken { get; set; }
        public string? StripeCustomerId { get; set; }
    }
}
