using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PasswordAdmin.Components;
using PasswordAdmin.Data;
using Supabase;
using DotNetEnv;

Env.Load();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

//sqlite configuration
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddAuthentication();
builder.Services.AddAuthorization();

var supabaseUrl = Environment.GetEnvironmentVariable("EXPO_PUBLIC_SUPABASE_URL"); //used ai
var supabaseKey = Environment.GetEnvironmentVariable("EXPO_PUBLIC_SUPABASE_KEY"); ;//used ai

var options = new SupabaseOptions//used ai
{
    AutoConnectRealtime = false//used ai
};//used ai

var client = new Supabase.Client(supabaseUrl, supabaseKey, options);//used ai
await client.InitializeAsync();//used ai

builder.Services.AddSingleton(client); // used ai 
builder.Services.AddScoped<UserService>(); // used ai 

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

app.MapPost("/account/logout", async (
    HttpContext http,
    SignInManager<IdentityUser> signInManager) =>
{
    await signInManager.SignOutAsync();
    return Results.Redirect("/");
});


// IMPORTANT: antiforgery AFTER auth
app.UseAntiforgery();


// ✅ Login endpoint (POST normal: aquí sí se setea cookie)
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
        return Results.Redirect("/");

    return Results.Redirect("/login?error=1");
});

app.MapPost("/account/register", async (
    HttpContext http,
    UserManager<IdentityUser> userManager) =>
{
    var form = await http.Request.ReadFormAsync();
    var email = form["email"].ToString();
    var password = form["password"].ToString();
    var confirm = form["confirmPassword"].ToString();

    if (password != confirm)
        return Results.Redirect("/register?error=pwdmatch");

    var user = new IdentityUser { UserName = email, Email = email };
    var result = await userManager.CreateAsync(user, password);

    if (result.Succeeded)
        return Results.Redirect("/login");

    return Results.Redirect("/register?error=1");
});



app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
