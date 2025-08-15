namespace TrackmaniaWebsiteProject.Models;

public class Users
{
    public int UserId { get; set; }
    public string Email { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public DateTime CreatedAt { get; set; }

    public Users(int userId, string email, string username, string password, DateTime createdAt)
    {
        UserId = userId;
        Email = email;
        Username = username;
        Password = password;
        CreatedAt = createdAt;
    }
}
