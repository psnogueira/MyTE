using Microsoft.AspNetCore.Identity;
using MyTE.Models;

namespace MyTE.Data
{
    public class DbInitializer
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            // Verifica se a base de dados existe
            context.Database.EnsureCreated();

            // Cria os departamentos RH e TI caso não exista nenhum
            if (!context.Department.Any())
            {
                context.Department.AddRange(
                    new Department { Name = "RH" },
                    new Department { Name = "TI" }
                );
                await context.SaveChangesAsync();
            }

            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var config = serviceProvider.GetRequiredService<IConfiguration>();

            var roleAdmin = "admin";
            var roleUser = "user";

            // Cria um Role Admin caso nenhum exista
            if (!await roleManager.RoleExistsAsync(roleAdmin))
            {
                await roleManager.CreateAsync(new IdentityRole(roleAdmin));
            }

            // Cria um Role User caso nenhum exista
            if (!await roleManager.RoleExistsAsync(roleUser))
            {
                await roleManager.CreateAsync(new IdentityRole(roleUser));
            }

            var adminEmail = config["AdminCredentials:Email"];
            var adminPassword = config["AdminCredentials:Password"];

            var admin = await userManager.FindByEmailAsync(adminEmail);

            if (admin == null)
            {
                // Assume que IT é o primeiro departamento e seu ID é 1
                var departmentId = context.Department.FirstOrDefault(d => d.Name == "TI")?.DepartmentId ?? 1;

                admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    FirstName = "admin",
                    LastName = "admin",
                    DepartmentId = departmentId,
                    PID = "admin",
                    HiringDate = DateTime.Parse("2024-01-01")
                };
                var result = await userManager.CreateAsync(admin, adminPassword);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, roleAdmin);
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        Console.WriteLine(error.Description);
                    }
                }
            }
            else
            {
                if (!await userManager.IsInRoleAsync(admin, roleAdmin))
                {
                    await userManager.AddToRoleAsync(admin, roleAdmin);
                }
            }
        }
    }
}
