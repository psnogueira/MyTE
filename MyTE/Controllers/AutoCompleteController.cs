using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using MyTE.Data;
using System.Data;

namespace MyTE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AutoCompleteController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly RoleManager<IdentityRole> _manager;

        public AutoCompleteController(ApplicationDbContext context, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _manager = roleManager;
        }

        [Produces("application/json")]
        [HttpGet("identitySearch")]
        public IActionResult AutoComplete()
        {
            try
            {
                string term = HttpContext.Request.Query["term"].ToString();

                var roleName = _manager.Roles
                    .Where(r => r.Name.Contains(term) || r.Id.Contains(term))
                    .Select(r => r.Name)
                    .ToList();

                return Ok(roleName);

            }
            catch (Exception)
            {
                return BadRequest();
            }
        }

        [Produces("application/json")]
        [HttpGet("{tableName}/search")]
        public async Task<IActionResult> Search(string tableName, [FromQuery] string columnName1, [FromQuery] string columnName2 = null)
        {
            try
            {
                string term = HttpContext.Request.Query["term"].ToString();

                // Valida as tabelas e colunas que podem ser consultadas através do auto-complete
                var allowedTables = new Dictionary<string, string[]>
                {
                    { "WBS", new[] { "Code", "Desc" } },
                    { "Department", new[] { "Name", "" } }
                };

                if (!allowedTables.ContainsKey(tableName) ||
                    !allowedTables[tableName].Contains(columnName1) ||
                    (columnName2 != null && !allowedTables[tableName].Contains(columnName2)))
                {
                    return BadRequest("Tabela ou Coluna Inválda");
                }

                // Obtêm a propriedade do DbSet correspondente à tabela especificada 
                var dbSetProperty = _context.GetType().GetProperty(tableName);
                if (dbSetProperty == null)
                    return BadRequest("Tabela não encontrada");

                // Obtêm o valor da propriedade e converte para IQueryable para usarmos o LINQ
                var dbSet = dbSetProperty.GetValue(_context) as IQueryable<object>;
                if (dbSet == null)
                    return BadRequest("Tabela não encontrada");


                // Consulta dinâmica
                var query = dbSet.AsQueryable();

                if (columnName2 == null)
                {
                    query = query.Where(e => EF.Functions.Like(EF.Property<string>(e, columnName1), $"%{term}%"));
                }
                else
                {
                    query = query.Where(e => EF.Functions.Like(EF.Property<string>(e, columnName1), $"%{term}%") ||
                                              EF.Functions.Like(EF.Property<string>(e, columnName2), $"%{term}%"));
                }

                var items = await query
                    .Select(e => EF.Property<string>(e, columnName1))
                    .ToListAsync();

                return Ok(items);
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }
    }
}
