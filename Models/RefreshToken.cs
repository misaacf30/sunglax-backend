namespace AuthWebApi.Models
{
    public class RefreshToken
    {
        public string? Token { get; set; }      // required ??
        public DateTime Created { get; set; } = DateTime.Now;
        public DateTime Expires { get; set; }
    }
}
