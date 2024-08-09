using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShiraSubathonTracker.DAL.Entities.Users;

public class JwtToken
{
    [Key]
    [MaxLength(50)]
    public required string Username { get; set; }
    
    [ForeignKey(nameof(Username))]
    public User user { get; set; }
    
    public required string Token { get; set; }
    public required bool IsBlocked { get; set; }
}