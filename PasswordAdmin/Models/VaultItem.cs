using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace PasswordAdmin.Models;

[Table("Vault Items")]
public class VaultItem : BaseModel
{
    [PrimaryKey("id")]
    public int Id { get; set; }
    [Column("created_at")]
    public string CreatedAt { get; set; } = string.Empty;
    [Column("user_id")]
    public int UserId { get; set; }
    [Column("encrypted_password")]
    public string EncryptedPassword { get; set; } = string.Empty;
    [Column("user_name")]
    public string? UserName { get; set; }

}