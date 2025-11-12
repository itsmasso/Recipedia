using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Recipedia.Data;
using Recipedia.Data.Services;
using Recipedia.Models;
using Rotativa.AspNetCore;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<RecipediaAppContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddHttpClient<IGeminiService, GeminiService>();
builder.Services.AddHttpClient<GoogleSearchEngineService>();
builder.Services.AddHttpClient<SpoonacularService>();

// Add Identity
builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    // Password settings (optional - adjust as needed)
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;

    // User settings
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<RecipediaAppContext>()
.AddDefaultTokenProviders();

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo("/keys")) 
    .SetApplicationName("Recipedia");

builder.Services.AddControllersWithViews();

// Configure Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie()
.AddGoogle(googleOptions =>
{
    googleOptions.ClientId = builder.Configuration["Authentication:Google:ClientId"];
    googleOptions.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
    googleOptions.SaveTokens = true;
});
// Configure application cookie
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
});
var isProd = builder.Environment.IsProduction();

if (isProd)
{   
    // Use port provided in env or 8080
    builder.WebHost.UseUrls("http://0.0.0.0:" + (Environment.GetEnvironmentVariable("PORT") ?? "8080"));
}


var app = builder.Build();
RotativaConfiguration.Setup(app.Environment.WebRootPath, "Rotativa/Windows");

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Home/Error");
	
	app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=FindRecipe}/{action=Index}/{id?}");

app.Run();
