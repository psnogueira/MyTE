using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace MyTE.Controllers
{
    [Authorize(Policy = "RequerPerfilAdmin")]
    public class RolesController : Controller
    {
        private readonly RoleManager<IdentityRole> _manager;

        public RolesController(RoleManager<IdentityRole> roleManager)
        {
            _manager = roleManager;
        }

        public async Task<IActionResult> Index(string searchString)
        {
            ViewData["CurrentFilter"] = searchString;
            var roles = from r in _manager.Roles select r;
            if (!String.IsNullOrEmpty(searchString))
            {
                roles = roles.Where(r => r.Id.Contains(searchString) || r.Name.Contains(searchString));
            }

            return View(await roles.ToListAsync());
        }

        [Produces("application/json")]
        [HttpGet("search")]
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

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([Bind("Id", "Name")] IdentityRole role)
        {
            // Verificar se o Nível de Acesso já existe
            if (!_manager.RoleExistsAsync(role.Name).GetAwaiter().GetResult())
            {
                _manager.CreateAsync(new IdentityRole(role.Name)).GetAwaiter().GetResult();
                TempData["SuccessMessage"] = "Role criada com sucesso!";
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var role = await _manager.FindByIdAsync(id);
            if (role == null)
            {
                return NotFound();
            }

            return View(role);
        }
        
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var role = await _manager.FindByIdAsync(id);
            if (role == null)
            {
                return NotFound();
            }
            TempData["SuccessMessage2"] = "Role editada com sucesso!";
            return View(role);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Id", "Name")] IdentityRole role)
        {
            if (id != role.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingRole = await _manager.FindByIdAsync(id);
                    existingRole.Name = role.Name;
                    await _manager.UpdateAsync(existingRole);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_manager.RoleExistsAsync(id).GetAwaiter().GetResult())
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(role);
        }

        public async Task<IActionResult> Delete(string id)
        {
            var role = await _manager.FindByIdAsync(id);
            if (role != null)
            {
                await _manager.DeleteAsync(role);
            }

            return RedirectToAction("Index");
        }

    }
}