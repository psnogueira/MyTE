using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyTE.Data;
using System.Data;

namespace MyTE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AutoCompleteController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AutoCompleteController(ApplicationDbContext context)
        {
            _context = context;
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
                        { "WBS", new[] { "Desc", "Code" } },
                        { "Department", new[] { "Name", "" } },
                        { "Roles", new[] { "Name", "" } },
                        { "Users", new[] { "FirstName", "Email" } },
                        { "BiweeklyRecords", new[] { "UserEmail", "EmployeeName" } }  // Adicionei "EmployeeName" aqui
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
