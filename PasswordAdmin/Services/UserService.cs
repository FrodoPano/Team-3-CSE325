using Supabase;
using PasswordAdmin.Models;
using Microsoft.AspNetCore.Identity;

public class UserService
{
    private readonly Client _client;
    private readonly PasswordHasher<User> _hasher = new();
    private readonly List<User> _users = new();

    public UserService(Client client)
    {
        _client = client;
    }

    //this is an example of how to get a list of users 
    public async Task<List<User>> GetUsersInformation()
    {
        var result = await _client.From<User>().Get();
        return result.Models;
    }

    //this is an example of how to get a user 
    public async Task<User> GetSpecificUser(int id) //this is used for display purposes (delete this later if needed)
    {
        var result = await _client.From<User>().Where(u => u.Id == id).Single();
        return result;
    }

    // En memoria por ahora (después lo pasamos a DB si el equipo quiere)


    //task: implement supabase code to register a user
    public (bool ok, string message) Register(string email, string password)
    {
        email = (email ?? "").Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return (false, "Email and password are required.");

        if (_users.Any(u => u.Email == email))
            return (false, "That email is already registered.");

        var user = new User { Email = email };
        user.PasswordHash = _hasher.HashPassword(user, password);

        _users.Add(user);
        return (true, "Account created successfully.");
    }

    //task: implement supabase code to login a user
    public (bool ok, string message, User? user) Login(string email, string password)
    {
        email = (email ?? "").Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return (false, "Email and password are required.", null);

        var user = _users.FirstOrDefault(u => u.Email == email);
        if (user == null)
            return (false, "Invalid email or password.", null);

        var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, password);
        if (result == PasswordVerificationResult.Failed)
            return (false, "Invalid email or password.", null);

        return (true, "Login successful.", user);
    }

    public User? GetById(int id) => _users.FirstOrDefault(u => u.Id == id);
}