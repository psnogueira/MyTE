using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MyTE.Data;
using MyTE.Models;
using MyTE.Models.ViewModel;
using MyTE.Pagination;

[Authorize(Roles = "admin")]
public class UsersController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ApplicationDbContext _context;

    public UsersController(UserManager<ApplicationUser> userManager, ApplicationDbContext context, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _context = context;
        _roleManager = roleManager;
    }

    public async Task<IActionResult> Index(string searchString, int? pageNumber, int? departmentType)
    {
        int pagesize = 5;
        ViewData["CurrentFilter"] = searchString;

        var usersQuery = _context.Users
            .Include(d => d.Department)
            .AsQueryable();

        if (!string.IsNullOrEmpty(searchString))
        {
            usersQuery = usersQuery.Where(s => s.FirstName.Contains(searchString));
        }

        if(departmentType.HasValue && departmentType != 0)
        {
            usersQuery = usersQuery.Where(d => d.DepartmentId == departmentType);
        }

        //var users = _userManager.Users
        //    .Include(u => u.Department)
        //    .ToList();
        var users = await PaginatedList<ApplicationUser>.CreateAsync(usersQuery.AsNoTracking(), pageNumber ?? 1, pagesize);
        var userRoles = new Dictionary<string, IList<string>>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            userRoles[user.Id] = roles;
        }

        var viewModel = new UserViewModel
        {
            UsersList = users,
            User = new ApplicationUser(),
            CurrentFilter = searchString,
            UserRoles = userRoles,
        };

        return View(viewModel);
    }

    public IActionResult Create()
    {
        ViewBag.Departments = new SelectList(_context.Department, "DepartmentId", "Name");
        ViewBag.Roles = new SelectList(_roleManager.Roles, "Id", "Name");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Email", "FirstName", "LastName", "HiringDate", "PID", "DepartmentId", "RoleId", "Password","ConfirmPassword")] CreateUserViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = new ApplicationUser
            {
                NormalizedUserName = model.Email,
                UserName = model.Email,
                Email = model.Email,
                EmailConfirmed = true,
                FirstName = model.FirstName,
                LastName = model.LastName,
                HiringDate = model.HiringDate,
                PID = model.PID,
                DepartmentId = model.DepartmentId,
                RoleId = model.RoleId
            };


            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                // Adiciona o usuário ao papel (role) especificado
                var role = await _roleManager.FindByIdAsync(model.RoleId);
                if (role != null)
                {
                    var roleResult = await _userManager.AddToRoleAsync(user, role.Name);
                    if (!roleResult.Succeeded)
                    {
                        foreach (var error in roleResult.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                    }
                }

                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }
        return View(model);
    }

    public async Task<IActionResult> Edit(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        ViewBag.Departments = new SelectList(_context.Department, "DepartmentId", "Name", user.DepartmentId);
        ViewBag.Roles = new SelectList(_roleManager.Roles, "Id", "Name", user.RoleId);

        var model = new EditUserViewModel
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            HiringDate = user.HiringDate,
            PID = user.PID,
            DepartmentId = user.DepartmentId,
            RoleId = user.RoleId
        };



        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(EditUserViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = await _userManager.FindByIdAsync(model.Id);
            if (user != null)
            {
                user.Email = model.Email;
                user.UserName = model.Email; // Certifique-se de que o UserName é atualizado também
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.HiringDate = model.HiringDate;
                user.PID = model.PID;
                user.DepartmentId = model.DepartmentId;
                user.RoleId = model.RoleId;

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    //Remover role anterior
                    var roles = await _userManager.GetRolesAsync(user);
                    foreach(var role in roles)
                    {
                        await _userManager.RemoveFromRoleAsync(user, role);
                    }
                         
                    // Adiciona o usuário ao papel (role) especificado
                    var newRole = await _roleManager.FindByIdAsync(model.RoleId);
                    if (newRole != null)
                    {
                        var roleResult = await _userManager.AddToRoleAsync(user, newRole.Name);
                        if (!roleResult.Succeeded)
                        {
                            foreach (var error in roleResult.Errors)
                            {
                                ModelState.AddModelError(string.Empty, error.Description);
                            }
                        }
                    }
                    return RedirectToAction(nameof(Index));
                }
            }
        }

        ViewBag.Departments = new SelectList(_context.Department, "DepartmentId", "Name", model.DepartmentId);
        ViewBag.Roles = new SelectList(_roleManager.Roles, "Id", "Name", model.RoleId);

        return View(model);
    }

    public async Task<IActionResult> Delete(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user != null)
        {
            await _userManager.DeleteAsync(user);
        }

        return RedirectToAction("Index");

    }
}
