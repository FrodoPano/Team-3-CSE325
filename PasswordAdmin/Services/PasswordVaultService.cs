using Supabase;
using PasswordAdmin.Models;

namespace PasswordAdmin.Services;

public class PasswordVaultService
{
    private readonly Client _client;
    private readonly UserService _userService;

    public PasswordVaultService(Client client, UserService userService)
    {
        _client = client;
        _userService = userService;
    }

    public async Task<List<VaultEntry>> GetItems(string ownerEmail)
    {
        try
        {
            ownerEmail ??= string.Empty;
            var result = await _client.From<VaultItem>()
                .Where(x => x.UserName == ownerEmail)
                .Get();

            return result.Models.Select(item => new VaultEntry
            {
                Id = item.Id,
                CreatedAt = item.CreatedAt,
                Title = item.Title,
                Password = item.EncryptedPassword,
                Description = item.Description
            }).ToList();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"PasswordVaultService.GetItems error: {ex.Message}");
            return new List<VaultEntry>();
        }
    }

    public async Task<VaultEntry> AddItem(string ownerEmail, VaultEntry item)
    {
        try
        {
            ownerEmail ??= string.Empty;
            System.Diagnostics.Debug.WriteLine($"PasswordVaultService.AddItem called: owner={ownerEmail} Title='{item.Title}' Password='{item.Password}' Description='{item.Description}'");
            
            // Get the user ID from the Users table
            var user = await _userService.GetUserByEmail(ownerEmail);
            var userId = user?.Id ?? 0;
            
            if (userId == 0)
            {
                System.Diagnostics.Debug.WriteLine($"PasswordVaultService.AddItem error: User not found for email {ownerEmail}");
                return item;
            }

            var newItem = new VaultItem
            {
                UserId = userId,
                UserName = ownerEmail,
                EncryptedPassword = item.Password,
                Title = item.Title,
                Description = item.Description,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _client.From<VaultItem>().Insert(newItem);
            if (result.Models.Count > 0)
            {
                var saved = result.Models[0];
                item.Id = saved.Id;
                item.CreatedAt = saved.CreatedAt;
                System.Diagnostics.Debug.WriteLine($"PasswordVaultService.AddItem saved id={item.Id}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"PasswordVaultService.AddItem error: {ex.Message}");
        }
        return item;
    }

    public async Task<VaultEntry?> UpdateItem(string ownerEmail, VaultEntry item)
    {
        try
        {
            ownerEmail ??= string.Empty;
            
            // Get the user ID from the Users table
            var user = await _userService.GetUserByEmail(ownerEmail);
            var userId = user?.Id ?? 0;
            
            if (userId == 0)
            {
                System.Diagnostics.Debug.WriteLine($"PasswordVaultService.UpdateItem error: User not found for email {ownerEmail}");
                return null;
            }

            var updated = new VaultItem
            {
                Id = item.Id,
                UserId = userId,
                UserName = ownerEmail,
                EncryptedPassword = item.Password,
                Title = item.Title,
                Description = item.Description,
                CreatedAt = item.CreatedAt
            };

            var result = await _client.From<VaultItem>()
                .Where(x => x.Id == item.Id)
                .Update(updated);

            System.Diagnostics.Debug.WriteLine($"PasswordVaultService.UpdateItem called: owner={ownerEmail} id={item.Id} Title='{item.Title}'");

            return result.Models.Count > 0 ? item : null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"PasswordVaultService.UpdateItem error: {ex.Message}");
            return null;
        }
    }

    public async Task<bool> DeleteItem(string ownerEmail, int id)
    {
        try
        {
            await _client.From<VaultItem>().Where(x => x.Id == id).Delete();
            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"PasswordVaultService.DeleteItem error: {ex.Message}");
            return false;
        }
    }

    public class VaultEntry
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool ShowPlain { get; set; } = false;

        public VaultEntry Clone() => new VaultEntry
        {
            Id = Id,
            CreatedAt = CreatedAt,
            Title = Title,
            Password = Password,
            Description = Description,
            ShowPlain = ShowPlain
        };
    }
}
