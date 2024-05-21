using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using MyTE.Data;
using MyTE.Data.Migrations;
using MyTE.Models;
using MyTE.Models.ViewModel;
using System.Security.Cryptography;

namespace MyTE.Controllers
{
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _manager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _context;

        public UsersController(UserManager<ApplicationUser> userManeger, SignInManager<ApplicationUser> signInManager, ApplicationDbContext context)
        {
            _manager = userManeger;
            _signInManager = signInManager;
            _context = context;
        }
        public async Task<IActionResult> Index(string searchString)
        {
            ViewData["CurrentFilter"] = searchString;

            var users = _manager.Users.Include(u => u.Department).ToList();

            var userViewModels = new List<UserViewModel>();
            foreach (var user in users)
            {
                var fullName = $"{user.FirstName} {user.LastName}";
                var userViewModel = new UserViewModel
                {
                    Id = user.Id,
                    FullName = fullName,
                    UserName = user.UserName,
                    Email = user.Email,
                    PID = user.PID,
                    HireDate = user.HiringDate,
                    Department = user.Department != null ? user.Department.Name : "Unknown",
                };
                userViewModels.Add(userViewModel);
            }

            return View(userViewModels);
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

            var model = new EditUserViewModel
            {
                UserName = user.UserName,
                FirstName = user.FirstName,
                LastName = user.LastName,
                HiringDate = user.HiringDate,
                Password = user.PasswordHash,
                Email = user.Email,
                PID = user.PID,
                Department = user.Department,
                Departments = await _context.Department
                    .Select(d => new SelectListItem
                    {
                        Value = d.DepartmentId.ToString(),
                        Text = d.Name
                    }).ToListAsync(),
            };

            return View(model);

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
                    existingUser.PasswordHash = user.Password;
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
    }
}
