using System.ComponentModel.DataAnnotations;

namespace ShiraSubathonTracker.DAL.Entities.Users;

public class User
{
    [Key]
    [MaxLength(50)]
    public required string Username { get; set; }
    [MaxLength(256)]
    public required string Password { get; set; }
    [MaxLength(200)]
    public required string SecretKey { get; set; }
}