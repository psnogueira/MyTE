using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MyTE.Models;

namespace MyTE.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<MyTE.Models.Department> Department { get; set; } = default!;
        public DbSet<MyTE.Models.WBS> WBS { get; set; } = default!;
        public DbSet<MyTE.Models.Record> Record { get; set; } = default!;
        public DbSet<BiweeklyRecord> BiweeklyRecords { get; set; }
    }
}
