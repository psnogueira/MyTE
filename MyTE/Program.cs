using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyTE.Data;
using MyTE.Models;
using MyTE.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Substitui AddDefaultIdentity por AddIdentity que é uma forma mais detalhada possibilitando habilitar o gerencimaneto de Roles e usa como base ApplicationUser em vez de IdentityUser
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultUI()
    .AddDefaultTokenProviders();

// Registra o Serviço CSVService para ser injetado em outras classes.
builder.Services.AddScoped<CSVService>();

// Define política para autorização apenas do perfil admin para acessar algumas funcionalidades
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequerPerfilAdmin",
        policy => policy.RequireRole("admin"));
    options.AddPolicy("Usuario",
        policy => policy.RequireRole("user", "admin"));
});

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Adicionar autenticação e autorização para as páginas.
app.UseAuthentication();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Records}/{action=Index}/{id?}");

app.MapRazorPages();

// Cria um escopo de serviço para inicializar o banco de dados e logar erros se ocorrerem durante a inicialização (DbInitializer).
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        DbInitializer.Initialize(services).Wait();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

app.Run();
