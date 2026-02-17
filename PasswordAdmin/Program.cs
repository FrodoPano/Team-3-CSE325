using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PasswordAdmin.Components;
using PasswordAdmin.Data;
using PasswordAdmin.Models;
using PasswordAdmin.Services;
using Supabase;
using DotNetEnv;

// Load environment variables from .env file
Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddHttpContextAccessor();
builder.Services.AddCascadingAuthenticationState();

// SQLite database context for Identity
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity configuration
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    
    // Configure password requirements
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>();

// Supabase configuration
var supabaseUrl = Environment.GetEnvironmentVariable("EXPO_PUBLIC_SUPABASE_URL");
var supabaseKey = Environment.GetEnvironmentVariable("EXPO_PUBLIC_SUPABASE_KEY");

// Check if Supabase credentials are configured
if (string.IsNullOrEmpty(supabaseUrl) || string.IsNullOrEmpty(supabaseKey))
{
    throw new Exception("Supabase credentials not found in environment variables. Please check your .env file.");
}

var options = new SupabaseOptions
{
    AutoConnectRealtime = false
};

var client = new Supabase.Client(supabaseUrl, supabaseKey, options);
await client.InitializeAsync();

builder.Services.AddSingleton(client);
builder.Services.AddScoped<UserService>();

builder.Services.AddAuthentication();
builder.Services.AddAuthorization();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

// IMPORTANT: antiforgery AFTER auth
app.UseAntiforgery();

// ✅ LOGIN ENDPOINT - Using Identity
app.MapPost("/account/login", async (
    HttpContext http,
    SignInManager<IdentityUser> signInManager) =>
{
    var form = await http.Request.ReadFormAsync();
    var email = form["email"].ToString();
    var password = form["password"].ToString();
    var remember = form["rememberMe"] == "on" || form["rememberMe"] == "true";

    var result = await signInManager.PasswordSignInAsync(email, password, remember, lockoutOnFailure: false);

    if (result.Succeeded)
        return Results.Redirect("/account");

    return Results.Redirect("/login?error=1");
});

// ✅ REGISTER ENDPOINT - Using Identity and Supabase
app.MapPost("/account/register", async (
    HttpContext http,
    UserManager<IdentityUser> userManager,
    SignInManager<IdentityUser> signInManager,
    UserService userService) =>
{
    var form = await http.Request.ReadFormAsync();
    var email = form["email"].ToString();
    var password = form["password"].ToString();
    var confirm = form["confirmPassword"].ToString();
    var firstName = form["firstName"].ToString() ?? email.Split('@')[0];
    var lastName = form["lastName"].ToString() ?? "User";

    if (password != confirm)
        return Results.Redirect("/register?error=pwdmatch");

    // Create Identity user
    var identityUser = new IdentityUser { UserName = email, Email = email };
    var identityResult = await userManager.CreateAsync(identityUser, password);

    if (identityResult.Succeeded)
    {
        // Create user in Supabase
        var supabaseResult = await userService.Register(email, password, firstName, lastName);
        
        if (supabaseResult.ok && supabaseResult.user != null)
        {
            // Sign in the user
            await signInManager.SignInAsync(identityUser, isPersistent: false);
            return Results.Redirect("/account");
        }
        else
        {
            // Rollback Identity user if Supabase creation failed
            await userManager.DeleteAsync(identityUser);
            return Results.Redirect("/register?error=1");
        }
    }

    // Handle specific errors
    var error = identityResult.Errors.FirstOrDefault();
    if (error != null)
    {
        if (error.Code.Contains("DuplicateUserName"))
            return Results.Redirect("/register?error=exists");
        if (error.Code.Contains("PasswordTooShort"))
            return Results.Redirect("/register?error=passwordlength");
        if (error.Code.Contains("PasswordRequiresDigit"))
            return Results.Redirect("/register?error=passworddigit");
        if (error.Code.Contains("PasswordRequiresUpper"))
            return Results.Redirect("/register?error=passwordupper");
    }

    return Results.Redirect("/register?error=1");
});

// ✅ LOGOUT ENDPOINT
app.MapPost("/account/logout", async (
    HttpContext http,
    SignInManager<IdentityUser> signInManager) =>
{
    await signInManager.SignOutAsync();
    return Results.Redirect("/");
});

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();