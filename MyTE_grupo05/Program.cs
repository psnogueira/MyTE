using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyTE_grupo05.Data;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<MyTE_DBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MyTE_DBContext") ?? throw new InvalidOperationException("Connection string 'MyTE_DBContext' not found.")));

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
