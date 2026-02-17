using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace PasswordAdmin.Models;

[Table("Users")]
public class User : BaseModel
{
    [PrimaryKey("id")]
    public int Id { get; set; }
    
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } 

    [Column("first_name")]
    public string? FirstName { get; set; } = string.Empty;
    
    [Column("last_name")]
    public string LastName { get; set; } = string.Empty;
    
    [Column("password_hash")]
    public string PasswordHash { get; set; } = string.Empty;
    
    [Column("email")]
    public string Email { get; set; } = string.Empty;
    
    [Column("phone_number")]
    public string? PhoneNumber { get; set; }  // Use string for full numbers
    
    [Column("language")]
    public string LanguagePreferance { get; set; } = "en";
    
    [Column("theme")]
    public string Theme { get; set; } = "Light";
    
    [Column("two_factor_enabled")]
    public bool TwoFactorEnabled { get; set; }
    
    [Column("email_notifications")]
    public bool EmailNotifications { get; set; }

    // Clone the user for cancel functionality
    public User Clone()
    {
        return (User)this.MemberwiseClone();
    }

    // Copy values from another user
    public void CopyFrom(User other)
    {
        if (other == null) return;
        Id = other.Id;
        CreatedAt = other.CreatedAt;
        FirstName = other.FirstName;
        LastName = other.LastName;
        PasswordHash = other.PasswordHash;
        Email = other.Email;
        PhoneNumber = other.PhoneNumber;
        LanguagePreferance = other.LanguagePreferance;
        Theme = other.Theme;
        TwoFactorEnabled = other.TwoFactorEnabled;
        EmailNotifications = other.EmailNotifications;
    }
}
