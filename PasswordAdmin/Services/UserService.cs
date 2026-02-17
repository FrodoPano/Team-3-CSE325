using Supabase;
using PasswordAdmin.Models;
using Microsoft.AspNetCore.Identity;

namespace PasswordAdmin.Services;

public class UserService
{
    private readonly Client _client;
    private readonly PasswordHasher<User> _hasher = new();

    public UserService(Client client)
    {
        _client = client;
    }



    // Get all users
    public async Task<List<User>> GetUsersInformation()
    {
        var result = await _client.From<User>().Get();
        return result.Models;
    }

    // Get specific user by ID
    public async Task<User?> GetSpecificUser(int id)
    {
        try
        {
            var result = await _client.From<User>().Where(u => u.Id == id).Get();
            return result.Models.FirstOrDefault();
        }
        catch
        {
            return null;
        }
    }

    // Get user by email
    public async Task<User?> GetUserByEmail(string email)
    {
        try
        {
            var result = await _client.From<User>().Where(u => u.Email == email).Get();
            return result.Models.FirstOrDefault();
        }
        catch
        {
            return null;
        }
    }

    // Register a new user in Supabase
    public async Task<(bool ok, string message, User? user)> Register(string email, string password, string firstName, string lastName)
    {
        email = (email ?? "").Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return (false, "Email and password are required.", null);

        // Check if user already exists
        var existingUser = await GetUserByEmail(email);
        if (existingUser != null)
            return (false, "That email is already registered.", null);

        var user = new User
        {
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            CreatedAt = DateTime.UtcNow,
            EmailNotifications = true,
            Theme = "Light",
            LanguagePreferance = "en",
            TwoFactorEnabled = false,
            PhoneNumber = null,
        };

        user.PasswordHash = _hasher.HashPassword(user, password);

        try
        {
            var result = await _client.From<User>().Insert(user);
            if (result.Models.Count > 0)
            {
                return (true, "Account created successfully.", result.Models.First());
            }
            return (false, "Failed to create account.", null);
        }
        catch (Exception ex)
        {
            return (false, $"Error: {ex.Message}", null);
        }
    }

    // Login user with Supabase
    public async Task<(bool ok, string message, User? user)> Login(string email, string password)
    {
        email = (email ?? "").Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return (false, "Email and password are required.", null);

        var user = await GetUserByEmail(email);
        if (user == null)
            return (false, "Invalid email or password.", null);

        var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, password);
        if (result == PasswordVerificationResult.Failed)
            return (false, "Invalid email or password.", null);

        return (true, "Login successful.", user);
    }

    // Update user
    public async Task<(bool ok, string message, User? user)> Update(User updatedUser)
    {
        try
        {
            var result = await _client.From<User>().Where(u => u.Id == updatedUser.Id).Update(updatedUser);
            if (result.Models.Count > 0)
            {
                return (true, "User updated successfully.", result.Models.First());
            }
            return (false, "Failed to update user.", null);
        }
        catch (Exception ex)
        {
            return (false, $"Error: {ex.Message}", null);
        }
    }
}