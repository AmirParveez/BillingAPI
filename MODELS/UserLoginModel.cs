namespace api.Models
{
    public class UserLoginModel
    {
        public int UserId { get; set; }
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
        public string Current_Session { get; set; } = "";
    }
}
