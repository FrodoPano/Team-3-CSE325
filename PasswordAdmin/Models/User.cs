using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace PasswordAdmin.Models;

[Table("Users")]
public class User : BaseModel
{
    [PrimaryKey("id")]
    public int Id { get; set; }
    
    [Column("created_at")]
    public string CreatedAt { get; set; } = string.Empty;
    
    [Column("first_name")]
    public string? FirstName { get; set; }
    
    [Column("last_name")]
    public string LastName { get; set; } = string.Empty;
    
    [Column("password_hash")]
    public string PasswordHash { get; set; } = string.Empty;
    
    [Column("email")]
    public string Email { get; set; } = string.Empty;
    
    [Column("phone_number")]
    public int? PhoneNumber { get; set; }
    
    [Column("language")]
    public string? LanguagePreferance { get; set; }
    
    [Column("theme")]
    public string? Theme { get; set; }
    
    [Column("two_factor_enabled")]
    public bool TwoFactorEnabled { get; set; }
    
    [Column("email_notifications")]
    public bool EmailNotifications { get; set; }
}
