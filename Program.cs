using CarvedRockFitness.Components;
using CarvedRockFitness.Services;
using CarvedRockFitness.Repositories;
using Microsoft.Extensions.Logging.AzureAppServices;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.Name = "CarvedRockFitness.Session";
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Logging.AddAzureWebAppDiagnostics();

// Register ICartRepository based on connection string, or use in-memory cart
string connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? Environment.GetEnvironmentVariable("DefaultConnection");
if (!string.IsNullOrEmpty(connectionString))
{
    builder.Services.AddScoped<ICartRepository, SqlCartRepository>();
}
else
{
    builder.Services.AddScoped<ICartRepository, InMemoryCartRepository>();
}

builder.Services.AddScoped<ShoppingCartService>();
builder.Services.AddSingleton<CartEventService>();

builder.Services.AddScoped<IProductRepository, ProductRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAntiforgery();


app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();