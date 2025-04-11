using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TodoApp.Components;
using TodoApp.Components.Account;
using TodoApp.Data;
using TodoApp.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();

builder.Services.AddHttpClient<EncryptionService>(client =>
{
    var baseAddress = builder.Configuration["BaseAddress"];
    Console.WriteLine($"Configured BaseAddress: {baseAddress}");
    if (string.IsNullOrEmpty(baseAddress))
    {
        throw new InvalidOperationException("BaseAddress is not configured in appsettings.json.");
    }
    client.BaseAddress = new Uri(baseAddress);
});

builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();
builder.Services.AddScoped<CprValidationService>();
builder.Services.AddScoped<SymetricEncryptionService>();
builder.Services.AddScoped<AsymetricEncryption>();

builder.Services.AddControllers();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddIdentityCookies();

var identityDbConnectionString = builder.Configuration.GetConnectionString("KestrelIdendityDb") ?? throw new InvalidOperationException("Connection string 'KestrelIdendityDb' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(identityDbConnectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

var toDoDbConnectionString = builder.Configuration.GetConnectionString("KestrelTodoDb") ?? throw new InvalidOperationException("Connection string 'KestrelTodoDb' not found.");
builder.Services.AddDbContext<TodoDbContext>(options =>
    options.UseSqlServer(toDoDbConnectionString));


builder.Services.AddIdentityCore<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;
    options.Password.RequiredLength = 8;
})
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("RequireAuthenticatedUser", policy =>
    {
        policy.RequireAuthenticatedUser();
    })
    .AddPolicy("RequireAdmin", policy =>
    {
        policy.RequireRole("Admin");
    });

builder.WebHost.UseKestrel(options =>
{
    options.Configure(builder.Configuration.GetSection("Kestrel"));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();
app.MapControllers();

app.Run();
