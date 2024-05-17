﻿using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MyTE.Models;

namespace MyTE.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<MyTE.Models.Department> Department { get; set; } = default!;
        public DbSet<MyTE.Models.WBS> WBS { get; set; } = default!;
        public DbSet<MyTE.Models.Employee> Employee { get; set; } = default!;
    }
}
