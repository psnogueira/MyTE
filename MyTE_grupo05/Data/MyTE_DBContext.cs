using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyTE_grupo05.Models;

namespace MyTE_grupo05.Data
{
    public class MyTE_DBContext : DbContext
    {
        public MyTE_DBContext (DbContextOptions<MyTE_DBContext> options)
            : base(options)
        {
        }

        public DbSet<MyTE_grupo05.Models.Department> Department { get; set; } = default!;
        public DbSet<MyTE_grupo05.Models.Login> Login { get; set; } = default!;
    }
}
