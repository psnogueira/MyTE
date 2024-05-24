using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using MyTE.Data;
using MyTE.Data.Migrations;
using MyTE.Models;
using MyTE.Models.Enum;
using MyTE.Models.ViewModel;
using MyTE.Pagination;
using System.Security.Cryptography;

namespace MyTE.Controllers
{
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _manager;
        private readonly RoleManager<IdentityRole> _roleManager;

        private readonly ApplicationDbContext _context;

        public UsersController(UserManager<ApplicationUser> userManeger, SignInManager<ApplicationUser> signInManager, ApplicationDbContext context)
        {
            _manager = userManeger;
            _context = context;
        }

        public async Task<IActionResult> Index(string searchString, int? pageNumber, int? departmentType)
        {
            int pageSize = 7;
            ViewData["CurrentFilter"] = searchString;

            var usersQuery = _context.Users.Include(u => u.Department).AsQueryable();


            if (departmentType.HasValue && departmentType != 0)
            {
                usersQuery = usersQuery.Where(u => u.DepartmentId == departmentType);
            }

            if (!string.IsNullOrEmpty(searchString))
            {
                usersQuery = usersQuery.Where(u => EF.Functions.Like(u.FirstName, $"%{searchString}%"));
            }

            var users = await PaginatedList<ApplicationUser>.CreateAsync(usersQuery.AsNoTracking(), pageNumber ?? 1, pageSize);
            var userRoles = new Dictionary<string, IList<string>>();

            foreach (var user in users)
            {
                var roles = await _manager.GetRolesAsync(user);
                userRoles[user.Id] = roles;
            }

            var viewModel = new EditUserViewModel
            {
                UserList = users,
                User = new ApplicationUser(),
                CurrentFilter = searchString,
                UserRoles = userRoles
            };
            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = _manager.Users.FirstOrDefault(u => u.Id == id);
            
            if (user == null)
            {
                return NotFound();
            }

            var viewModel = new EditUserViewModel
            {
                UserName = user.UserName,
                FirstName = user.FirstName,
                LastName = user.LastName,
                HiringDate = user.HiringDate,
                Email = user.Email,
                PID = user.PID,
                Department = user.Department,
                Departments = await _context.Department
                        .Select(d => new SelectListItem
                        {
                            Value = d.DepartmentId.ToString(),
                            Text = d.Name
                        }).ToListAsync(),

                RolesList = await _context.Roles
                        .Select(d => new SelectListItem
                        {
                            Value = d.Id.ToString(),
                            Text = d.Name
                        }).ToListAsync(),

            };



            return View(viewModel);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, EditUserViewModel user)
        {
            if (id != user.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingUser = await _manager.FindByIdAsync(id);
                    if (existingUser == null)
                    {
                        return NotFound();
                    }

                    existingUser.UserName = user.UserName;
                    existingUser.FirstName = user.FirstName;
                    existingUser.LastName = user.LastName;
                    existingUser.HiringDate = user.HiringDate;
                    existingUser.Email = user.Email;
                    existingUser.PID = user.PID;
                    existingUser.Department = user.Department;

                    await _manager.UpdateAsync(existingUser);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (_manager.Users.FirstOrDefault(u => u.Id == id) == null)
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Index");
            }
            return View(user);
        }

        public async Task<IActionResult> Delete(string id)
        {
            var user = await _manager.FindByIdAsync(id);
            if (user != null)
            {
                _manager.DeleteAsync(user);
            }

            return RedirectToAction("Index");

        }


        //public async Task<IActionResult> Edit(string id)
        //{
        //    var user = await _manager.FindByIdAsync(id);
        //    if (user == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(user);
        //}
    }
}
